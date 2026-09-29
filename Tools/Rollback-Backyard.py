"""Selectively remove rejected course code; retain Kyle meshes and established scenes."""
from pathlib import Path
import subprocess
root=Path(__file__).resolve().parents[1]
for name in ['RaceDirector.cs','RaceFlow.cs','RaceMenus.cs','RoadDriver.cs','RacePlaylists.cs']:
    path='Assets/Scripts/'+name
    (root/path).write_bytes(subprocess.check_output(['git','show','777b6242:'+path],cwd=root))
# Saved playlists may still reference the two removed course indices. Keep the
# stored definitions readable and editable, but never load a removed scene.
p=root/'Assets/Scripts/RacePlaylists.cs';s=p.read_text()
s=s.replace('public string Title=>Titles[Mathf.Clamp(course,0,Titles.Length-1)]','public string Title=>(course>=Scenes.Length?"Unavailable course (rolled back)":Titles[Mathf.Clamp(course,0,Titles.Length-1)])')
s=s.replace('e.course<Scenes.Length','e.course<8')
s=s.replace('public static bool Eligible(Entry entry,string vehicle)=>entry.course<2||VehicleProfile.Find(vehicle).Small;','public static bool Eligible(Entry entry,string vehicle)=>entry.course<Scenes.Length&&(entry.course<2||VehicleProfile.Find(vehicle).Small);')
s=s.replace('if(!Valid(d)||d.entries.Count==0)','if(!Valid(d)||d.entries.Count==0||d.entries.Any(e=>e.course>=Scenes.Length))')
p.write_text(s)
p=root/'Assets/Scripts/RaceFlow.cs';s=p.read_text().replace('if(definition.entries.Count==0){Notify("Add a race first",4);return;}','if(definition.entries.Count==0){Notify("Add a race first",4);return;}if(definition.entries.Exists(e=>e.course>=RacePlaylists.Scenes.Length)){Notify("Remove the rolled-back course from this playlist",4);return;}');p.write_text(s)
for name in ['Assets/Scripts/BackyardValidation.cs','Assets/Scripts/BackyardValidation.cs.meta','Assets/Scripts/Editor/BackyardAuthoring.cs','Assets/Scripts/Editor/BackyardAuthoring.cs.meta']:
    (root/name).unlink()
print('Removed rejected runtime/authoring behavior; preserved historical records and saved playlist definitions.')
