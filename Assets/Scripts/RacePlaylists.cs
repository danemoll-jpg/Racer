using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Racer
{
    public sealed class RacePlaylists
    {
        public static readonly string[] Scenes={"StreetLoopGreybox","StreetLoopReverse","LakeWoods","ForestLoopReverse","MountainLoop","MountainLoopReverse"};
        public static readonly string[] Titles={"Street Loop","Street Loop Reverse","Forest Loop","Forest Loop Reverse","Mountain Loop","Mountain Loop Reverse"};
        [Serializable] public sealed class Entry { public int course,laps=1; public string Title=>Titles[Mathf.Clamp(course,0,Titles.Length-1)]+" / "+laps+" lap"+(laps==1?"":"s"); }
        [Serializable] public sealed class Definition { public string name="My playlist"; public List<Entry> entries=new(); }
        [Serializable] sealed class Library { public int version=1; public List<Definition> playlists=new(); }
        public List<Definition> Definitions=>library.playlists;
        Library library=new(); readonly string path; public string Error {get;private set;}
        public static Definition Active {get;private set;} public static int Position {get;private set;}
        public static bool PendingStart {get;set;}
        public static PlaylistChampionship Championship {get;private set;}
        public static Entry Current=>Active==null?null:Active.entries[Position];
        public static string PositionLabel=>Active==null?"":Active.name+" · "+(Position+1)+" / "+Active.entries.Count;
        public static bool HasNext=>Active!=null&&Position+1<Active.entries.Count;
        public RacePlaylists(string root){path=Path.Combine(root,"race-playlists-v1.json");try{if(File.Exists(path)){var data=JsonUtility.FromJson<Library>(File.ReadAllText(path));if(data==null||data.version!=1||data.playlists==null||data.playlists.Any(d=>!Valid(d)))throw new IOException("Invalid playlist file retained");library=data;}}catch(Exception e){Error=e.Message;}}
        static bool Valid(Definition d)=>d!=null&&!string.IsNullOrWhiteSpace(d.name)&&d.entries!=null&&d.entries.All(e=>e!=null&&e.course>=0&&e.course<Scenes.Length&&e.laps>=1&&e.laps<=5);
        public bool Save(){if(Error!=null)return false;try{if(library.playlists.Any(d=>!Valid(d)))throw new IOException("Choose a name and legal finite entries");AtomicSave.Write(path,JsonUtility.ToJson(library,true));return true;}catch(Exception e){Error=e.Message;return false;}}
        public static bool Eligible(Entry entry,string vehicle)=>entry.course<2||VehicleProfile.Find(vehicle).Small;
        public static void Begin(Definition d){if(!Valid(d)||d.entries.Count==0)throw new ArgumentException("Add at least one race");Active=JsonUtility.FromJson<Definition>(JsonUtility.ToJson(d));Position=0;Championship=new(d.entries.Count);}
        public static void Advance(){if(HasNext&&Championship.Events[Position]!=null)Position++;}
        public static void Record(RaceDirector race){if(Active==null||!race.ClassificationFinal)return;Championship.Record(Position,new PlaylistChampionship.Event{course=Current.Title,order=race.Ordered(true).Select((r,i)=>new PlaylistChampionship.Finish{id="racer-"+race.Racers.IndexOf(r),name=race.Racers.IndexOf(r)==0?"YOU":"Rival "+race.Racers.IndexOf(r),place=i+1,estimated=r.Estimated,dnf=r.Dnf,finalTime=r.ClassifiedTime(race.Clock),penalties=r.Progress.PenaltySeconds}).ToArray()});}
        public static void Quit(){Active=null;Championship=null;Position=0;PendingStart=false;}
    }
}
