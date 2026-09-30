using System;
using System.Linq;
using UnityEngine;

namespace Racer
{
    public sealed partial class RaceMenus
    {
        static string TrackTitle(string category)
        {
            string c=category??"";bool reverse=c.Contains("reverse");
            if(c.StartsWith("backyard"))return "Dan's Backyard Loop - "+(reverse?"Reverse":"Forward");
            if(c.StartsWith("mountain"))return "Mountain Loop - "+(reverse?"Reverse":"Forward");
            if(c.StartsWith("lake")||c.StartsWith("forest"))return "Forest Loop - "+(reverse?"Reverse":"Forward");
            if(c.StartsWith("street"))return "Street Loop - "+(reverse?"Reverse":"Forward");return "Historical layout";
        }
        string recordsTrack="All tracks",recordsVehicle="All vehicles";
        int recordCourse=-1;
        string PreviewRecordCategory(int index)=>CoursePreviewCatalog.Courses[index].id+flow.Race.Category.Substring(flow.Race.courseId.Length);
        string BrowsedRecordCategory=>string.IsNullOrEmpty(recordFilter)?(recordCourse<0?flow.Race.Category:PreviewRecordCategory(recordCourse)):recordFilter;
        static string ConfigurationLabel(string category)
        {
            var match=System.Text.RegularExpressions.Regex.Match(category,@"-(original|tourer|moto|atv)-");
            string vehicle=match.Success?VehicleProfile.Find(match.Groups[1].Value).Name:"Legacy vehicle";
            var laps=System.Text.RegularExpressions.Regex.Match(category,@"-laps(\d+)$");
            return vehicle+" · "+(category.Contains("-solo-")?"Solo":category.Contains("-race4-")?"3 AI":"Legacy mode")+" · "+(category.Contains("-traffic")?"Traffic":"Clear")+(laps.Success?" · "+laps.Groups[1].Value+" laps":"");
        }
        string[] RecordCategories()=>flow.Boards.Categories(recordTab==1).Concat(Enumerable.Range(0,CoursePreviewCatalog.Courses.Length).Select(i=>recordTab==1?PreviewRecordCategory(i):RecordBoards.LapCategory(PreviewRecordCategory(i)))).Append(recordTab==1?flow.Race.Category:RecordBoards.LapCategory(flow.Race.Category)).Distinct().ToArray();
        void RenderRecords()
        {
            if(flow.TrackBrowsingLocked){recordCourse=-1;recordsTrack=TrackTitle(flow.Race.Category);if(!string.IsNullOrEmpty(recordFilter)&&TrackTitle(recordFilter)!=recordsTrack)recordFilter="";}
            if(page=="record-tracks")
            {
                ClearCore("RECORDS · SELECT TRACK","Browse records without changing your race. Forward and Reverse remain separate.");int n=0;Row(n++,"back","Back",()=>BackPage());
                foreach(int i in RacePlaylists.DisplayOrder){int choice=i;if(i>=CoursePreviewCatalog.Courses.Length||flow.TrackBrowsingLocked)continue;Row(n++,"record-track-"+i,RacePlaylists.Titles[i],()=>{recordCourse=choice;recordsTrack=RacePlaylists.Titles[choice];recordFilter="";var preferred=PreviewRecordCategory(choice);if(flow.Boards.Board(preferred,recordTab==1).Count==0)recordFilter=flow.Boards.Categories(recordTab==1).Where(c=>TrackTitle(c)==recordsTrack&&(recordsVehicle=="All vehicles"||c.Contains("-"+recordsVehicle+"-"))).OrderByDescending(c=>c.StartsWith(CoursePreviewCatalog.Courses[choice].id+"-")).FirstOrDefault()??"";BackPage();});}return;
            }
            if(page=="record-filters")
            {
                ClearCore("RECORD FILTERS","Choose a track / direction or vehicle. Categories remain separate.");int n=0;Row(n++,"back","Back",()=>BackPage());
                foreach(string t in RecordCategories().Select(TrackTitle).Distinct().Prepend("All tracks").Where(t=>!flow.TrackBrowsingLocked||t==TrackTitle(flow.Race.Category))){string choice=t;Row(n++,"filter-"+t,(recordsTrack==t?"✓ ":"")+t,()=>{recordsTrack=choice;Show();});}
                foreach(string v in new[]{"All vehicles","original","tourer","moto","atv"}){string choice=v;Row(n++,"vehicle-"+v,(recordsVehicle==v?"✓ ":"")+(v=="All vehicles"?v:VehicleProfile.Find(v).Name),()=>{recordsVehicle=choice;Show();});}return;
            }
            if(page=="record-categories")
            {
                ClearCore("SAVED CONFIGURATIONS","Choose a distinct configuration. Historical records are retained.");int n=0;Row(n++,"back","Back",()=>BackPage());
                if(recordTab<2)foreach(string cat in RecordCategories().Where(c=>(recordsTrack=="All tracks"||TrackTitle(c)==recordsTrack)&&(recordsVehicle=="All vehicles"||c.Contains("-"+recordsVehicle+"-")))){string choice=cat;Row(n++,"category-"+cat,TrackTitle(cat)+"\n"+ConfigurationLabel(cat)+" · "+flow.Boards.Board(cat,recordTab==1).Count+" saved",()=>{recordFilter=choice;BackPage();});buttons[n-1].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=82;}
                else
                {
                    var kind=recordTab==2?ActivitySite.Kind.Speed:ActivitySite.Kind.Jump;
                    var keys=flow.Activities.Records.Archive.entries.Select(e=>e.key).Concat(flow.Activities.Sites.Where(s=>s.kind==kind).Select(flow.Activities.Key)).Distinct();
                    foreach(string key in keys){string choice=key;var site=flow.Activities.Sites.FirstOrDefault(s=>s.id==key.Split('/')[0]);if(site&&site.kind!=kind)continue;Row(n++,"category-"+key,(site?site.title:"Historical site")+"\n"+key,()=>{activityKey=choice;BackPage();});buttons[n-1].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=82;}
                }
                if(n==1)details.text="No saved configurations match these filters.";return;
            }
            ClearCore("RECORDS","");TabRow(new[]{"LAP","RACE","SPEED TRAPS","JUMPS","GHOST"},recordTab,i=>{recordTab=i;recordFilter=activityKey="";Show();});int row=5;
            Row(row++,"back","Back",flow.CloseExtras);
            if(recordTab==4){details.gameObject.SetActive(true);details.text="Race your best compatible clean lap.\n"+flow.Ghost.Status+"\nNo resets, teleports or missed gates. Legal shortcuts qualify.";details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=120;Row(row,"ghost",flow.Ghost.Enabled?"Ghost: On":"Ghost: Off",flow.ToggleGhost);return;}
            Row(row++,"categories","Configurations",()=>Navigate("record-categories"));
            if(recordTab<2)
            {
                Row(row++,"filters","Filters",()=>Navigate("record-filters"));
                Row(row++,"current","Current race",()=>{recordCourse=-1;recordFilter="";recordsTrack="All tracks";Show();});
                var tools=LaterGroup("Record controls",content,true);tools.SetSiblingIndex(2);for(int i=5;i<row;i++)buttons[i].transform.SetParent(tools,false);
                string category=BrowsedRecordCategory;var board=flow.Boards.Board(category,recordTab==1);
                Row(row++,"record-tracks","Track: "+TrackTitle(category)+(flow.TrackBrowsingLocked?"  ·  Current race":"  ·  Change"),()=>{if(!flow.TrackBrowsingLocked)Navigate("record-tracks");});buttons[row-1].interactable=!flow.TrackBrowsingLocked;
                title.text="RECORDS";details.gameObject.SetActive(true);details.text=(recordTab==1?"TOTAL RACE":"BEST LAP")+" · "+(string.IsNullOrEmpty(recordFilter)?(recordCourse<0?"Current configuration":"Browsing track · race settings unchanged"):"Saved configuration")+"\n"+ConfigurationLabel(category)+"\n"+(flow.Boards.Error??(board.Count==0?"No times in this setup. Configurations lists your other saved records.":"Select a row for details."));details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=100;
                TableRow(row++,"header",new[]{"Rank","Driver","Vehicle","Time"},new[]{.10f,.30f,.32f,.28f},()=>{});
                for(int i=0;i<board.Count;i++){var e=board[i];TableRow(row++,"record-"+e.id,new[]{(i+1).ToString(),e.legacy?"Local record (legacy)":"You",VehicleProfile.Find(e.vehicle).Name,RaceHud.FormatTime(e.seconds)+(flow.Boards.IsNew(e.id)?"  NEW":"")},new[]{.10f,.30f,.32f,.28f},()=>Help((e.legacy?"Legacy identity unknown":"Your local attempt")+"\nDate: "+(e.date??"Unknown")+"\n"+RecordBoards.Describe(category)+"\nLayout / rules: "+e.category+"\nExact ties retain original insertion order."),i<3||flow.Boards.IsNew(e.id));}
                Row(row,"details","Configuration Details",()=>Help(RecordBoards.Describe(category)+"\nLayout / rules: "+category));
            }
            else
            {
                var kind=recordTab==2?ActivitySite.Kind.Speed:ActivitySite.Kind.Jump;var site=flow.Activities.Sites.FirstOrDefault(s=>s.kind==kind);
                string key=string.IsNullOrEmpty(activityKey)?(site?flow.Activities.Key(site):""):activityKey;var entries=flow.Activities.Records.Board(key);var actual=flow.Activities.Sites.FirstOrDefault(s=>s.id==key.Split('/')[0]);
                details.gameObject.SetActive(true);details.text=(actual?actual.title:"Historical site")+"\n"+(flow.Activities.Records.Error??(entries.Count==0?"No records for this configuration. Explore and complete an activity.":"Select a row for details."));details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=100;
                TableRow(row++,"header",new[]{"Rank","Driver","Vehicle",recordTab==2?"Speed":"Distance","Medal"},new[]{.08f,.27f,.25f,.23f,.17f},()=>{});
                for(int i=0;i<entries.Count;i++){var e=entries[i];TableRow(row++,"activity-"+e.id,new[]{(i+1).ToString(),e.historical?"Local record (legacy)":"You",VehicleProfile.Find(e.vehicle).Name,actual?ArcadeActivities.Measurement(actual,e.value):"See Details",new[]{"—","Bronze","Silver","Gold"}[Mathf.Clamp(e.medal,0,3)]},new[]{.08f,.27f,.25f,.23f,.17f},()=>Help("Date: "+(e.date??"Unknown")+"\nConfiguration: "+e.key+(!actual?"\nOriginal stored value: "+e.value.ToString("0.###")+" (site type unavailable)":"")+(recordTab==3&&e.airtime>0?"\nAirtime: "+e.airtime.ToString("0.000")+"s":"")+"\nEqual values retain attempt order."),i<3);}
            }
        }
        void RenderResults()
        {
            ClearCore("RESULTS",flow.Race.courseName);bool playlist=RacePlaylists.Active!=null;
            string[] tabs=playlist?new[]{"STANDINGS","LAP TIMES","PENALTIES","CHAMPIONSHIP"}:new[]{"STANDINGS","LAP TIMES","PENALTIES"};resultTab=Mathf.Clamp(resultTab,0,tabs.Length-1);
            TabRow(tabs,resultTab,i=>{resultTab=i;Show();});int n=4;
            Row(n++,"primary",RacePlaylists.HasNext?"NEXT RACE":playlist?"RESTART CURRENT ENTRY":"RACE AGAIN",RacePlaylists.HasNext?flow.NextPlaylistRace:flow.StartRace);
            if(playlist&&RacePlaylists.HasNext)Row(n++,"restart","Restart current entry",flow.StartRace);
            if(flow.FinishCards.Lap!=null&&resultTab==0)
            {
                var cards=LaterGroup("Lap and race achievements",content,true,150);cards.SetSiblingIndex(3);
                var lap=Label("Best lap",cards,21,150);lap.text="BEST LAP\n"+flow.FinishCards.Lap.Text;lap.color=new(.3f,.95f,.81f);
                var race=Label("Total race",cards,21,150);race.text="TOTAL RACE\n"+flow.FinishCards.Race.Text;race.color=new(.3f,.95f,.81f);
            }
            if(resultTab==0)
            {
                TableRow(n++,"header",new[]{"Place","Driver","Time","Status"},new[]{.1f,.32f,.30f,.28f},()=>{});int rank=0;
                foreach(var r in flow.Race.Ordered(true)){int place=++rank;TableRow(n++,"standing-"+place,new[]{place.ToString(),r.IsAi?"Rival "+flow.Race.Racers.IndexOf(r):"You",r.Dnf?"—":RaceHud.FormatTime(r.ClassifiedTime(flow.Race.Clock)),r.Dnf?"DNF":r.Estimated?"Estimated":"Measured"},new[]{.1f,.32f,.30f,.28f},()=>{},!r.IsAi);}
            }
            else if(resultTab==1)
            {
                TableRow(n++,"header",new[]{"Lap","Adjusted time"},new[]{.5f,.5f},()=>{});for(int i=0;i<flow.Race.Progress.LapTimes.Count;i++)TableRow(n++,"lap-"+i,new[]{(i+1).ToString(),RaceHud.FormatTime(flow.Race.Progress.LapTimes[i])},new[]{.5f,.5f},()=>{});
            }
            else if(resultTab==2)Row(n++,"penalties","Penalty Details · "+flow.Race.Progress.MissedGates+" missed / +"+flow.Race.Progress.PenaltySeconds.ToString("0")+"s",()=>Navigate("penalties"));
            else
            {
                var championship=RacePlaylists.Championship;details.text=championship.Announcement;details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=65;
                if(!championship.Solo){TableRow(n++,"header",new[]{"Driver","Points","Wins / 2nd / 3rd"},new[]{.4f,.2f,.4f},()=>{});foreach(var s in championship.Standings())TableRow(n++,"champ-"+s.id,new[]{s.name,s.points.ToString(),s.wins+" / "+s.seconds+" / "+s.thirds},new[]{.4f,.2f,.4f},()=>{});}
                for(int i=0;i<championship.Events.Length;i++){int eventIndex=i;var e=championship.Events[i];Row(n++,"event-"+i,(i+1)+". "+(e?.course??RacePlaylists.Active.entries[i].Title+" · Pending"),()=>Help(championship.Summary(eventIndex)));}
                Row(n++,"rules","Scoring / tie rules",()=>Help(PlaylistChampionship.Rules));
            }
            Row(n++,"setup","Race Setup",()=>flow.OpenResultsSetup());Row(n++,"records","Records",flow.OpenBoards);Row(n++,"exploration","Exploration",flow.OpenExploration);Row(n++,"settings","Settings",flow.OpenSettings);
            Row(n,"menu","Return to Menu",()=>{if(playlist)Confirm("QUIT PLAYLIST?","This ends the active championship. Saved playlists are kept.",flow.QuitRace);else flow.QuitRace();});
        }
    }
}
