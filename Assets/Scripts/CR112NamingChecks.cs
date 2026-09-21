using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer
{
    public sealed partial class CR112Validation
    {
        IEnumerator Naming()
        {
            var definition=new RacePlaylists.Definition{name="Before naming",entries=new(){new(){course=4,laps=1}}};flow.Playlists.Definitions.Clear();flow.Playlists.Definitions.Add(definition);flow.OpenPlaylists();yield return null;
            void Click(string label){var b=FindObjectsByType<UnityEngine.UI.Button>().Single(b=>b.GetComponentInChildren<UnityEngine.UI.Text>()?.text==label);b.onClick.Invoke();}
            Click("Name playlist");yield return null;var field=FindAnyObjectByType<UnityEngine.UI.InputField>();
            Check(field&&field.isFocused&&EventSystem.current.currentSelectedGameObject==field.gameObject,"Naming opens a focused editable field");
            field.selectionAnchorPosition=0;field.selectionFocusPosition=field.text.Length;
            foreach(char c in "Mountain")field.ProcessEvent(new Event{type=EventType.KeyDown,character=c});
            string clipboard=GUIUtility.systemCopyBuffer;GUIUtility.systemCopyBuffer=" Night Flights";field.ProcessEvent(Event.KeyboardEvent("^v"));GUIUtility.systemCopyBuffer=clipboard;
            field.selectionAnchorPosition=9;field.selectionFocusPosition=14;foreach(char c in "Sunset")field.ProcessEvent(new Event{type=EventType.KeyDown,character=c});
            field.MoveTextEnd(false);field.ProcessEvent(new Event{type=EventType.KeyDown,character='X'});field.ProcessEvent(Event.KeyboardEvent("backspace"));
            field.MoveTextStart(false);field.ProcessEvent(new Event{type=EventType.KeyDown,character='X'});field.MoveTextStart(false);field.ProcessEvent(Event.KeyboardEvent("delete"));field.ForceLabelUpdate();
            Check(field.text=="Mountain Sunset Flights","Typed/pasted spaces, selection replacement, Backspace and Delete");
            var keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Check(flow.State==RaceFlow.Stage.Playlists&&field.gameObject.activeSelf&&definition.name=="Before naming","Focused Space does not trigger menu actions or save");
            ThreeFeatureValidation.CaptureUi(dir+"/naming-field.png");
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Enter));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Check(definition.name=="Mountain Sunset Flights"&&new RacePlaylists(flow.Save.DirectoryPath).Definitions.Single().name==definition.name,"Enter saves the edited name and reload preserves it");
            Click("Name playlist");yield return null;field.MoveTextEnd(false);foreach(char c in " cancelled")field.ProcessEvent(new Event{type=EventType.KeyDown,character=c});
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Check(definition.name=="Mountain Sunset Flights"&&!field.gameObject.activeSelf&&flow.State==RaceFlow.Stage.Playlists,"Escape cancels draft without closing the playlist menu");InputSystem.RemoveDevice(keyboard);
        }
        void ModelChecks()
        {
            PlaylistChampionship.Event E(params string[] order)=>new(){course="Fixture",order=order.Select((id,i)=>new PlaylistChampionship.Finish{id=id,name=id,place=i+1,finalTime=120+i}).ToArray()};
            Check(new[]{1,2,3,4}.Select(p=>PlaylistChampionship.Points(p,false)).SequenceEqual(new[]{10,6,4,2})&&PlaylistChampionship.Points(1,true)==0,"Points 10/6/4/2; DNF zero even if first slot");
            var c=new PlaylistChampionship(2);c.Record(0,E("A","B","C","D"));c.Record(0,E("A","B","C","D"));Check(c.Standings()[0].points==10&&!c.Complete&&!c.Announcement.Contains("WINNER"),"Repeated result snapshot is idempotent; unfinished has no winner");
            c.Restart(0);c.Record(0,E("B","A","C","D"));c.Record(1,E("A","B","C","D"));Check(c.Announcement=="JOINT OVERALL WINNERS: A + B"&&c.Standings()[0].points==16,"Restart replaces previous points; exact podium tie gives joint winners");
            var tie=new PlaylistChampionship(1);var e=E("A","B");tie.Record(0,e);e.order[0].points=e.order[1].points=10;Check(tie.Announcement=="OVERALL WINNER: A","Equal points resolves by most wins");
            e.order[0].place=2;e.order[1].place=3;Check(tie.Announcement=="OVERALL WINNER: A","Equal points and wins resolves by most seconds");
            e.order[0].place=3;e.order[1].place=4;Check(tie.Announcement=="OVERALL WINNER: A","Equal points, wins and seconds resolves by most thirds");
            var solo=new PlaylistChampionship(1);solo.Record(0,E("YOU"));Check(solo.Announcement.Contains("PERSONAL COMPLETION")&&solo.Events[0].order[0].points==0,"Solo completion has no invented rivals or championship points");
        }
    }
}
