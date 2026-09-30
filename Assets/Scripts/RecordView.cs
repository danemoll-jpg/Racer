using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Racer
{
    // Read-only presentation over retained attempts. Save identities and writers stay unchanged.
    public static class RecordView
    {
        const string Vehicle = "(?:original|tourer|moto|atv)";
        static readonly Regex Key = new Regex("^(?<era>.+)-(?<vehicle>"+Vehicle+")-(?:solo|race4-d[0-2]-"+Vehicle+"-"+Vehicle+"-"+Vehicle+")-(?:traffic|clear)(?:-laps(?<laps>[0-9]+))?$");
        public static string Era(string category)
        {
            var match=Key.Match(category??"");
            return match.Success?match.Groups["era"].Value:"unknown/"+(category??"");
        }
        public static string EraLabel(string era)
        {
            if(era.StartsWith("unknown/"))return "Unrecognized historical rules";
            return "Historical · "+Regex.Replace(era,@"^(?:street|lake|forest|mountain|backyard)(?:-forward|-reverse)?-","").Replace('-',' ');
        }
        public static IReadOnlyList<RecordBoards.Entry> Query(IEnumerable<RecordBoards.Entry> entries,string era,bool race,int laps,string vehicle=null)
        {
            return entries.Where(e=>e.race==race&&Era(e.category)==era&&(vehicle==null||e.vehicle==vehicle))
                .Where(e=>!race||RaceLaps(e.category)==laps)
                .OrderBy(e=>e.seconds).ThenBy(e=>e.order).Take(10).ToArray();
        }
        public static int RaceLaps(string category)
        {
            var match=Regex.Match(category??"",@"-laps(\d+)$");
            return match.Success&&int.TryParse(match.Groups[1].Value,out int laps)?laps:-1;
        }
    }
}
