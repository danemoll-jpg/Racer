using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine;
namespace Racer
{
    public sealed partial class CR112Validation
    {
        IEnumerator ScoringFixture()
        {
            race.opponents=race.traffic=false;flow.Save.Settings.opponents=false;
            // Explicit synthetic result data, not a claim of completed physical races.
            var definition=new RacePlaylists.Definition{name="Three-event scoring fixture",entries=new(){new(){course=4,laps=1},new(){course=5,laps=1},new(){course=5,laps=1}}};
            RacePlaylists.Begin(definition);var championship=RacePlaylists.Championship;
            int[][] orders={new[]{0,1,2,3},new[]{1,0,3,2},new[]{1,2,0,3}};
            for(int e=0;e<3;e++)championship.Record(e,new PlaylistChampionship.Event{course=definition.entries[e].Title,order=orders[e].Select((id,p)=>new PlaylistChampionship.Finish{id="racer-"+id,name=id==0?"YOU":"Rival "+id,place=p+1,dnf=e==0&&id==3,estimated=e==1&&id!=0,finalTime=100+e*12+p*3,penalties=p==2?5:0}).ToArray()});
            Check(championship.Complete&&championship.Announcement=="OVERALL WINNER: Rival 1","Synthetic three-event fixture: complete summary and correct overall winner");
            Check(championship.Standings().OrderBy(s=>s.id).Select(s=>s.points).SequenceEqual(new[]{20,26,12,6}),"Synthetic totals: YOU 20 / Rival 1 26 / Rival 2 12 / Rival 3 6; DNF earns zero");
            flow.CompleteResults();yield return null;var menus=FindAnyObjectByType<RaceMenus>();var page=typeof(RaceMenus).GetField("championshipPage",BindingFlags.Instance|BindingFlags.NonPublic);
            for(int e=0;e<3;e++){page.SetValue(menus,e);menus.Show();yield return null;ThreeFeatureValidation.CaptureUi(dir+"/synthetic-event-"+(e+1)+".png");}
            File.WriteAllText(dir+"/synthetic-summary.txt",championship.Announcement+"\n"+string.Join("\n\n",Enumerable.Range(0,3).Select(championship.Summary)));
            var replacement=championship.Events[0];flow.StartRace();Check(championship.Events[0]==null&&!championship.Complete,"Restart clears the entry and completed-winner state");championship.Record(0,replacement);championship.Record(0,replacement);
            Check(championship.Standings().OrderBy(s=>s.id).Select(s=>s.points).SequenceEqual(new[]{20,26,12,6}),"Replacing the restarted entry and reopening its snapshot do not add points twice");
            flow.StartRace();flow.Pause();flow.QuitRace();Check(RacePlaylists.Championship==null&&RacePlaylists.Active==null&&flow.State==RaceFlow.Stage.Ready,"Unfinished quit clears championship state without a completed winner");
            File.WriteAllText(dir+"/fixture-scope.txt","Synthetic data/UI checks only, authorized by Dan's reduced test scope. No races driven, no AI event, and no physical course acceptance claimed.");
        }
    }
}
