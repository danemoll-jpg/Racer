using System;
using System.IO;
using System.Threading;
namespace Racer
{
    // Atomic replacement preserves the previous file and backup. Windows readers
    // (including indexing/scan tools) can briefly hold either path after a replace.
    public static class AtomicSave
    {
        public static int Retries {get;private set;}
        public static string LastFailure {get;private set;}
        public static void Write(string path,string contents)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            for(int attempt=0;;attempt++)
            {
                try
                {
                    string temp=path+".tmp";File.WriteAllText(temp,contents);
                    if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
                    return;
                }
                catch(Exception error) when(error is IOException||error is UnauthorizedAccessException)
                {
                    LastFailure=error.GetType().Name+" / "+error.HResult+" / "+error.Message;
                    if(attempt>=4)throw;
                    Retries++;Thread.Sleep(10<<attempt);
                }
            }
        }
    }
}
