using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

namespace Racer
{
    // Local files only. One streaming clip, one cancellable request, bounded path/history lists.
    public sealed class LocalRadio : MonoBehaviour
    {
        RaceFlow flow;
        AudioSource source;
        AudioClip clip;
        UnityWebRequest request;
        Coroutine loading;
        Task<string[]> scan;
        Task<string> metadata, picker;
        readonly List<string> library=new(), bag=new(), history=new();
        readonly HashSet<string> failed=new(StringComparer.OrdinalIgnoreCase);
        readonly System.Random random=new();
        string metadataPath;
        string current, lastPlayed, song="Radio: add MP3, WAV or Ogg files in Settings / Music", scanFolder;
        float toastUntil, retryAt;
        int revision;
        public string Status { get; private set; }="Empty library";
        public string Song=>song;
        public string Toast=>Time.unscaledTime<toastUntil?song:null;
        public string Folder=>flow.Save.Settings.musicFolder;
        public int Count=>library.Count;
        public bool Loading=>loading!=null;
        public bool Playing=>source && source.isPlaying;
        public void Initialize(RaceFlow owner)
        {
            flow=owner;
            source=gameObject.AddComponent<AudioSource>(); source.playOnAwake=false;
            source.spatialBlend=0; source.ignoreListenerPause=true; source.priority=180;
            if(string.IsNullOrWhiteSpace(Folder)) flow.Save.Settings.musicFolder=Path.Combine(Application.persistentDataPath,"Music");
            EnsureDefault(); Rescan();
        }
        void EnsureDefault()
        {
            try
            {
                if(Folder!=Path.Combine(Application.persistentDataPath,"Music"))return;
                Directory.CreateDirectory(Folder);
                var readme=Path.Combine(Folder,"README-Racer.txt");
                if(!File.Exists(readme))File.WriteAllText(readme,"Add your own DRM-free MP3, PCM WAV or Ogg Vorbis files here, then choose Settings / Music / Rescan. No songs are included. Subfolders are not scanned. Files remain local and are never modified. Gameplay: D-pad right/left/up/down = next/previous/show/toggle; keyboard ] / [ / I / M. Music continues through pause, menus and race restart; toggle it off independently. Limits: 2048 files, MP3/Ogg 256 MiB, PCM WAV 64 MiB. See RADIO.md in the game folder.");
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException){Status="Music folder unavailable; choose another folder";}
        }
        public static bool LocalFolder(string path)=>!string.IsNullOrWhiteSpace(path)&&Path.IsPathRooted(path)&&!path.StartsWith(@"\\")&&!path.Contains("://");
        public void Rescan()
        {
            if(scan!=null)return;
            var folder=Folder; scanFolder=folder;
            if(!LocalFolder(folder)){Status="Choose a local folder";return;}
            Status="Scanning…";
            scan=Task.Run(()=>
            {
                try { return Directory.EnumerateFiles(folder).Where(p=>Type(p)!=AudioType.UNKNOWN).Take(2048).ToArray(); }
                catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException){return Array.Empty<string>();}
            });
        }
        public void SetFolder(string path)
        {
            if(!LocalFolder(path)){Status="Choose a local folder";return;}
            StopLoad(); source.Stop(); if(clip)Destroy(clip);clip=null;current=null;
            history.Clear(); library.Clear();bag.Clear();failed.Clear();lastPlayed=null;
            flow.Save.Settings.musicFolder=Path.GetFullPath(path);flow.Save.SaveSettings();
            // An older scan is allowed to finish, but never installs results from the old folder.
            if(scan==null)Rescan();
        }
        public void OpenFolder()
        {
            try { if(Directory.Exists(Folder))System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Folder){UseShellExecute=true}); else Status="Folder missing; choose another folder"; }
            catch(Exception){Status="Could not open Music folder";}
        }
        public void ChooseFolder()
        {
            if(picker!=null)return;
            var promise=new TaskCompletionSource<string>();picker=promise.Task;
            var thread=new Thread(()=>{try{promise.SetResult(WindowsFolder.Pick());}catch{promise.SetResult(null);}});
            thread.IsBackground=true;thread.SetApartmentState(ApartmentState.STA);thread.Start();
        }
        public void Toggle()
        {
            flow.Save.Settings.radioOn=!flow.Save.Settings.radioOn;flow.Save.SaveSettings();
            if(!flow.Save.Settings.radioOn)source.Pause();
            else if(clip){source.UnPause();if(!source.isPlaying)source.Play();}else Next();
            ShowSong();
        }
        public void ShowSong(){toastUntil=Time.unscaledTime+5; if(!clip&&loading==null)song=Status;}
        public void Next()
        {
            if(library.Count==0){Status="No playable files. Settings / Music / Open folder, then Rescan";ShowSong();return;}
            if(bag.Count==0)
            {
                bag.AddRange(library.Where(p=>!failed.Contains(p)));
                for(int i=bag.Count-1;i>0;i--){int j=random.Next(i+1);(bag[i],bag[j])=(bag[j],bag[i]);}
                if(bag.Count>1&&bag[^1]==lastPlayed)(bag[0],bag[^1])=(bag[^1],bag[0]);
            }
            if(bag.Count==0){Status="No readable audio. Check files and Rescan";retryAt=float.PositiveInfinity;ShowSong();return;}
            if(bag[^1]==lastPlayed&&library.Count(p=>!failed.Contains(p))>1)
            {bag.RemoveAt(bag.Count-1);if(bag.Count==0){Next();return;}}
            var path=bag[^1];bag.RemoveAt(bag.Count-1);Play(path,true);
        }
        public void Previous()
        {
            while(history.Count>0){var path=history[^1];history.RemoveAt(history.Count-1);if(library.Contains(path)&&!failed.Contains(path)){Play(path,false);return;}}
            ShowSong();
        }
        void Play(string path,bool remember)
        {
            if(remember&&current!=null&&clip){if(history.Count==32)history.RemoveAt(0);history.Add(current);}
            StopLoad();source.Stop();if(clip)Destroy(clip);clip=null;
            current=path;Status="Loading…";song=Plain(Path.GetFileNameWithoutExtension(path));
            if(metadata==null)metadataPath=null;

            loading=StartCoroutine(Load(path,revision));
        }
        IEnumerator Load(string path,int token)
        {
            yield return null;
            bool valid=false;
            try{var info=new FileInfo(path);valid=info.Exists&&info.Length>0&&info.Length<=(Type(path)==AudioType.WAV?64L:256L)*1024*1024;}catch{}
            if(!valid){Fail(path);yield break;}
            request=UnityWebRequestMultimedia.GetAudioClip(new Uri(path).AbsoluteUri,Type(path));request.timeout=15;
            ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio=true;
            yield return request.SendWebRequest();
            if(token!=revision)yield break;
            if(request.result!=UnityWebRequest.Result.Success){request.Dispose();request=null;Fail(path);yield break;}
            clip=DownloadHandlerAudioClip.GetContent(request);request.Dispose();request=null;
            if(!clip||clip.length<=0){Fail(path);yield break;}
            StartClip();
        }
        void StartClip()
        {
            lastPlayed=current;
            source.clip=clip;source.volume=flow.Save.Settings.music*.32f;
            if(flow.Save.Settings.radioOn)source.Play();
            retryAt=Time.unscaledTime+1.5f; // Give a newly opened streaming voice time to start.
            loading=null;Status="Ready / "+library.Count+" tracks";toastUntil=Time.unscaledTime+5;
        }
        void Fail(string path){failed.Add(path);loading=null;Status="Skipped unreadable or oversized audio";song=Status;toastUntil=Time.unscaledTime+4;retryAt=Time.unscaledTime+.5f;}
        void StopLoad(){revision++;if(request!=null){request.Abort();request.Dispose();request=null;}if(loading!=null){StopCoroutine(loading);loading=null;}}
        void Update()
        {
            if(flow?.Save==null)return;
            source.volume=flow.Save.Settings.music*.32f;
            if(scan!=null&&scan.IsCompleted)
            {
                var result=scan.IsCompletedSuccessfully?scan.Result:Array.Empty<string>();scan=null;
                if(scanFolder!=Folder){Rescan();return;}
                library.Clear();library.AddRange(result);bag.Clear();failed.Clear();retryAt=0;
                Status=library.Count==0?"No audio found. Open Music folder, add songs, then Rescan":library.Count+" tracks";
                if(current!=null&&!library.Contains(current)){StopLoad();source.Stop();if(clip)Destroy(clip);clip=null;current=null;}
                if(!clip&&loading==null&&flow.Save.Settings.radioOn&&library.Count>0)Next();
            }
            if(metadata!=null&&metadata.IsCompleted){if(metadata.IsCompletedSuccessfully&&clip&&metadataPath==current){song=metadata.Result;toastUntil=Time.unscaledTime+5;}metadata=null;}
            if(metadata==null&&clip&&metadataPath!=current){metadataPath=current;var path=current;metadata=Task.Run(()=>ReadTitle(path));}
            if(picker!=null&&picker.IsCompleted){var path=picker.Result;picker=null;if(path!=null)SetFolder(path);}
            if(flow.Save.Settings.radioOn&&loading==null&&!source.isPlaying&&library.Count>0&&Time.unscaledTime>=retryAt){retryAt=Time.unscaledTime+1;Next();}
            if(flow.State!=RaceFlow.Stage.Racing)return;
            var k=Keyboard.current;var g=Gamepad.current;
            if(k?.rightBracketKey.wasPressedThisFrame==true||g?.dpad.right.wasPressedThisFrame==true)Next();
            else if(k?.leftBracketKey.wasPressedThisFrame==true||g?.dpad.left.wasPressedThisFrame==true)Previous();
            if(k?.iKey.wasPressedThisFrame==true||g?.dpad.up.wasPressedThisFrame==true)ShowSong();
            if(k?.mKey.wasPressedThisFrame==true||g?.dpad.down.wasPressedThisFrame==true)Toggle();
        }
        static AudioType Type(string p)=>Path.GetExtension(p).ToLowerInvariant() switch {".mp3"=>AudioType.MPEG,".wav"=>AudioType.WAV,".ogg"=>AudioType.OGGVORBIS,_=>AudioType.UNKNOWN};
        static string Plain(string value)=>new string((value??"").Where(c=>!char.IsControl(c)).Take(120).ToArray());
        static string ReadTitle(string path)
        {
            try
            {
                using var stream=new MetadataStream(path);
                ATL.Settings.ReadAllMetaFrames=false;
                var track=new ATL.Track(stream,Path.GetExtension(path));
                string title=string.IsNullOrWhiteSpace(track.Title)?Path.GetFileNameWithoutExtension(path):track.Title;
                return Plain(string.IsNullOrWhiteSpace(track.Artist)?title:track.Artist+" — "+title);
            }
            catch {return Plain(Path.GetFileNameWithoutExtension(path));}
        }
        sealed class MetadataStream:FileStream
        {
            int remaining=4*1024*1024;
            public override int ReadByte(){if(--remaining<0)throw new IOException("Metadata read budget");return base.ReadByte();}
            public MetadataStream(string path):base(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete){}
            public override int Read(byte[] b,int o,int c){if((remaining-=c)<0)throw new IOException("Metadata read budget");return base.Read(b,o,c);}
            public override int Read(Span<byte> b){if((remaining-=b.Length)<0)throw new IOException("Metadata read budget");return base.Read(b);}
        }
        void OnDestroy(){StopLoad();if(clip)Destroy(clip);}
    }
    static class WindowsFolder
    {
        [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
        struct BrowseInfo {public IntPtr owner,root,display;[MarshalAs(UnmanagedType.LPWStr)]public string title;public uint flags;public IntPtr callback,param;public int image;}
        [DllImport("shell32.dll",CharSet=CharSet.Unicode)]static extern IntPtr SHBrowseForFolderW(ref BrowseInfo info);
        [DllImport("shell32.dll",CharSet=CharSet.Unicode)]static extern bool SHGetPathFromIDListW(IntPtr id,System.Text.StringBuilder path);
        public static string Pick()
        {
            var display=Marshal.AllocHGlobal(520);
            try {var info=new BrowseInfo{display=display,title="Choose your local Racer music folder",flags=0x41};var id=SHBrowseForFolderW(ref info);if(id==IntPtr.Zero)return null;
                try{var path=new System.Text.StringBuilder(260);return SHGetPathFromIDListW(id,path)?path.ToString():null;}finally{Marshal.FreeCoTaskMem(id);}}
            finally{Marshal.FreeHGlobal(display);}
        }
    }
}
