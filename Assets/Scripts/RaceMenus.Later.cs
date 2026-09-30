using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Racer
{
    public sealed partial class RaceMenus
    {
        readonly List<GameObject> laterLayouts=new();
        readonly List<GameObject> tableCells=new();
        readonly List<UnityEngine.UI.Button> leftPaneButtons=new(),rightPaneButtons=new();
        UnityEngine.InputSystem.InputAction playlistAdd,playlistContext;
        int recordTab,resultTab;string recordFilter="",activityKey="";
        RacePlaylists.Definition playlistDraft;
        int draftIndex=-1;string savedDraft="";int moveFrom=-1,moveTo;RacePlaylists.Entry entryDraft;
        Action dirtyContinuation;bool dirtyStarting;
        bool PlaylistDirty=>playlistDraft!=null&&JsonUtility.ToJson(playlistDraft)!=savedDraft;
        void EnsureRows(int count)
        {
            while(buttons.Count<count){var r=Rect("Action "+buttons.Count,content);r.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=44;var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;b.colors=buttons[0].colors;var label=Label("Label",r,21,0);Stretch(label.rectTransform,10,0,-10,0);buttons.Add(b);}
        }
        void UpdateLater()
        {
            if(playlistAdd==null){playlistAdd=new("Add Race",UnityEngine.InputSystem.InputActionType.Button,"<Keyboard>/f");playlistAdd.AddBinding("<Gamepad>/buttonWest");playlistAdd.Enable();playlistContext=new("Actions",UnityEngine.InputSystem.InputActionType.Button,"<Keyboard>/g");playlistContext.AddBinding("<Gamepad>/buttonNorth");playlistContext.Enable();}
            if(!flow.MenuVisible||modalConfirm!=null||page=="keyboard")return;
            if(page=="playlist-move")
            {
                string name=EventSystem.current?.currentSelectedGameObject?.name;
                if(name!=null&&name.StartsWith("move-")&&int.TryParse(name.Substring(5),out int index)&&index!=moveTo){moveTo=index;CapturePage();Show();}return;
            }
            bool records=flow.State==RaceFlow.Stage.Boards||flow.State==RaceFlow.Stage.Activities;
            if(page==""&&(records||flow.State==RaceFlow.Stage.Results))
            {
                int d=tabsAction.WasPressedThisFrame()?1:previousTab.WasPressedThisFrame()?-1:0;
                if(d!=0){if(records){recordTab=(recordTab+d+5)%5;activityKey="";}else{int count=RacePlaylists.Active==null?3:4;resultTab=(resultTab+d+count)%count;}Show();MenuInput.ConsumeThroughRelease();}
            }
            if(flow.State==RaceFlow.Stage.Playlists&&page==""&&playlistDraft!=null)
            {
                string focused=EventSystem.current?.currentSelectedGameObject?.name;
                if(focused!=null&&focused.StartsWith("entry-")&&int.TryParse(focused.Substring(6),out int i))entryIndex=i;
                if(playlistAdd.WasPressedThisFrame())EditEntry(-1);else if(playlistContext.WasPressedThisFrame())Navigate("playlist-actions");
            }
        }
        void ResetLaterLayout()
        {
            foreach(var b in buttons){if(laterLayouts.Any(g=>g&&b.transform.IsChildOf(g.transform)))b.transform.SetParent(content,false);b.GetComponentInChildren<UnityEngine.UI.Text>(true).gameObject.SetActive(true);}
            foreach(var cell in tableCells)if(cell){cell.SetActive(false);Destroy(cell);}tableCells.Clear();
            foreach(var g in laterLayouts)if(g){g.SetActive(false);Destroy(g);}laterLayouts.Clear();leftPaneButtons.Clear();rightPaneButtons.Clear();
        }
        RectTransform LaterGroup(string name,Transform parent,bool horizontal,float height=44)
        {
            var r=Rect(name,parent);laterLayouts.Add(r.gameObject);r.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=height;
            UnityEngine.UI.HorizontalOrVerticalLayoutGroup layout=horizontal?r.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>():r.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();layout.spacing=6;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;return r;
        }
        RectTransform Pane(string name,Transform parent,float width)
        {
            var r=Rect(name,parent);var element=r.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();element.preferredWidth=width;element.preferredHeight=355;
            var sr=r.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();sr.horizontal=false;sr.scrollSensitivity=32;sr.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var view=Rect("Viewport",r);Stretch(view,0,0,0,0);view.gameObject.AddComponent<UnityEngine.UI.Image>().color=new(0,0,0,.01f);view.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=false;
            var body=Rect("Rows",view);body.anchorMin=new(0,1);body.anchorMax=new(1,1);body.pivot=new(.5f,1);body.sizeDelta=Vector2.zero;
            var l=body.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();l.spacing=5;l.childControlWidth=l.childControlHeight=true;l.childForceExpandHeight=false;body.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;sr.viewport=view;sr.content=body;return body;
        }
        void TabRow(string[] labels,int chosen,Action<int> choose)
        {
            var tabs=LaterGroup("Visible tabs",content,true);tabs.SetSiblingIndex(details.gameObject.activeSelf?1:0);
            for(int i=0;i<labels.Length;i++){int n=i;Row(i,"tab-"+labels[i],(chosen==i?"✓ ":"")+labels[i],()=>choose(n));buttons[i].transform.SetParent(tabs,false);buttons[i].GetComponentInChildren<UnityEngine.UI.Text>(true).alignment=TextAnchor.MiddleCenter;}
        }
        void TableRow(int index,string id,string[] cells,float[] widths,Action click,bool emphasis=false)
        {
            Row(index,id,string.Join(" / ",cells),click);var b=buttons[index];b.GetComponentInChildren<UnityEngine.UI.Text>(true).gameObject.SetActive(false);
            var colors=b.colors;colors.normalColor=emphasis?new(.10f,.33f,.32f):index%2==0?new(.06f,.14f,.19f):new(.10f,.20f,.25f);b.colors=colors;
            float x=0;for(int i=0;i<cells.Length;i++){var t=Label("Cell",b.transform,20,0);tableCells.Add(t.gameObject);t.supportRichText=false;t.text=cells[i];t.alignment=i==cells.Length-1?TextAnchor.MiddleRight:TextAnchor.MiddleLeft;t.rectTransform.anchorMin=new(x,0);x+=widths[i];t.rectTransform.anchorMax=new(x,1);t.rectTransform.offsetMin=new(9,0);t.rectTransform.offsetMax=new(-9,0);}
        }
        bool RenderLater()
        {
            if(flow.State==RaceFlow.Stage.Playlists){RenderPlaylists();return true;}
            if(flow.State==RaceFlow.Stage.Boards||flow.State==RaceFlow.Stage.Activities){RenderRecords();return true;}
            if(flow.State==RaceFlow.Stage.Results){RenderResults();return true;}return false;
        }
        bool LaterBack()
        {
            if(flow.State!=RaceFlow.Stage.Playlists)return false;
            if(page=="playlist-dirty"){dirtyContinuation=null;page=pages.Pop();Show();return true;}
            if(page=="playlist-move"){moveFrom=-1;return false;}
            if(page==""&&rightPaneButtons.Any(b=>b.gameObject==EventSystem.current?.currentSelectedGameObject)&&leftPaneButtons.Count>0){EventSystem.current.SetSelectedGameObject(leftPaneButtons[0].gameObject);MenuInput.ConsumeThroughRelease();return true;}
            if(page==""&&PlaylistDirty){GuardPlaylist(()=>{playlistDraft=null;flow.PopMenu();});return true;}
            return false;
        }
        void GuardPlaylist(Action continuation,bool start=false)
        {
            if(!PlaylistDirty){continuation();return;}dirtyContinuation=continuation;dirtyStarting=start;Navigate("playlist-dirty");
        }
        void LoadDraft(int index)
        {
            draftIndex=index;playlistDraft=index<0?new RacePlaylists.Definition():RacePlaylists.Clone(flow.Playlists.Definitions[index]);savedDraft=index<0?"":JsonUtility.ToJson(playlistDraft);entryIndex=0;Show();if(rightPaneButtons.Count>0)EventSystem.current.SetSelectedGameObject(rightPaneButtons[0].gameObject);
        }
        bool SavePlaylistDraft()
        {
            if(!flow.Playlists.SaveDraft(draftIndex,playlistDraft)){Show();return false;}if(draftIndex<0)draftIndex=flow.Playlists.Definitions.Count-1;savedDraft=JsonUtility.ToJson(playlistDraft);return true;
        }
        void RenderPlaylists()
        {
            if(playlistDraft==null&&flow.Playlists.Definitions.Count>0){draftIndex=0;playlistDraft=RacePlaylists.Clone(flow.Playlists.Definitions[0]);savedDraft=JsonUtility.ToJson(playlistDraft);}
            if(page=="playlist-dirty")
            {
                ClearCore("UNSAVED CHANGES",flow.Playlists.Error??(dirtyStarting?"Save before starting this playlist?":"Save changes before leaving this playlist?"));
                Row(0,"cancel","CANCEL",()=>BackPage());Row(1,"save",dirtyStarting?"SAVE AND START":"SAVE CHANGES",()=>{if(!SavePlaylistDraft())return;var next=dirtyContinuation;dirtyContinuation=null;page=pages.Pop();next();});
                Row(2,"discard",dirtyStarting?"START WITHOUT SAVING":"DISCARD CHANGES",()=>{var next=dirtyContinuation;dirtyContinuation=null;page=pages.Pop();if(!dirtyStarting){playlistDraft=draftIndex>=0?RacePlaylists.Clone(flow.Playlists.Definitions[draftIndex]):null;savedDraft=playlistDraft==null?"":JsonUtility.ToJson(playlistDraft);}next();});return;
            }
            if(page=="playlist-editor")
            {
                ClearCore("RACE ENTRY",entryDraft.Title);int i=0;
                foreach(int course in RacePlaylists.DisplayOrder){int c=course;Row(i++,"edit-course-"+c,(entryDraft.course==c?"✓ ":"")+RacePlaylists.Titles[c],()=>{entryDraft.course=c;Show();});}
                Step(i++,"entry-laps","Laps: "+entryDraft.laps,d=>{entryDraft.laps=Mathf.Clamp(entryDraft.laps+d,1,5);Show();});
                Row(i++,"apply","APPLY",()=>{if(entryIndex<0)playlistDraft.entries.Add(entryDraft);else playlistDraft.entries[entryIndex]=entryDraft;BackPage();});Row(i,"cancel","Cancel",()=>BackPage());return;
            }
            if(page=="playlist-actions")
            {
                ClearCore("PLAYLIST ACTIONS",playlistDraft.name);
                Row(0,"back","Back",()=>BackPage());Row(1,"rename","Rename",()=>OpenKeyboard(playlistDraft.name,64,value=>{var name=new string(value.Where(c=>!char.IsControl(c)).Take(64).ToArray()).Trim();if(name.Length==0){keyboardError="Enter a name.";return false;}playlistDraft.name=name;return true;}));
                if(playlistDraft.entries.Count>0){entryIndex=Mathf.Clamp(entryIndex,0,playlistDraft.entries.Count-1);Row(2,"move","Move selected entry",()=>{moveFrom=moveTo=entryIndex;Navigate("playlist-move");EventSystem.current.SetSelectedGameObject(buttons[moveTo].gameObject);});Row(3,"remove","Remove selected entry",()=>Confirm("REMOVE RACE?",playlistDraft.entries[entryIndex].Title,()=>{playlistDraft.entries.RemoveAt(entryIndex);Show();}));}return;
            }
            if(page=="playlist-move")
            {
                ClearCore("MOVE RACE","Choose a destination row to preview. Select commits; Back cancels.");var preview=playlistDraft.entries.ToList();var moving=preview[moveFrom];preview.RemoveAt(moveFrom);preview.Insert(moveTo,moving);
                for(int i=0;i<preview.Count;i++){int target=i;Row(i,"move-"+i,(i==moveTo?"→ ":"")+(i+1)+". "+preview[i].Title,()=>{playlistDraft.entries=preview;entryIndex=target;moveFrom=-1;BackPage();});}
                Row(preview.Count,"cancel","Cancel move",()=>BackPage());return;
            }
            ClearCore("PLAYLISTS",playlistDraft==null?"Create a playlist to begin.":playlistDraft.name+(PlaylistDirty?"  ·  Unsaved Changes":"  ·  Saved")+(flow.Playlists.Error==null?"":"\nSave failed: "+flow.Playlists.Error));
            var actions=LaterGroup("Playlist actions",content,true);actions.SetSiblingIndex(1);int n=0;
            void ActionButton(string id,string label,Action action){Row(n,id,label,action);buttons[n++].transform.SetParent(actions,false);}
            ActionButton("back","Back",()=>{if(!BackPage())flow.PopMenu();});
            if(playlistDraft!=null){ActionButton("save","Save",()=>{SavePlaylistDraft();Show();});ActionButton("start","START",()=>GuardPlaylist(()=>flow.StartPlaylist(playlistDraft),true));buttons[n-1].interactable=playlistDraft.entries.Count>0;ActionButton("actions","Actions",()=>Navigate("playlist-actions"));if(playlistDraft.entries.Count==0)details.text+="\nAdd a race before starting.";}
            var split=LaterGroup("Saved playlists and races",content,true,355);split.SetSiblingIndex(2);var left=Pane("Saved playlists",split,260);var right=Pane("Race entries",split,670);
            for(int i=0;i<flow.Playlists.Definitions.Count;i++){int choice=i;Row(n,"saved-"+i,(draftIndex==i?"✓ ":"")+flow.Playlists.Definitions[i].name,()=>GuardPlaylist(()=>LoadDraft(choice)));buttons[n].transform.SetParent(left,false);buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=88;leftPaneButtons.Add(buttons[n++]);}
            Row(n,"new","+ New Playlist",()=>GuardPlaylist(()=>LoadDraft(-1)));buttons[n].transform.SetParent(left,false);buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight=88;leftPaneButtons.Add(buttons[n++]);
            if(playlistDraft!=null){TableRow(n++,"entry-header",new[]{"#","Track / direction","Laps"},new[]{.08f,.79f,.13f},()=>{});buttons[n-1].transform.SetParent(right,false);
                for(int i=0;i<playlistDraft.entries.Count;i++){int e=i;var entry=playlistDraft.entries[i];TableRow(n,"entry-"+i,new[]{(i+1).ToString(),RacePlaylists.Titles[entry.course],entry.laps.ToString()},new[]{.08f,.79f,.13f},()=>EditEntry(e));buttons[n].transform.SetParent(right,false);rightPaneButtons.Add(buttons[n++]);}
                Row(n,"add","+ Add Race",()=>EditEntry(-1));buttons[n].transform.SetParent(right,false);rightPaneButtons.Add(buttons[n]);}
        }
        void EditEntry(int index){entryIndex=index;entryDraft=index<0?new RacePlaylists.Entry():new RacePlaylists.Entry{course=playlistDraft.entries[index].course,laps=playlistDraft.entries[index].laps};Navigate("playlist-editor");}
        void ConfigureLaterFocus()
        {
            for(int i=0;i<leftPaneButtons.Count;i++){var nav=leftPaneButtons[i].navigation;nav.selectOnRight=rightPaneButtons.FirstOrDefault();leftPaneButtons[i].navigation=nav;}
            foreach(var b in rightPaneButtons){var nav=b.navigation;nav.selectOnLeft=leftPaneButtons.FirstOrDefault();b.navigation=nav;}
        }
    }
}
