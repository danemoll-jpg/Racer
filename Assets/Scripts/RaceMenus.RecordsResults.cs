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
        // 0.83 Part G: a plain Top 10. Opening Records shows the best laps on the track last raced (or the first track with any
        // times), all vehicles, all race lengths, all layout versions and rule eras together: rank, time, vehicle, date. One
        // control changes track (left / right through the eight courses), the LAP / RACE tabs switch best laps and best
        // races (every lap count, shown as a column), and an optional vehicle filter offers only vehicles with an entry on
        // that track. An empty list always says why in one line. Display only: no stored record is changed or re-ranked;
        // legacy entries stay in the list, marked. Speed traps and jumps: one site at a time, every configuration together.
        string recordsVehicle="All vehicles";
        int recordTrack=-1,recordSite;
        bool OnRecordTrack(string category)=>recordTrack>=0&&TrackTitle(category)==RacePlaylists.Titles[recordTrack];
        bool TrackHasTimes(int track,bool race){int keep=recordTrack;recordTrack=track;bool any=flow.Boards.Top(OnRecordTrack,race).Count>0;recordTrack=keep;return any;}
        void ChooseRecordTrack()
        {
            int last=Array.IndexOf(RacePlaylists.Titles,TrackTitle(flow.Race.Category));
            if(flow.TrackBrowsingLocked&&last>=0){recordTrack=last;return;}
            if(last>=0&&TrackHasTimes(last,false)){recordTrack=last;return;}
            foreach(int i in RacePlaylists.DisplayOrder)if(TrackHasTimes(i,false)||TrackHasTimes(i,true)){recordTrack=i;return;}
            recordTrack=Mathf.Max(0,last);
        }
        static string RecordDate(string date)=>DateTime.TryParse(date,null,System.Globalization.DateTimeStyles.RoundtripKind,out var d)?d.ToLocalTime().ToString("d MMM yyyy"):"—";
        void StepRecordTrack(int d)
        {
            if(flow.TrackBrowsingLocked)return;var order=RacePlaylists.DisplayOrder.ToList();int at=Mathf.Max(0,order.IndexOf(recordTrack));
            recordTrack=order[(at+d+order.Count)%order.Count];recordsVehicle="All vehicles";Show();
        }
        void RenderRecords()
        {
            if(page!="")page="";
            if(recordTrack<0||recordTrack>=RacePlaylists.Titles.Length||(flow.TrackBrowsingLocked&&RacePlaylists.Titles[recordTrack]!=TrackTitle(flow.Race.Category)))ChooseRecordTrack();
            ClearCore("RECORDS","");TabRow(new[]{"LAP","RACE","SPEED TRAPS","JUMPS","GHOST"},recordTab,i=>{recordTab=i;activityKey="";Show();});int row=5;
            int backRow=row;Row(row++,"back","Back",flow.CloseExtras);
            if(recordTab==4){details.gameObject.SetActive(true);details.text="Race your best compatible clean lap.\n"+flow.Ghost.Status+"\nNo resets, teleports or missed gates. Legal shortcuts qualify.";details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=120;Toggle(row,"ghost","Ghost",flow.Ghost.Enabled,flow.ToggleGhost);return;}
            if(recordTab<2)
            {
                bool race=recordTab==1;string track=RacePlaylists.Titles[recordTrack].Replace(" - "," — ");
                var vehicles=flow.Boards.TopVehicles(OnRecordTrack,race);
                if(recordsVehicle!="All vehicles"&&!vehicles.Contains(recordsVehicle))recordsVehicle="All vehicles";
                var board=flow.Boards.Top(OnRecordTrack,race,recordsVehicle=="All vehicles"?null:recordsVehicle);
                bool any=vehicles.Length>0;
                Step(row++,"record-track",track+(any?"":"   (no times yet)")+(flow.TrackBrowsingLocked?"   ·   current race":""),StepRecordTrack);
                int firstControl=row-1;var trackText=buttons[row-1].GetComponentInChildren<UnityEngine.UI.Text>(true);trackText.fontSize=25;trackText.alignment=TextAnchor.MiddleCenter;
                if(vehicles.Length>1||recordsVehicle!="All vehicles")
                {
                    var choices=vehicles.Prepend("All vehicles").ToList();
                    Step(row++,"record-vehicle","Vehicle: "+(recordsVehicle=="All vehicles"?"All vehicles":VehicleProfile.Find(recordsVehicle).Name),d=>{int at=Mathf.Max(0,choices.IndexOf(recordsVehicle));recordsVehicle=choices[(at+d+choices.Count)%choices.Count];Show();});
                    buttons[row-1].GetComponentInChildren<UnityEngine.UI.Text>(true).alignment=TextAnchor.MiddleCenter;
                }
                Controls(firstControl,row);
                title.text="RECORDS";details.gameObject.SetActive(true);details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=28;
                string vehicleName=recordsVehicle=="All vehicles"?"":" with the "+VehicleProfile.Find(recordsVehicle).Name;
                details.text=flow.Boards.Error??(board.Count==0
                    ?$"No {(race?"race":"lap")} times on {track}{vehicleName} yet. Finish a {(race?"race":"lap")} there to set one."
                    :(race?"BEST RACES · full-race totals, every race length":"BEST LAPS · every race length")+(recordsVehicle=="All vehicles"?" · all vehicles":" · "+VehicleProfile.Find(recordsVehicle).Name)+" · ‹ › changes track");
                // 0.94 Part A: who set each time; the player's own entries highlighted
                var widths=race?new[]{.08f,.21f,.17f,.08f,.24f,.22f}:new[]{.09f,.23f,.19f,.27f,.22f};
                int firstRow=row;if(board.Count>0)TableRow(row++,"header",race?new[]{"Rank","Time","Name","Laps","Vehicle","Date"}:new[]{"Rank","Time","Name","Vehicle","Date"},widths,()=>{});
                for(int i=0;i<board.Count;i++)
                {
                    var e=board[i];int laps=RecordView.RaceLaps(e.category);string time=RaceHud.FormatTime(e.seconds)+(flow.Boards.IsNew(e.id)?"  NEW":"");
                    string vehicle=VehicleProfile.Find(e.vehicle).Name,date=RecordDate(e.date)+(e.legacy?" · legacy":"");
                    string who=string.IsNullOrEmpty(e.name)?PlayerNames.Player:e.name;bool own=who==PlayerNames.Player;
                    TableRow(row++,"record-"+e.id,race?new[]{(i+1).ToString(),time,who,laps>0?laps.ToString():"?",vehicle,date}:new[]{(i+1).ToString(),time,who,vehicle,date},widths,
                        ()=>Help((e.legacy?"Legacy record (identity unknown)":who+"'s attempt on this PC")+"\nDate: "+(e.date??"Unknown")+"\n"+RecordBoards.Describe(e.category)+"\nLayout / rules: "+e.category+"\nExact ties retain original insertion order."),own||flow.Boards.IsNew(e.id));
                }
                Table(firstRow,row);buttons[backRow].transform.SetAsLastSibling();
            }
            else
            {
                var kind=recordTab==2?ActivitySite.Kind.Speed:ActivitySite.Kind.Jump;var sites=flow.Activities.Sites.Where(s=>s&&s.kind==kind).ToArray();
                recordSite=sites.Length==0?0:Mathf.Clamp(recordSite,0,sites.Length-1);var site=sites.Length>0?sites[recordSite]:null;
                var entries=site?flow.Activities.Records.Archive.entries.Where(e=>e.key.Split('/')[0]==site.id).OrderByDescending(e=>e.value).ThenBy(e=>e.date,StringComparer.Ordinal).Take(10).ToList():new System.Collections.Generic.List<ActivityRecords.Entry>();
                string where=site?site.title:recordTab==2?"the speed traps":"the jumps";
                Step(row++,"activity-site",where+(entries.Count==0?"   (no results yet)":""),d=>{if(sites.Length>0)recordSite=(recordSite+d+sites.Length)%sites.Length;Show();});
                var siteText=buttons[row-1].GetComponentInChildren<UnityEngine.UI.Text>(true);siteText.fontSize=25;siteText.alignment=TextAnchor.MiddleCenter;Controls(row-1,row);
                details.gameObject.SetActive(true);details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=28;
                details.text=flow.Activities.Records.Error??(entries.Count==0?$"No {(recordTab==2?"speed-trap":"jump")} results at {where} yet. {(recordTab==2?"Drive through it":"Land it")} in Free Roam to set one."
                    :(recordTab==2?"FASTEST":"LONGEST")+" · all vehicles · ‹ › changes "+(recordTab==2?"speed trap":"jump"));
                var widths=new[]{.08f,.23f,.27f,.17f,.25f};
                int firstRow=row;if(entries.Count>0)TableRow(row++,"header",new[]{"Rank",recordTab==2?"Speed":"Distance","Vehicle","Medal","Date"},widths,()=>{});
                for(int i=0;i<entries.Count;i++){var e=entries[i];TableRow(row++,"activity-"+e.id,new[]{(i+1).ToString(),site?ArcadeActivities.Measurement(site,e.value):e.value.ToString("0.#"),VehicleProfile.Find(e.vehicle).Name,"",RecordDate(e.date)+(e.historical?" · legacy":"")},widths,
                    ()=>Help("Date: "+(e.date??"Unknown")+"\nConfiguration: "+e.key+(recordTab==3&&e.airtime>0?"\nAirtime: "+e.airtime.ToString("0.000")+"s":"")+"\nEqual values retain attempt order."),i<3);CellBadge(row-1,widths,3,Mathf.Clamp(e.medal,0,3));}
                Table(firstRow,row);buttons[backRow].transform.SetAsLastSibling();
            }
        }
        // the track (and vehicle) control as one row; the Top 10 as one compact block, so all ten rows fit on the screen
        void Controls(int from,int to){var group=LaterGroup("Record track",content,true,48);for(int k=from;k<to;k++){buttons[k].transform.SetParent(group,false);buttons[k].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=48;}}
        void Table(int from,int to)
        {
            if(to<=from)return;var table=LaterGroup("Top 10",content,false,(to-from)*30-2);table.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing=2;
            for(int k=from;k<to;k++){buttons[k].transform.SetParent(table,false);buttons[k].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=28;
                foreach(var cell in buttons[k].GetComponentsInChildren<UnityEngine.UI.Text>(true))if(cell.name=="Cell")cell.fontSize=18;}
        }
        void RenderResults()
        {
            if(CampaignRun.Active!=null){RenderCampaignResults();return;}
            if(SplitScreen.Active&&PoliceChase.Current){RenderPoliceResults();return;} // 0.94 Part C
            if(SplitScreen.Active&&SpeedPatrol.Current){RenderPatrolResults();return;} // 0.95 Part G
            if(SplitScreen.Active){RenderSplitResults();return;}
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
                foreach(var r in flow.Race.Ordered(true)){int place=++rank;TableRow(n++,"standing-"+place,new[]{place.ToString(),CampaignRun.Active!=null?r.Name+" · "+VehicleProfile.Find(r.Car.GetComponent<VehicleConfiguration>().profileId).Name:r.IsAi?"Rival "+flow.Race.Racers.IndexOf(r):"You",r.Dnf?"—":RaceHud.FormatTime(r.ClassifiedTime(flow.Race.Clock)),r.Dnf?"DNF":r.Estimated?"Estimated":"Measured"},new[]{.1f,.32f,.30f,.28f},()=>{},!r.IsAi);}
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
