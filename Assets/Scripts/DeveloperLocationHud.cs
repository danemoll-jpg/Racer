using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Racer
{
    [DefaultExecutionOrder(-900)]
    public sealed class DeveloperLocationHud : MonoBehaviour
    {
        public static DeveloperLocationHud Instance { get; private set; }
        static DebugReportSession session;
        static bool resumeChecked;
        public static bool DebugEnabled => Instance && Instance.visible;
        public static bool Inspecting => Instance && Instance.Flying;
        public static bool OwnsInput => Instance && (Instance.Holding || Time.frameCount <= Instance.releasedFrame);
        public static bool Interactive => Instance && (Instance.menuOpen || Instance.CommentOpen);
        public static DebugReportSession Session { get { ResumeOnce(); return session; } }
#if UNITY_EDITOR
        public static string ValidationReportRoot;
        // Simulates a fresh game launch: forget the in-memory session so the next use resumes from disk.
        public static void ResetValidationSession() { session = null; resumeChecked = false; }
#endif
        RaceDirector race;
        Font font;
        GameObject panel, shade, menuCard, commentCard, newSessionCard;
        UnityEngine.UI.Text label, status, captureDetails, sessionDetails, newSessionWarning;
        UnityEngine.UI.InputField comment;
        UnityEngine.UI.Button firstButton;
        UnityEngine.UI.Button flyButton, returnButton, exportButton, folderButton, newSessionButton, keepSessionButton;
        readonly List<UnityEngine.UI.Button> menuButtons = new();
        readonly List<(MenuGlyph glyph, UnityEngine.UI.Text key, string pad, string keyboard)> prompts = new();
        bool visible, hudVisible = true, menuOpen, capturing, held;
        bool oldInput, oldAudio, oldCursor, oldChase;
        float oldScale, nextRefresh, yaw, pitch;
        CursorLockMode oldLock;
        int releasedFrame = -1;
        Camera cameraView;
        ChaseCamera chase;
        Vector3 cameraPosition;
        Quaternion cameraRotation;
        DebugReportSession.Bug pending;
        Keyboard textKeyboard;
        float nextErase;
        public bool Flying { get; private set; }
        public bool CommentOpen { get; private set; }
        public bool NewSessionConfirmationOpen { get; private set; }
        public bool Visible => visible;
        public bool Capturing => capturing;
        public string LastExport { get; private set; }
        public string LastOpenedFolder { get; private set; }
        public string Error { get; private set; }
        public string HudText => label ? label.text : "";
        public string SessionText => Session == null ? "Session: NEW / Reports 0\nNext F4 starts BUG-001"
            : "Session: " + session.Id + "\n" + (session.Closed ? "CLOSED" : "OPEN") + " / Reports " + session.Count
                + (session.Closed ? " / Next F4: new BUG-001" : "");
        bool Holding => menuOpen || capturing || CommentOpen || Flying;
        bool Available => race && race.Flow && race.vehicle && race.Flow.State != RaceFlow.Stage.Title;

        public static void Create(Transform parent, RaceDirector owner, Font font)
        {
            var host = new GameObject("Developer inspection", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            var canvas = host.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 500;
            var scaler = host.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new(1280, 800); scaler.matchWidthOrHeight = .5f;
            var hud = host.AddComponent<DeveloperLocationHud>(); Instance = hud; hud.race = owner; hud.font = font; hud.BuildUi();
        }
        RectTransform Rect(string name, Transform parent, Vector2 size)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false); r.sizeDelta = size; return r;
        }
        UnityEngine.UI.Text Text(string name, Transform parent, int size)
        {
            var t = Rect(name, parent, Vector2.zero).gameObject.AddComponent<UnityEngine.UI.Text>();
            t.font = font; t.fontSize = size; t.color = Color.white; t.raycastTarget = false; t.supportRichText = false; return t;
        }
        void Fill(RectTransform r, float pad = 0) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new(pad, pad); r.offsetMax = new(-pad, -pad); }
        void Background(GameObject g, Color color) => g.AddComponent<UnityEngine.UI.Image>().color = color;
        UnityEngine.UI.Button Button(Transform parent, string title, Action action)
        {
            var r = Rect(title, parent, new(0, 38)); r.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 38;
            Background(r.gameObject, new(.12f, .25f, .29f)); var b = r.gameObject.AddComponent<UnityEngine.UI.Button>();
            b.targetGraphic = r.GetComponent<UnityEngine.UI.Image>(); var colors = b.colors; colors.highlightedColor = new(.4f, .9f, .8f); colors.selectedColor = colors.highlightedColor; b.colors = colors;
            var t = Text(title, r, 19); t.alignment = TextAnchor.MiddleCenter; t.text = title; Fill(t.rectTransform);
            b.onClick.AddListener(() => { if(MenuInput.UiBlocked)return; MenuInput.ConsumeThroughRelease(); action(); });
            if(menuCard && parent==menuCard.transform)menuButtons.Add(b);
            return b;
        }
        GameObject Card(string name, Vector2 size)
        {
            var r = Rect(name, shade.transform, size); r.anchorMin = r.anchorMax = new(.5f, .5f);
            Background(r.gameObject, new(.035f, .065f, .085f, .99f));
            var layout = r.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); layout.padding = new(24, 24, 20, 20); layout.spacing = 8;
            layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandHeight = false; return r.gameObject;
        }
        UnityEngine.UI.Text Row(Transform parent, string value, int size, float height)
        {
            var t = Text(value, parent, size); t.text = value; t.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = height; return t;
        }
        void BuildUi()
        {
            var p = Rect("Debug HUD", transform, new(430, 270)); panel = p.gameObject;
            p.anchorMin = p.anchorMax = p.pivot = new(1, 0); p.anchoredPosition = new(-18, 94);
            Background(panel, new(.025f, .055f, .07f, .94f)); label = Text("Debug state", p, 15); Fill(label.rectTransform, 12);
            var hb = Button(p, "Debug menu  F6", OpenMenu); var hr = (RectTransform)hb.transform;
            hr.anchorMin = new(0, 0); hr.anchorMax = new(1, 0); hr.pivot = new(.5f, 0); hr.offsetMin = new(8, 8); hr.offsetMax = new(-8, 38);
            var sr = Rect("Debug overlay", transform, Vector2.zero); Fill(sr); shade = sr.gameObject; Background(shade, new(0, .02f, .04f, .72f));
            menuCard = Card("Debug menu", new(660, 740));
            Row(menuCard.transform, "DEBUG / INSPECTION", 27, 40).color = new(.3f, .95f, .81f);
            sessionDetails = Row(menuCard.transform, SessionText, 16, 60);
            firstButton = Button(menuCard.transform, "Resume", CloseMenu);
            Button(menuCard.transform, "Capture Bug  F4", CaptureBug);
            flyButton = Button(menuCard.transform, "Debug Fly / Inspection", StartFly);
            returnButton = Button(menuCard.transform, "Return to Vehicle", () => { ReturnToVehicle(); CloseMenu(); });
            exportButton = Button(menuCard.transform, "Export Bug Report ZIP", Export);
            folderButton = Button(menuCard.transform, "Open Debug Report Folder", OpenFolder);
            newSessionButton = Button(menuCard.transform, "START NEW DEBUG SESSION", StartNewSession);
            Button(menuCard.transform, "Toggle Debug HUD", () => { hudVisible = !hudVisible; });
            Button(menuCard.transform, "Exit Debug Mode  F3", () => SetEnabled(false));
            // 0.89 campaign (the F6 menu opens only in Debug Mode): these write the campaign save even in Testing mode.
            var campaignRow = Rect("Campaign (debug)", menuCard.transform, new(0, 38)); campaignRow.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 38;
            var campaignLayout = campaignRow.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>(); campaignLayout.spacing = 6; campaignLayout.childControlWidth = campaignLayout.childControlHeight = true; campaignLayout.childForceExpandWidth = true;
            foreach (var (title, action) in new (string, Action)[] {
                ("Campaign +$5,000", () => { Campaign.DebugAddMoney(5000); CampaignDone("Campaign: +$5,000, now " + Campaign.Money(Campaign.Current.money)); }),
                ("Unlock campaign", () => { Campaign.DebugUnlockAll(); CampaignDone("Campaign: every vehicle, course, chapter and event unlocked"); }),
                ("Mark event won", () => CampaignDone("Campaign: marked won: " + Campaign.DebugMarkWon())),
                ("Reset campaign", () => { Campaign.Reset(); CampaignDone("Campaign reset to a new campaign"); }) })
            { var b = Button(campaignRow, title, action); b.GetComponentInChildren<UnityEngine.UI.Text>().fontSize = 15; menuButtons.Add(b); }
            var controls=Rect("Debug controls",menuCard.transform,new(0,32));controls.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=32;
            Prompt(controls,0,"Select","buttonSouth","enter");Prompt(controls,198,"Close","buttonEast","escape");Prompt(controls,396,"Navigate","dpad","arrows");
            status = Row(menuCard.transform, "F3 mode · F4 capture · F6 menu\nRace timeout suspended while Debug Mode is on.", 16, 50);
            commentCard = Card("Bug comment", new(680, 550));
            Row(commentCard.transform, "BUG CAPTURED", 28, 40).color = new(.3f, .95f, .81f);
            captureDetails = Row(commentCard.transform, "", 17, 62);
            Row(commentCard.transform, "Describe the problem:", 20, 28);
            var field = Rect("Comment", commentCard.transform, new(0, 156)); field.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 156;
            Background(field.gameObject, new(.12f, .22f, .27f)); comment = field.gameObject.AddComponent<UnityEngine.UI.InputField>(); comment.targetGraphic = field.GetComponent<UnityEngine.UI.Image>();
            var content = Text("Editable comment", field, 20); Fill(content.rectTransform, 12); comment.textComponent = content;
            comment.lineType = UnityEngine.UI.InputField.LineType.MultiLineNewline; comment.characterLimit = 4000;
            // Input System text events own insertion; the field retains native selection/caret rendering.
            comment.readOnly = true;
            Button(commentCard.transform, "Save  Enter", () => SaveComment(comment.text));
            Button(commentCard.transform, "Cancel  Esc / B", CancelComment);
            Row(commentCard.transform, "Shift+Enter: new line. Screenshot already saved before this dialog.", 15, 28);
            newSessionCard = Card("Confirm new debug session", new(660, 310));
            Row(newSessionCard.transform, "START NEW DEBUG SESSION?", 25, 36);
            newSessionWarning = Row(newSessionCard.transform, "", 18, 104);
            keepSessionButton = Button(newSessionCard.transform, "Keep Current Session", CancelNewSession);
            var closeSessionButton = Button(newSessionCard.transform, "Close Session Without Export", CloseCurrentSession);
            keepSessionButton.navigation = new UnityEngine.UI.Navigation { mode=UnityEngine.UI.Navigation.Mode.Explicit, selectOnUp=closeSessionButton, selectOnDown=closeSessionButton };
            closeSessionButton.navigation = new UnityEngine.UI.Navigation { mode=UnityEngine.UI.Navigation.Mode.Explicit, selectOnUp=keepSessionButton, selectOnDown=keepSessionButton };
            newSessionCard.SetActive(false);
            panel.SetActive(false); shade.SetActive(false); menuCard.SetActive(false); commentCard.SetActive(false);
        }
        void Prompt(Transform parent,float x,string title,string pad,string keyboard)
        {
            var icon=Rect(title+" binding",parent,new(48,30));icon.anchorMin=icon.anchorMax=icon.pivot=new(0,.5f);icon.anchoredPosition=new(x,0);
            var glyph=icon.gameObject.AddComponent<MenuGlyph>();glyph.raycastTarget=false;
            var key=Text("Key",icon,16);Fill(key.rectTransform);key.alignment=TextAnchor.MiddleCenter;
            var text=Text(title,parent,17);text.text=title;text.rectTransform.anchorMin=text.rectTransform.anchorMax=text.rectTransform.pivot=new(0,.5f);text.rectTransform.sizeDelta=new(140,30);text.rectTransform.anchoredPosition=new(x+54,0);text.alignment=TextAnchor.MiddleLeft;
            prompts.Add((glyph,key,"<Gamepad>/"+pad,"<Keyboard>/"+keyboard));
        }
        void RefreshMenu()
        {
            flyButton.interactable=!Flying; returnButton.interactable=Flying;
            ResumeOnce();
            exportButton.interactable=session!=null&&session.Count>0&&!session.Closed;
            folderButton.interactable=session!=null;
            sessionDetails.text=SessionText;
            var enabled=menuButtons.FindAll(b=>b.interactable);
            for(int i=0;i<enabled.Count;i++)enabled[i].navigation=new UnityEngine.UI.Navigation { mode=UnityEngine.UI.Navigation.Mode.Explicit, selectOnUp=enabled[(i+enabled.Count-1)%enabled.Count],selectOnDown=enabled[(i+1)%enabled.Count] };
            foreach(var p in prompts){var path=MenuInput.Controller?p.pad:p.keyboard;p.glyph.SetPath(path);p.key.text=MenuGlyph.Label(path);}
        }
        void CampaignDone(string message) { status.text = message + (Campaign.Error != null ? "\n" + Campaign.Error : ""); race.Flow.RefreshMenu(); }
        public string LocationText() => Available ? $"Position: {race.vehicle.transform.position} | Course: {race.courseName}" : "";
        public void CopyLocation() => GUIUtility.systemCopyBuffer = LocationText();
        public void Toggle() => SetEnabled(!visible);
        public void SetEnabled(bool value)
        {
            if (!Available || capturing || CommentOpen || NewSessionConfirmationOpen) return;
            visible = value; nextRefresh = 0;
            if (!value) { ReturnToVehicle(); menuOpen = false; shade.SetActive(false); SyncHold(); }
        }
        static string ReportRoot()
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(ValidationReportRoot)) return ValidationReportRoot;
#endif
            return Path.Combine(Application.persistentDataPath, "DebugReports");
        }
        // First use after launch continues the newest still-OPEN session from disk.
        static void ResumeOnce()
        {
            if (resumeChecked || session != null) return;
            resumeChecked = true;
            try { session = DebugReportSession.ResumeLatest(ReportRoot()); }
            catch (Exception e) { Debug.LogWarning("Debug session resume skipped: " + e.Message); }
        }
        void EnsureSession()
        {
            ResumeOnce();
            if (session != null && !session.Closed) return;
            session = new DebugReportSession(ReportRoot());
        }
        public void OpenMenu()
        {
            if (!visible || capturing || CommentOpen || NewSessionConfirmationOpen) return;
            menuOpen = true; shade.SetActive(true); menuCard.SetActive(true); commentCard.SetActive(false); SyncHold();
            RefreshMenu();MenuInput.ConsumeThroughRelease();EventSystem.current?.SetSelectedGameObject(firstButton.gameObject);
        }
        public void CloseMenu() { menuOpen = false; shade.SetActive(false); SyncHold(); }
        void SyncHold()
        {
            var input = race && race.vehicle ? race.vehicle.GetComponent<VehicleInput>() : null;
            if (Holding && !held)
            {
                oldScale = Time.timeScale; oldAudio = AudioListener.pause; oldInput = input && input.enabled;
                oldCursor = Cursor.visible; oldLock = Cursor.lockState; held = true;
                Time.timeScale = 0; AudioListener.pause = true; if (input) input.enabled = false;
            }
            if (!Holding && held)
            {
                Time.timeScale = oldScale; AudioListener.pause = oldAudio; if (input) input.enabled = oldInput;
                Cursor.visible = oldCursor; Cursor.lockState = oldLock; held = false; releasedFrame = Time.frameCount + 1;
                MenuInput.ConsumeThroughRelease(); EventSystem.current?.SetSelectedGameObject(null);
            }
            else if (Holding) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
        }
        public void CaptureBug()
        {
            if (!visible || !Available || capturing || CommentOpen || NewSessionConfirmationOpen) return;
            try { EnsureSession(); } catch (Exception e) { Fail(e); return; }
            var t = Flying && cameraView ? cameraView.transform : race.vehicle.transform;
            var b = race.Racers.Count > 0 ? race.Racers[0].Branch : null;
            pending = new DebugReportSession.Bug {
                id = session.NextId, timestamp = DateTimeOffset.Now.ToString("o"), position = t.position, rotation = t.eulerAngles, heading = t.eulerAngles.y,
                vehiclePosition = race.vehicle.transform.position, course = race.courseName, direction = race.reverseCourse ? "Reverse" : "Forward",
                mode = race.FreeRoam ? "Free Roam" : "Race", lap = race.FreeRoam ? 0 : race.Progress.CompletedLaps + 1,
                nextCheckpoint = race.FreeRoam ? -1 : race.Progress.NextGate, roadProgress = race.road ? race.road.Project(race.vehicle.transform.position, out _) : 0,
                branch = b?.Route ? b.Route.title : "Main", branchProgress = b?.Position ?? 0,
                vehicle = race.vehicle.GetComponent<VehicleConfiguration>().profileId, speedMps = Mathf.Abs(race.vehicle.ForwardSpeed),
                version = Application.version, buildGuid = Application.buildGUID, scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                viewpoint = Flying ? "Detached inspection camera" : "Vehicle", debugMovementUsed = race.Flow.DebugMovementUsed,
                conditions = WorldLook.Current ? WorldLook.Current.Conditions : "Look off"
            };
            pending.screenshot = "Screenshots/" + pending.id + ".png";
            menuOpen = false; capturing = true; shade.SetActive(false); EventSystem.current?.SetSelectedGameObject(null); SyncHold();
            StartCoroutine(CaptureView());
        }
        IEnumerator CaptureView()
        {
            yield return null;
            if (!Application.isBatchMode) yield return new WaitForEndOfFrame();
            Texture2D image = null;
            try
            {
                image = Application.isBatchMode ? BatchView() : ScreenCapture.CaptureScreenshotAsTexture();
                if (!image) throw new IOException("Could not read the rendered view.");
                File.WriteAllBytes(Path.Combine(session.DirectoryPath, pending.screenshot), image.EncodeToPNG());
                capturing = false; CommentOpen = true; comment.text = "";
                captureDetails.text = pending.id + " / " + pending.course + "\n" + pending.viewpoint + " / " + pending.position.ToString("F2");
                shade.SetActive(true); menuCard.SetActive(false); commentCard.SetActive(true); SyncHold();
                EventSystem.current?.SetSelectedGameObject(comment.gameObject); comment.ActivateInputField();
            }
            catch (Exception e) { capturing = false; pending = null; SyncHold(); Fail(e); }
            finally { if (image) Destroy(image); }
        }
        // Batch Editor verification has no end-of-frame callback. Render the same camera + live canvases explicitly.
        Texture2D BatchView()
        {
            var cam = Camera.main; var rt = new RenderTexture(1280, 800, 24); var oldTarget = cam.targetTexture; var active = RenderTexture.active;
            var canvases = FindObjectsByType<Canvas>(); var modes = new RenderMode[canvases.Length]; var cameras = new Camera[canvases.Length]; var distances = new float[canvases.Length];
            try
            {
                for (int i = 0; i < canvases.Length; i++) { var c = canvases[i]; modes[i] = c.renderMode; cameras[i] = c.worldCamera; distances[i] = c.planeDistance; if (c.renderMode == RenderMode.ScreenSpaceOverlay) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1; } }
                cam.targetTexture = rt; Canvas.ForceUpdateCanvases(); cam.Render(); RenderTexture.active = rt;
                var image = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); image.Apply(); return image;
            }
            finally { cam.targetTexture = oldTarget; RenderTexture.active = active; for (int i = 0; i < canvases.Length; i++) { canvases[i].renderMode = modes[i]; canvases[i].worldCamera = cameras[i]; canvases[i].planeDistance = distances[i]; } rt.Release(); Destroy(rt); }
        }
        public void SaveComment(string value)
        {
            if (!CommentOpen || pending == null) return;
            try { pending.comment = value ?? ""; session.Save(pending); string id = pending.id; pending = null; EndComment(); race.Flow.Notify(id + " saved / " + session.Count + " reports", 4); }
            catch (Exception e) { captureDetails.text = "Save failed. Your screenshot and comment are retained.\n" + e.Message; Error = e.Message; }
        }
        public void CancelComment()
        {
            if (!CommentOpen) return;
            try { if (pending != null && !session.Data.bugs.Contains(pending)) File.Delete(Path.Combine(session.DirectoryPath, pending.screenshot)); }
            catch (Exception e) { Error = e.Message; }
            pending = null; EndComment();
        }
        void EndComment() { comment.DeactivateInputField(); CommentOpen = false; shade.SetActive(false); SyncHold(); }
        void InsertComment(string value)
        {
            int a=Mathf.Min(comment.selectionAnchorPosition,comment.selectionFocusPosition),b=Mathf.Max(comment.selectionAnchorPosition,comment.selectionFocusPosition);
            string text=comment.text; a=Mathf.Clamp(a,0,text.Length);b=Mathf.Clamp(b,a,text.Length);
            int capacity=comment.characterLimit-(text.Length-(b-a));if(value.Length>capacity)value=value.Substring(0,Mathf.Max(0,capacity));
            comment.SetTextWithoutNotify(text.Remove(a,b-a).Insert(a,value));comment.caretPosition=a+value.Length;
        }
        void TypedCharacter(char value)
        {
            if(!CommentOpen||!comment.isFocused||Keyboard.current?.ctrlKey.isPressed==true)return;
            if(!char.IsControl(value))InsertComment(value.ToString());
        }
        void CommentInput(Keyboard k)
        {
            if(k==null||!comment.isFocused)return;
            if(k.ctrlKey.isPressed&&k.vKey.wasPressedThisFrame)InsertComment(GUIUtility.systemCopyBuffer);
            if(k.ctrlKey.isPressed&&k.xKey.wasPressedThisFrame){int a=Mathf.Min(comment.selectionAnchorPosition,comment.selectionFocusPosition),b=Mathf.Max(comment.selectionAnchorPosition,comment.selectionFocusPosition);GUIUtility.systemCopyBuffer=comment.text.Substring(a,b-a);InsertComment("");}
            if(k.shiftKey.isPressed&&k.enterKey.wasPressedThisFrame)InsertComment("\n");
            bool back=k.backspaceKey.isPressed,delete=k.deleteKey.isPressed;
            if((back||delete)&&(k.backspaceKey.wasPressedThisFrame||k.deleteKey.wasPressedThisFrame||Time.unscaledTime>=nextErase))
            {
                int a=Mathf.Min(comment.selectionAnchorPosition,comment.selectionFocusPosition),b=Mathf.Max(comment.selectionAnchorPosition,comment.selectionFocusPosition);
                if(a==b){if(back&&a>0)comment.selectionAnchorPosition=a-1;else if(delete&&b<comment.text.Length)comment.selectionFocusPosition=b+1;}
                InsertComment("");nextErase=Time.unscaledTime+((k.backspaceKey.wasPressedThisFrame||k.deleteKey.wasPressedThisFrame) ? .35f : .055f);
            }
        }
        public void Export()
        {
            if (capturing || CommentOpen || NewSessionConfirmationOpen || session == null || session.Closed || session.Count == 0) return;
            try { LastExport = session.Export(); status.text = "Session CLOSED. Next F4 starts BUG-001. ZIP:\n" + LastExport; Error = null; RefreshMenu(); EventSystem.current?.SetSelectedGameObject(firstButton.gameObject); }
            catch (Exception e) { Fail(e); }
        }
        public void OpenFolder()
        {
            if (session == null) return;
            try { LastOpenedFolder = session.DirectoryPath; System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(session.DirectoryPath) { UseShellExecute = true }); status.text = "Report folder:\n" + session.DirectoryPath; }
            catch (Exception e) { Fail(e); }
        }
        public void StartNewSession()
        {
            if (!menuOpen || capturing || CommentOpen || NewSessionConfirmationOpen) return;
            if (session != null && !session.Closed && session.Count > 0)
            {
                NewSessionConfirmationOpen = true; menuCard.SetActive(false); newSessionCard.SetActive(true);
                newSessionWarning.text = session.Id + "\n" + session.Count + " unexported reports. Close without export?\nFiles stay in history. Next F4 starts a new BUG-001.";
                MenuInput.ConsumeThroughRelease(); EventSystem.current?.SetSelectedGameObject(keepSessionButton.gameObject);
            }
            else CloseCurrentSession();
        }
        void CancelNewSession()
        {
            NewSessionConfirmationOpen=false;newSessionCard.SetActive(false);menuCard.SetActive(true);
            MenuInput.ConsumeThroughRelease();EventSystem.current?.SetSelectedGameObject(newSessionButton.gameObject);
        }
        void CloseCurrentSession()
        {
            try { session?.Close(); CancelNewSession(); RefreshMenu(); status.text="Session closed; history preserved.\nNext F4 creates a new session starting at BUG-001."; Error=null; }
            catch(Exception e) { CancelNewSession(); Fail(e); }
        }
        void Fail(Exception e) { Error = e.Message; if (status) status.text = e.Message; race.Flow.Notify("Debug report: " + e.Message, 8); }
        public void StartFly()
        {
            if (!visible || Flying || capturing || CommentOpen || !Available) return;
            cameraView = Camera.main; if (!cameraView) return;
            cameraPosition = cameraView.transform.position; cameraRotation = cameraView.transform.rotation;
            chase = cameraView.GetComponent<ChaseCamera>(); oldChase = chase && chase.enabled; if (chase) chase.enabled = false;
            yaw = cameraView.transform.eulerAngles.y; pitch = Mathf.DeltaAngle(0, cameraView.transform.eulerAngles.x);
            Flying = true; race.Flow.MarkDebugMovement(); race.Flow.Activities?.NewSession();
            menuOpen = false; shade.SetActive(false); EventSystem.current?.SetSelectedGameObject(null); SyncHold();
        }
        public void ReturnToVehicle()
        {
            if (!Flying) return; Flying = false;
            if (cameraView) cameraView.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
            if (chase) { chase.enabled = oldChase; if (oldChase) chase.Snap(); }
            if (race) { race.GetComponent<ExplorationCollection>()?.ResetMovement(); race.GetComponent<ExplorationMap>()?.ResetMovement(); }
            SyncHold();
        }
        void FlyInput()
        {
            var k = Keyboard.current; var g = Gamepad.current; var m = Mouse.current;
            float x = (k?.dKey.isPressed == true ? 1 : 0) - (k?.aKey.isPressed == true ? 1 : 0);
            float z = (k?.wKey.isPressed == true ? 1 : 0) - (k?.sKey.isPressed == true ? 1 : 0);
            float y = (k?.eKey.isPressed == true ? 1 : 0) - (k?.qKey.isPressed == true ? 1 : 0);
            var stick = g?.leftStick.ReadValue() ?? Vector2.zero; x += stick.x; z += stick.y; y += (g?.rightTrigger.ReadValue() ?? 0) - (g?.leftTrigger.ReadValue() ?? 0);
            Vector2 look = (g?.rightStick.ReadValue() ?? Vector2.zero) * (100 * Time.unscaledDeltaTime);
            if (m?.rightButton.isPressed == true) look += m.delta.ReadValue() * .13f;
            yaw += look.x; pitch = Mathf.Clamp(pitch - look.y, -89, 89); cameraView.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            float speed = k?.leftShiftKey.isPressed == true || g?.rightShoulder.isPressed == true ? 90 : k?.leftCtrlKey.isPressed == true || g?.leftShoulder.isPressed == true ? 3 : 22;
            var delta = cameraView.transform.right * x + cameraView.transform.forward * z + Vector3.up * y;
            cameraView.transform.position += Vector3.ClampMagnitude(delta, 1) * speed * Time.unscaledDeltaTime;
        }
        void Update()
        {
            if (!Available) return;
            var k = Keyboard.current; var g = Gamepad.current;
            if(textKeyboard!=k){if(textKeyboard!=null)textKeyboard.onTextInput-=TypedCharacter;textKeyboard=k;if(k!=null)k.onTextInput+=TypedCharacter;}
            if (NewSessionConfirmationOpen)
            {
                if(k?.escapeKey.wasPressedThisFrame==true||g?.buttonEast.wasPressedThisFrame==true)CancelNewSession();
                else if(!MenuInput.UiBlocked&&(k?.enterKey.wasPressedThisFrame==true||k?.numpadEnterKey.wasPressedThisFrame==true))
                {
                    var selected=EventSystem.current?.currentSelectedGameObject;
                    if(selected&&selected.transform.IsChildOf(newSessionCard.transform))ExecuteEvents.Execute(selected,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);
                }
                return;
            }
            if (CommentOpen)
            {
                CommentInput(k);
                if (k?.escapeKey.wasPressedThisFrame == true || g?.buttonEast.wasPressedThisFrame == true) CancelComment();
                else if ((k?.enterKey.wasPressedThisFrame == true || k?.numpadEnterKey.wasPressedThisFrame == true) && k?.shiftKey.isPressed != true) SaveComment(comment.text);
                return;
            }
            if (capturing) return;
            if (k?.f3Key.wasPressedThisFrame == true) Toggle();
            if (!visible) { panel.SetActive(false); return; }
            if (k?.f4Key.wasPressedThisFrame == true) { CaptureBug(); return; }
            if (k?.f6Key.wasPressedThisFrame == true || g?.startButton.wasPressedThisFrame == true) { if (menuOpen) CloseMenu(); else OpenMenu(); }
            if (menuOpen && (k?.escapeKey.wasPressedThisFrame == true || g?.buttonEast.wasPressedThisFrame == true)) CloseMenu();
            if(menuOpen)
            {
                RefreshMenu();
                // The ordinary menu reserves Enter for pause. Debug owns it as Select.
                if(!MenuInput.UiBlocked&&(k?.enterKey.wasPressedThisFrame==true||k?.numpadEnterKey.wasPressedThisFrame==true))
                {
                    var selected=EventSystem.current?.currentSelectedGameObject;
                    if(selected&&selected.transform.IsChildOf(menuCard.transform))ExecuteEvents.Execute(selected,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);
                }
            }
            if (Flying && !menuOpen) FlyInput();
            panel.SetActive(hudVisible && !menuOpen);
            if (Time.unscaledTime < nextRefresh) return; nextRefresh = Time.unscaledTime + .1f;
            var t = Flying && cameraView ? cameraView.transform : race.vehicle.transform; var p = t.position;
            label.text = $"DEBUG / {race.courseName}\n{(race.reverseCourse ? "Reverse" : "Forward")} / {(race.FreeRoam ? "Free Roam" : "Race")}\nX {p.x:F2}   Y {p.y:F2}   Z {p.z:F2}\nHeading {t.eulerAngles.y:F1}° / {race.vehicle.GetComponent<VehicleConfiguration>().profileId} / {DisplayUnits.Mph(Mathf.Abs(race.vehicle.ForwardSpeed)):F1} mph\n"
                + (race.FreeRoam ? "Exploration" : $"Lap {race.Progress.CompletedLaps + 1} / Next CP {race.Progress.NextGate}")
                + "\n" + (WorldLook.Current ? WorldLook.Current.Conditions : "Look off")
                + "\n" + SessionText + "\nF3 mode / F4 capture / Timeout OFF\n"
                + (Flying ? "FLY: WASD · Q/E · RMB look\nShift fast / Ctrl precise · F6 return" : race.Flow.DebugMovementUsed ? "DEBUG RUN / records disabled" : "Vehicle view / records eligible");
        }
        void OnDestroy()
        {
            if(textKeyboard!=null)textKeyboard.onTextInput-=TypedCharacter;
            if (Instance != this) return;
            ReturnToVehicle(); menuOpen = capturing = CommentOpen = false; SyncHold(); Instance = null;
        }
    }
}
