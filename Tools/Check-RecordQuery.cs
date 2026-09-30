const string output="Docs/UI/RecordFilters";
var log=new System.Text.StringBuilder();
void Check(bool good,string name){log.AppendLine((good?"PASS ":"FAIL ")+name);}
const string era="street-v14-discovery";
var entries=new List<Racer.RecordBoards.Entry>();
void Add(string id,string vehicle,int laps,double seconds,bool race=false,string rules=era,string mode="solo-clear",long order=0){entries.Add(new Racer.RecordBoards.Entry{id=id,vehicle=vehicle,category=rules+"-"+vehicle+"-"+mode+"-laps"+laps,race=race,seconds=seconds,order=order==0?entries.Count+1:order});}
Add("one","original",1,135);Add("five","moto",5,132);Add("two","atv",2,134);Add("three","tourer",3,133);Add("four","moto",4,136);
Add("ai","moto",3,130,false,era,"race4-d2-atv-moto-original-traffic");
Add("old","moto",3,1,false,"street-v8-landings");Add("reverse","moto",3,2,false,"street-reverse-v14-discovery");
for(int i=1;i<=5;i++)Add("race"+i,"moto",i,100*i,true);
var lap=Racer.RecordView.Query(entries,era,false,1);
Check(lap.Select(e=>e.id).SequenceEqual(new[]{"ai","five","three","two","one","four"}),"Lap ranks all lengths, vehicles, AI/difficulty/traffic by actual time; 5-lap 2:12 above 1-lap 2:15");
for(int i=1;i<=5;i++)Check(Racer.RecordView.Query(entries,era,true,i).Single().id=="race"+i,"Race totals separated at "+i+" laps");
foreach(string vehicle in new[]{"original","tourer","moto","atv"})Check(Racer.RecordView.Query(entries,era,false,5,vehicle).All(e=>e.vehicle==vehicle)&&Racer.RecordView.Query(entries,era,false,5,vehicle).Count>0,"Actual profile filter: "+vehicle);
Check(Racer.RecordView.Query(entries,"street-v8-landings",false,1).Single().id=="old","Historical rules isolated");
Check(Racer.RecordView.Query(entries,era,true,5,"atv").Count==0,"Empty filter combination");
Add("tie-earlier","moto",1,130,false,era,"solo-clear",1);
Check(Racer.RecordView.Query(entries,era,false,1)[0].id=="tie-earlier","Exact ties use original insertion order");
var unknown=new Racer.RecordBoards.Entry{id="unknown",category=era+"-moto-future-rules-laps1",vehicle="moto",seconds=.1};entries.Add(unknown);
Check(!Racer.RecordView.Query(entries,era,false,1).Contains(unknown)&&Racer.RecordView.Query(entries,Racer.RecordView.Era(unknown.category),false,1).Single()==unknown,"Unrecognized grammar never merges; exact historical group remains readable");
for(int i=0;i<15;i++)Add("cap"+i,"atv",5,140+i);
Check(Racer.RecordView.Query(entries,era,false,1).Count==10,"Aggregation caps at ten without trimming source");
string raw=JsonUtility.ToJson(new Racer.RecordBoards.Data{entries=entries});Racer.RecordView.Query(entries,era,false,1);Check(raw==JsonUtility.ToJson(new Racer.RecordBoards.Data{entries=entries}),"Query never mutates entries");
string dir="Temp/RecordFilters-query";System.IO.Directory.CreateDirectory(dir);var boards=new Racer.RecordBoards(dir);
boards.Add("one",era+"-moto-solo-clear-laps1",false,135,"moto");boards.Add("five",era+"-moto-solo-clear-laps5",false,132,"moto");
Check(boards.View(era,false,1).First().id=="five"&&boards.Categories(false).Length==1,"Existing writer already strips race length; read view preserves it");
string actual="C:/Users/danmo/AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3/top-ten-v1.json";
byte[] before=System.IO.File.ReadAllBytes(actual);var data=JsonUtility.FromJson<Racer.RecordBoards.Data>(System.Text.Encoding.UTF8.GetString(before));
Check(data.entries.Count==219,"Actual archive contains 219 saved entries");
Check(data.entries.All(e=>!Racer.RecordView.Era(e.category).StartsWith("unknown/")),"All actual historical categories parse explicitly");
foreach(var group in data.entries.GroupBy(e=>new {era=Racer.RecordView.Era(e.category),e.race,laps=e.race?Racer.RecordView.RaceLaps(e.category):0})){
 var expected=group.OrderBy(e=>e.seconds).ThenBy(e=>e.order).Take(10).ToArray();var found=Racer.RecordView.Query(data.entries,group.Key.era,group.Key.race,group.Key.laps);
 if(!found.SequenceEqual(expected))throw new Exception("Actual archive order mismatch");
}
Check(before.SequenceEqual(System.IO.File.ReadAllBytes(actual)),"Every actual era/length queried read-only; archive byte-identical");
System.IO.File.WriteAllText(output+"/query-checks.txt",log.ToString());return log.ToString();
