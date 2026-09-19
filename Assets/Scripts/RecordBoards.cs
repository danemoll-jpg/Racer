using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Racer
{
    // Separate from legacy best files: never overwrite or infer unknown legacy metadata.
    public sealed class RecordBoards
    {
        [Serializable] public sealed class Entry
        {
            public string id, category, vehicle, date;
            public bool race, legacy;
            public double seconds;
            public long order;
        }
        [Serializable] public sealed class Data
        {
            public int version=1;
            public long sequence;
            public List<Entry> entries=new();
            public List<string> received=new(), migrations=new();
        }
        readonly string path;
        Data data=new();
        public string Error { get; private set; }
        public string LatestLapId { get; private set; }
        public string LatestRaceId { get; private set; }
        readonly HashSet<string> newEntries=new();
        public bool IsNew(string id)=>newEntries.Contains(id);
        public void BeginAttempt()=>newEntries.Clear();
        public static string LapCategory(string category)=>Regex.Replace(category,@"-laps\d+$","");
        public static string Describe(string category)
        {
            var match=Regex.Match(category,@"^(.*?)-(original|tourer|moto|atv)-(.*)$");if(!match.Success)return category;
            var bits=match.Groups[3].Value.Split('-');string course=match.Groups[1].Value;
            string track=course=="lake-v2-forest"?"Forest Loop / v2":course=="lake-v1"?"Forest Loop / historical v1":course=="street-v8-landings"?"Street Loop / v8 landings":course;
            string mode=bits[0]=="solo"?"Solo":bits.Length>=5?"3 AI / "+new[]{"Easy","Normal","Hard"}[Mathf.Clamp(bits[1].Last()-'0',0,2)]:"Legacy race";
            string roster=bits[0]=="race4"&&bits.Length>=5?"\nAI: "+string.Join(" / ",bits.Skip(2).Take(3).Select(id=>VehicleProfile.Find(id).Name)):"";
            return track+" / "+VehicleProfile.Find(match.Groups[2].Value).Name+"\n"+mode+" / "+(bits.Contains("traffic")?"Traffic":"Clear")+(bits.Last().StartsWith("laps")?" / "+bits.Last().Substring(4)+" laps":" / completed laps")+roster;
        }
        static bool Valid(double n)=>n>0 && n<31536000 && !double.IsNaN(n) && !double.IsInfinity(n);
        public RecordBoards(string directory)
        {
            path=Path.Combine(directory,"top-ten-v1.json");
            try
            {
                if(File.Exists(path))
                {
                    var loaded=JsonUtility.FromJson<Data>(File.ReadAllText(path));
                    if(loaded==null||loaded.version!=1||loaded.entries==null||loaded.received==null||loaded.migrations==null)throw new InvalidDataException();
                    data=loaded;
                }
                Migrate(directory);
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException)
            { Error="Record boards could not be read; existing file preserved."; readFailed=true; }
        }
        bool readFailed;
        public IReadOnlyList<Entry> Board(string category,bool race)=>data.entries.Where(e=>e.category==(race?category:LapCategory(category))&&e.race==race)
            .OrderBy(e=>e.seconds).ThenBy(e=>e.order).Take(10).ToArray();
        public string[] Categories(bool race)=>data.entries.Where(e=>e.race==race).Select(e=>e.category).Distinct().OrderBy(s=>s,StringComparer.Ordinal).ToArray();
        public int Add(string id,string category,bool race,double seconds,string vehicle,string date=null,bool legacy=false)
        {
            if(readFailed||string.IsNullOrEmpty(id)||!Valid(seconds))return 0;
            if(category.StartsWith("lake-v2-forest-")&&(!VehicleProfile.Find(vehicle).Small||category.Split('-').Any(p=>p=="original"||p=="tourer")))return 0;
            string legacyFile="records-"+category+".json";
            if(!race)category=LapCategory(category);
            if(data.received.Contains(id))return 0;
            var entry=new Entry{id=id,category=category,race=race,seconds=seconds,vehicle=vehicle,date=date,legacy=legacy,order=++data.sequence};
            data.received.Add(id);data.entries.Add(entry);
            var ordered=Board(category,race);int rank=Array.FindIndex(ordered.ToArray(),e=>e.id==id)+1;
            var retained=new HashSet<string>(ordered.Select(e=>e.id));
            data.entries.RemoveAll(e=>e.category==category&&e.race==race&&!retained.Contains(e.id));
            if(!legacy)
            {
                if(race)LatestRaceId=id;else LatestLapId=id;if(rank>0)newEntries.Add(id);
                // RaceFlow also maintains its legacy best file for save compatibility.
                // Mark it in the same atomic write so reopening cannot import our own
                // newly recorded attempt a second time as an undated legacy best.
                if(!data.migrations.Contains(legacyFile))data.migrations.Add(legacyFile);
            }
            Write();return rank;
        }
        public int CompletedLap(string attempt,string category,string vehicle,RaceProgress progress)
        {
            if(progress.CompletedLaps<=0||progress.LapTimes.Count!=progress.CompletedLaps)return 0;
            return Add(attempt+"/lap/"+progress.CompletedLaps,category,false,progress.LastLap,vehicle,DateTime.UtcNow.ToString("o"));
        }
        public int CompletedRace(string attempt,string category,string vehicle,RaceProgress progress,double now)
        {
            if(!progress.Finished)return 0;
            return Add(attempt+"/race",category,true,progress.AdjustedTime(now),vehicle,DateTime.UtcNow.ToString("o"));
        }
        void Migrate(string directory)
        {
            if(!Directory.Exists(directory))return;
            foreach(var file in Directory.GetFiles(directory,"records-*.json").OrderBy(f=>f,StringComparer.Ordinal))
            {
                string name=Path.GetFileName(file);if(data.migrations.Contains(name))continue;
                RacerSave.Records old;
                try{old=JsonUtility.FromJson<RacerSave.Records>(File.ReadAllText(file));}catch(ArgumentException){continue;}
                // These known categories encode rules, vehicle, mode, roster, traffic and lap count.
                // Older categories missing any of them remain untouched in their original files.
                if(old==null||old.version!=1||old.course==null)continue;
                var match=Regex.Match(old.course,@"^street-v8-landings-(original|tourer|moto|atv)-(solo|race4-d[0-2]-(original|tourer|moto|atv)-(original|tourer|moto|atv)-(original|tourer|moto|atv))-(traffic|clear)-laps[1-9]\d*$");
                if(!match.Success)continue;
                Add("legacy/"+name+"/lap",old.course,false,old.lap,match.Groups[1].Value,null,true);
                Add("legacy/"+name+"/race",old.course,true,old.race,match.Groups[1].Value,null,true);
                data.migrations.Add(name);Write();
            }
        }
        void Write()
        {
            if(readFailed)return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";
                File.WriteAllText(temp,JsonUtility.ToJson(data,true));
                if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
                Error=null;
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException){Error="Could not save record boards; retained in memory.";}
        }
    }
}

