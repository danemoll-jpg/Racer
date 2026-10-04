using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Racer
{
    public sealed partial class RaceMenus
    {
        string folderDraft="",folderError="";
        string[] directoryRows=Array.Empty<string>();
        Task<(string[] paths,string error)> directoryTask;
        int directoryOffset;
        string enumeratingFolder;
        public static bool ValidDirectory(string path)
        {
            try{
                if(string.IsNullOrWhiteSpace(path)||!Path.IsPathFullyQualified(path)||path.StartsWith(@"\\")||path.StartsWith("//")||path.Contains("://")||!Directory.Exists(path))return false;
                if(new DriveInfo(Path.GetPathRoot(path)).DriveType==DriveType.Network)return false;
                for(var directory=new DirectoryInfo(path);directory!=null;directory=directory.Parent)if((directory.Attributes&FileAttributes.ReparsePoint)!=0)return false;
                return true;
            }
            catch{return false;}
        }
        void OpenFolderPicker(){folderDraft=ValidDirectory(flow.Radio.Folder)?Path.GetFullPath(flow.Radio.Folder):"";directoryOffset=0;Navigate("folder");ReadDirectories();}
        void ReadDirectories()
        {
            string path=folderDraft;int offset=directoryOffset;enumeratingFolder=path;directoryRows=Array.Empty<string>();folderError="Loading folders…";
            directoryTask=Task.Run(()=>{
                try{
                    if(path=="")return(DriveInfo.GetDrives().Where(d=>d.DriveType!=DriveType.Network&&d.IsReady).Select(d=>d.RootDirectory.FullName).ToArray(),"");
                    if(!ValidDirectory(path))return(Array.Empty<string>(),"Folder missing, invalid, or a directory link. Choose a local folder.");
                    // Bounded pages; directory links are never traversed. Enumeration stays off the UI thread.
                    var rows=Directory.EnumerateDirectories(path).Where(p=>{try{return (File.GetAttributes(p)&FileAttributes.ReparsePoint)==0;}catch{return false;}}).Skip(offset).Take(65).ToArray();
                    return(rows,"");
                }catch(UnauthorizedAccessException){return(Array.Empty<string>(),"Access denied. Choose another folder.");}
                catch(Exception){return(Array.Empty<string>(),"Folder unavailable. Return to its parent or choose another root.");}
            });Show();
        }
        void UpdateFolder()
        {
            if(page!="folder"||directoryTask==null||!directoryTask.IsCompleted)return;
            var task=directoryTask;directoryTask=null;
            if(enumeratingFolder!=folderDraft)return;
            var result=task.IsCompletedSuccessfully?task.Result:(Array.Empty<string>(),"Unable to read folder.");
            directoryRows=result.Item1;folderError=result.Item2;Show();
        }
        void FolderBack()
        {
            if(folderDraft!="") {try{folderDraft=Directory.GetParent(folderDraft)?.FullName??"";}catch{folderDraft="";}directoryOffset=0;ReadDirectories();}
            else {directoryTask=null;page=pages.Count>0?pages.Pop():"library";MenuInput.ConsumeThroughRelease();Show();}
        }
        void RenderFolder()
        {
            ClearCore("CHOOSE MUSIC FOLDER",(folderDraft==""?"Local roots":folderDraft)+"\n"+folderError);
            Row(0,"cancel","Cancel",()=>{directoryTask=null;page=pages.Count>0?pages.Pop():"library";Show();});
            Row(1,"use-folder","USE THIS FOLDER",()=>{
                if(!ValidDirectory(folderDraft)){folderError="Select a valid local folder first.";Show();return;}
                try{using var check=Directory.EnumerateFileSystemEntries(folderDraft).GetEnumerator();check.MoveNext();}
                catch{folderError="Access denied. Folder was not changed.";Show();return;}
                flow.Radio.SetFolder(Path.GetFullPath(folderDraft));directoryTask=null;page=pages.Pop();Show();});
            buttons[1].interactable=ValidDirectory(folderDraft)&&directoryTask==null&&folderError=="";
            Row(2,"parent","Parent Folder",FolderBack);Row(3,"roots","Local Roots",()=>{folderDraft="";directoryOffset=0;ReadDirectories();});
            Row(4,"path","Enter Path…",()=>OpenKeyboard(folderDraft,1024,value=>{if(!ValidDirectory(value)){keyboardError="Enter an existing local absolute folder path.";return false;}folderDraft=Path.GetFullPath(value);directoryOffset=0;return true;},()=>ReadDirectories()));
            for(int i=0;i<Math.Min(64,directoryRows.Length);i++){string path=directoryRows[i];Row(i+5,"directory-"+path,(folderDraft==""?path:Path.GetFileName(path)),()=>{folderDraft=path;directoryOffset=0;ReadDirectories();});}
            if(directoryOffset>0)Row(70,"previous-folders","Previous folders",()=>{directoryOffset=Math.Max(0,directoryOffset-64);ReadDirectories();});
            if(directoryRows.Length>64)Row(71,"more-folders","More folders",()=>{directoryOffset+=64;ReadDirectories();});
        }
        RectTransform keyGrid,keyUtility;
        UnityEngine.UI.InputField draftField;
        string keyboardDraft="",keyboardError="";
        int keyboardLimit,caret;
        bool shift;
        Keyboard textKeyboard;
        Func<string,bool> saveText;
        Action afterText;
        const string Keys="1234567890qwertyuiopasdfghjkl;zxcvbnm,./\\:-_@()[]";
        void OpenKeyboard(string value,int limit,Func<string,bool> save,Action after=null)
        {
            keyboardDraft=value??"";caret=keyboardDraft.Length;keyboardLimit=limit;saveText=save;afterText=after;keyboardError="";Navigate("keyboard");
        }
        void EnsureKeyboard()
        {
            if(keyGrid)return;
            keyGrid=Rect("On-screen keyboard",content);
            var grid=keyGrid.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();grid.cellSize=new(84,44);grid.spacing=new(8,8);grid.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=10;
            keyGrid.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=260;
            keyUtility=Rect("Keyboard utilities",content);var utilityGrid=keyUtility.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();utilityGrid.cellSize=new(222,44);utilityGrid.spacing=new(8,8);utilityGrid.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;utilityGrid.constraintCount=4;keyUtility.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=100;
            var rect=Rect("Shared text draft",content);rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=48;
            var bg=rect.gameObject.AddComponent<UnityEngine.UI.Image>();bg.color=new(.08f,.19f,.23f);
            draftField=rect.gameObject.AddComponent<UnityEngine.UI.InputField>();draftField.targetGraphic=bg;
            var label=Label("Draft",rect,22,0);Stretch(label.rectTransform,12,4,-12,-4);draftField.textComponent=label;draftField.lineType=UnityEngine.UI.InputField.LineType.SingleLine;
            draftField.onValueChanged.AddListener(value=>{keyboardDraft=value;caret=draftField.caretPosition;});
        }
        void RenderKeyboard()
        {
            EnsureKeyboard();ClearCore("TEXT ENTRY",keyboardError);keyGrid.gameObject.SetActive(true);keyUtility.gameObject.SetActive(true);draftField.gameObject.SetActive(true);
            draftField.characterLimit=keyboardLimit;draftField.SetTextWithoutNotify(keyboardDraft);draftField.caretPosition=caret;
            draftField.transform.SetSiblingIndex(1);keyGrid.SetSiblingIndex(2);keyUtility.SetSiblingIndex(3);
            for(int i=0;i<Keys.Length;i++){char key=shift?char.ToUpperInvariant(Keys[i]):Keys[i];Row(i,"key-"+i,key.ToString(),()=>InsertText(key.ToString()));buttons[i].transform.SetParent(keyGrid,false);}
            int n=Keys.Length;
            Row(n,"shift",shift?"Shift: ON":"Shift",()=>{shift=!shift;Show();});Row(n+1,"caret-left","Caret ←",()=>{caret=Math.Max(0,caret-1);RefreshDraft();});Row(n+2,"caret-right","Caret →",()=>{caret=Math.Min(keyboardDraft.Length,caret+1);RefreshDraft();});
            Row(n+3,"delete","Delete",DeleteText);Row(n+4,"space","Space",()=>InsertText(" "));Row(n+5,"save","SAVE",()=>CloseKeyboard(true));Row(n+6,"cancel","Cancel",()=>CloseKeyboard(false));
            for(int i=n;i<=n+6;i++)buttons[i].transform.SetParent(keyUtility,false);
        }
        void RefreshDraft(){draftField.SetTextWithoutNotify(keyboardDraft);draftField.caretPosition=caret;}
        void InsertText(string value){if(keyboardDraft.Length+value.Length>keyboardLimit)return;caret=Mathf.Clamp(caret,0,keyboardDraft.Length);keyboardDraft=keyboardDraft.Insert(caret,value);caret+=value.Length;RefreshDraft();}
        void DeleteText(){caret=Mathf.Clamp(caret,0,keyboardDraft.Length);if(caret==0)return;keyboardDraft=keyboardDraft.Remove(--caret,1);RefreshDraft();}
        void CloseKeyboard(bool save)
        {
            if(save&&!saveText(keyboardDraft)){Show();return;}
            draftField.DeactivateInputField();page=pages.Pop();MenuInput.ConsumeThroughRelease();Show();if(save)afterText?.Invoke();
        }
        void UpdateKeyboard()
        {
            if(page!="keyboard")return;
            if(textKeyboard!=Keyboard.current){if(textKeyboard!=null)textKeyboard.onTextInput-=TypedCharacter;textKeyboard=Keyboard.current;if(textKeyboard!=null)textKeyboard.onTextInput+=TypedCharacter;}
            var k=Keyboard.current;var g=Gamepad.current;
            if(deleteAction.WasPressedThisFrame()&&!draftField.isFocused){DeleteText();MenuInput.ConsumeThroughRelease();}
            else if(spaceAction.WasPressedThisFrame()&&spaceAction.activeControl?.device is Gamepad){InsertText(" ");MenuInput.ConsumeThroughRelease();}
            if(k?.enterKey.wasPressedThisFrame==true){CloseKeyboard(true);return;}
            if(!draftField.isFocused&&k!=null){

                if(k.vKey.wasPressedThisFrame&&(k.leftCtrlKey.isPressed||k.rightCtrlKey.isPressed))InsertText(GUIUtility.systemCopyBuffer);
            }
            if(MenuInput.Controller&&draftField.isFocused){caret=draftField.caretPosition;draftField.DeactivateInputField();EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);}
        }
        void TypedCharacter(char value){if(page=="keyboard"&&!draftField.isFocused&&!char.IsControl(value)&&Keyboard.current?.ctrlKey.isPressed!=true){InsertText(value.ToString());MenuInput.ConsumeThroughRelease();}}
        void ResetEntryLayout()
        {
            if(!keyGrid)return;
            foreach(var button in buttons)if((button.transform.parent==keyGrid||button.transform.parent==keyUtility))button.transform.SetParent(content,false);
            keyGrid.gameObject.SetActive(false);keyUtility.gameObject.SetActive(false);draftField.gameObject.SetActive(false);
        }
        void ConfigureCoreFocus()
        {
            if(flow.State==RaceFlow.Stage.Garage&&page!="rider"){
                var profiles=buttons.Take(4).Where(b=>b.gameObject.activeSelf).ToArray();
                var chosen=swatches[Mathf.Clamp(flow.SelectedColor,0,swatches.Count-1)];
                // The Model row (0.73) and the Rider row (0.75) sit between the swatches and Done, in that order.
                var rows=new[]{5,6}.Where(i=>buttons.Count>i&&buttons[i].gameObject.activeSelf).Select(i=>buttons[i]).ToList();var below=rows.Count>0?rows[0]:buttons[4];
                for(int i=0;i<swatches.Count;i++){var nav=swatches[i].navigation;nav.selectOnUp=profiles.Last();nav.selectOnDown=below;swatches[i].navigation=nav;}
                var last=profiles.Last().navigation;last.selectOnDown=chosen;profiles.Last().navigation=last;
                for(int i=0;i<rows.Count;i++){var m=rows[i].navigation;m.selectOnUp=i==0?chosen:rows[i-1];m.selectOnDown=i+1<rows.Count?rows[i+1]:buttons[4];rows[i].navigation=m;}
                var back=buttons[4].navigation;back.selectOnUp=rows.Count==0?chosen:rows[^1];buttons[4].navigation=back;
            }
            if(page!="keyboard")return;
            for(int i=Keys.Length;i<=Keys.Length+6;i++){
                var nav=buttons[i].navigation;nav.selectOnLeft=buttons[Math.Max(Keys.Length,i-1)];nav.selectOnRight=buttons[Math.Min(Keys.Length+6,i+1)];nav.selectOnUp=buttons[Math.Max(0,i-4)];nav.selectOnDown=buttons[Math.Min(Keys.Length+6,i+4)];buttons[i].navigation=nav;
            }
            for(int i=0;i<Keys.Length;i++)
            {
                var nav=buttons[i].navigation;nav.selectOnLeft=buttons[Math.Max(0,i-1)];nav.selectOnRight=buttons[Math.Min(Keys.Length-1,i+1)];nav.selectOnUp=buttons[Math.Max(0,i-10)];nav.selectOnDown=buttons[Math.Min(Keys.Length,i+10)];buttons[i].navigation=nav;
            }
        }
    }
}
