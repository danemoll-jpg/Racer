using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Racer
{
    // Path discovery only: no metadata or audio decoding during collection scans.
    public static class MusicCollection
    {
        public const int MaxTracks=100000, MaxEntries=250000, MaxDirectories=20000;
        public sealed class Result
        {
            public readonly List<string> Paths=new();
            public int Entries, Folders, Unsupported, Links, Inaccessible, Oversized;
            public bool Limited, Cancelled;
            public string Error;
            public string Summary => $"{Paths.Count:N0} tracks · skipped {Unsupported:N0} other, {Links:N0} links, {Inaccessible:N0} inaccessible, {Oversized:N0} size"
                +(Cancelled?" · cancelled":Limited?" · LIMIT reached; unvisited files not counted. Choose a smaller root.":Error!=null?" · "+Error:"");
        }
        public sealed class Progress { public int Tracks, Entries, Folders; }
        public static bool Supported(string path)
        {var ext=Path.GetExtension(path);return ext.Equals(".mp3",StringComparison.OrdinalIgnoreCase)||ext.Equals(".wav",StringComparison.OrdinalIgnoreCase)||ext.Equals(".ogg",StringComparison.OrdinalIgnoreCase);}
        public static Result Scan(string root,bool recursive,CancellationToken cancellation,Progress progress=null)
        {
            var result=new Result();var clock=Stopwatch.StartNew();
            var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);var pending=new Stack<string>();
            try { pending.Push(Path.GetFullPath(root)); }
            catch(Exception e) when(e is ArgumentException||e is NotSupportedException){result.Error="Invalid folder";return result;}
            bool Stop()
            {
                if(cancellation.IsCancellationRequested){result.Cancelled=true;return true;}
                if(result.Paths.Count>=MaxTracks||result.Entries>=MaxEntries||result.Folders>=MaxDirectories||clock.Elapsed.TotalSeconds>=30){result.Limited=true;return true;}
                return false;
            }
            while(pending.Count>0&&!Stop())
            {
                string dir=pending.Pop();if(!seen.Add(dir))continue;
                try
                {
                    if((File.GetAttributes(dir)&FileAttributes.ReparsePoint)!=0){result.Links++;continue;}
                    result.Folders++;
                    using var entries=Directory.EnumerateFileSystemEntries(dir).GetEnumerator();
                    while(!Stop()&&entries.MoveNext())
                    {
                        result.Entries++;
                        try
                        {
                            var path=Path.GetFullPath(entries.Current);var attrs=File.GetAttributes(path);
                            if((attrs&FileAttributes.ReparsePoint)!=0){result.Links++;continue;}
                            if((attrs&FileAttributes.Directory)!=0){if(recursive)pending.Push(path);continue;}
                            if(!Supported(path)){result.Unsupported++;continue;}
                            var length=new FileInfo(path).Length;long max=Path.GetExtension(path).Equals(".wav",StringComparison.OrdinalIgnoreCase)?64L:256L;
                            if(length<=0||length>max*1024*1024){result.Oversized++;continue;}
                            if(seen.Add(path))result.Paths.Add(path);
                        }
                        catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is System.Security.SecurityException){result.Inaccessible++;}
                        if(progress!=null){Volatile.Write(ref progress.Tracks,result.Paths.Count);Volatile.Write(ref progress.Entries,result.Entries);Volatile.Write(ref progress.Folders,result.Folders);}
                    }
                }
                catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is System.Security.SecurityException){result.Inaccessible++;}
            }
            return result;
        }
    }
}
