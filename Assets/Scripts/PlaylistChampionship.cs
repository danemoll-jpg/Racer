using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Racer
{
    // Results are snapshots keyed by entry, never an incrementing score counter.
    public sealed class PlaylistChampionship
    {
        [Serializable] public sealed class Finish
        {
            public string id,name; public int place,points; public bool estimated,dnf; public double finalTime,penalties;
        }
        [Serializable] public sealed class Event
        {
            public string course; public Finish[] order;
        }
        public sealed class Standing
        {
            public string id,name; public int points,wins,seconds,thirds;
        }
        public readonly Event[] Events;
        public bool Complete=>Events.Length>0&&Events.All(e=>e!=null);
        public bool Solo=>Events.Where(e=>e!=null).All(e=>e.order.Length==1);
        public const string Rules="Points: 1st 10 / 2nd 6 / 3rd 4 / 4th 2 / DNF 0.\nTies: wins, then seconds, then thirds; otherwise joint winners.";
        public PlaylistChampionship(int count){Events=new Event[count];}
        public static int Points(int place,bool dnf)=>dnf?0:place switch{1=>10,2=>6,3=>4,4=>2,_=>0};
        public void Restart(int entry){Events[entry]=null;}
        public void Record(int entry,Event result)
        {
            if(result==null||result.order==null||result.order.Select(r=>r.id).Distinct().Count()!=result.order.Length)throw new ArgumentException("Unique racer identities required");
            foreach(var r in result.order)r.points=result.order.Length==1?0:Points(r.place,r.dnf);
            Events[entry]=result;
        }
        public List<Standing> Standings()=>Events.Where(e=>e!=null).SelectMany(e=>e.order).GroupBy(r=>r.id).Select(g=>new Standing{id=g.Key,name=g.First().name,points=g.Sum(r=>r.points),wins=g.Count(r=>!r.dnf&&r.place==1),seconds=g.Count(r=>!r.dnf&&r.place==2),thirds=g.Count(r=>!r.dnf&&r.place==3)}).OrderByDescending(s=>s.points).ThenByDescending(s=>s.wins).ThenByDescending(s=>s.seconds).ThenByDescending(s=>s.thirds).ThenBy(s=>s.id,StringComparer.Ordinal).ToList();
        public string Announcement
        {
            get{
                if(!Complete)return "PLAYLIST IN PROGRESS";
                if(Solo)return "PLAYLIST COMPLETE — PERSONAL COMPLETION";
                var list=Standings();if(list.Count==0)return "PLAYLIST COMPLETE";
                var first=list[0];var winners=list.Where(s=>s.points==first.points&&s.wins==first.wins&&s.seconds==first.seconds&&s.thirds==first.thirds).ToArray();
                return (winners.Length>1?"JOINT OVERALL WINNERS: ":"OVERALL WINNER: ")+string.Join(" + ",winners.Select(w=>w.name));
            }
        }
        public string Summary(int page)
        {
            var s=new StringBuilder();s.AppendLine(Solo?"Solo event summaries / personal completion":Rules);
            if(!Solo){s.AppendLine("CUMULATIVE STANDINGS      PTS   1st / 2nd / 3rd");foreach(var r in Standings())s.AppendLine($"{r.name}     {r.points}     {r.wins} / {r.seconds} / {r.thirds}");}
            page=Math.Clamp(page,0,Events.Length-1);var e=Events[page];s.AppendLine($"\nEVENT {page+1} / {Events.Length}: {e?.course??"Not completed"}");
            if(e!=null)foreach(var r in e.order)s.AppendLine($"{r.place}. {r.name}  {(r.dnf?"DNF":RaceHud.FormatTime(r.finalTime))}  {(r.estimated?"Estimated":r.dnf?"Unfinished":"Measured")}  (+{r.penalties:0.0}s)  {r.points} pts");
            s.AppendLine("Times include the displayed penalties.");
            s.AppendLine("\nPrevious / next event reviews every playlist entry.");return s.ToString();
        }
    }
}
