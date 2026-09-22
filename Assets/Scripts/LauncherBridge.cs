using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Racer
{
    // Optional launcher arguments; ordinary portable saves and radio preferences stay unchanged.
    public sealed class LauncherBridge : MonoBehaviour
    {
        public static string Argument(string key)
        {var args=Environment.GetCommandLineArgs();for(int i=1;i+1<args.Length;i++)if(args[i]==key)return args[i+1];return null;}
        public static string ManagedMusic=>Argument("-racerManagedMusic");
        public static string PersonalMusic=>Argument("-racerPersonalMusic");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartBridge()
        {if(Argument("-racerLauncherReady")==null)return;var go=new GameObject("Launcher startup handshake");DontDestroyOnLoad(go);go.AddComponent<LauncherBridge>();}
        IEnumerator Start()
        {
            var path=Argument("-racerLauncherReady");var nonce=Argument("-racerLauncherNonce");
            if(string.IsNullOrEmpty(nonce)||nonce.Length>128||!Path.IsPathRooted(path)){Destroy(gameObject);yield break;}
            RaceFlow flow=null;float end=Time.realtimeSinceStartup+90;
            while(Time.realtimeSinceStartup<end)
            {flow=FindFirstObjectByType<RaceFlow>();if(flow&&flow.Save!=null)break;yield return null;}
            if(flow&&flow.Save!=null)
            {
                yield return new WaitForEndOfFrame();
                try{Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path+".tmp",nonce);File.Move(path+".tmp",path);}
                catch(Exception e){Debug.LogWarning("Launcher ready signal unavailable: "+e.Message);}
                if(Argument("-racerTestSave")!=null)
                {
                    float scanEnd=Time.realtimeSinceStartup+35;
                    while(Time.realtimeSinceStartup<scanEnd&&(!flow.Radio||flow.Radio.Scanning))yield return null;
                    if(flow.Radio)Debug.Log("LAUNCHER_DISCOVERY "+JsonUtility.ToJson(new Discovery{save=flow.Save.DirectoryPath,managed=ManagedMusic,personal=PersonalMusic,tracks=flow.Radio.Count,stations=flow.Radio.ChannelNames}));
                }
            }
            Destroy(gameObject);
        }
        [Serializable] sealed class Discovery { public string save,managed,personal;public int tracks;public string[] stations; }
    }
}
