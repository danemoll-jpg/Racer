using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Racer
{
    // 0.77 Part C: the player's view in ordinary racing and Free Roam: Chase (the 0.76 camera, unchanged, the default), Far
    // chase, First person and Front. V / X (controller) cycles them and the choice is saved; races stay eligible for records
    // in every view. ChaseCamera keeps tracking the chase pose underneath every view; the other views are placed here, after
    // it, ease out to that chase pose during a wipeout or reset and come back afterwards. The Trailer Mode cameras (Parts A
    // and B) are in CameraViews.Trailer.cs. Nothing here acts while ChaseCamera is disabled (debug fly, evidence tools that
    // place the camera themselves): the scene's own camera settings are put back first.
    [DefaultExecutionOrder(50)]
    public sealed partial class CameraViews : MonoBehaviour
    {
        public enum View { Chase, FarChase, FirstPerson, Front }
        public static readonly string[] Names = { "Chase", "Far chase", "First person", "Front" };
        public static CameraViews Current { get; private set; }
        struct Pose
        {
            public Vector3 position; public Quaternion rotation; public float fov, near;
            public Pose(Vector3 p, Quaternion r, float f, float n) { position = p; rotation = r; fov = f; near = n; }
            public static Pose Lerp(Pose a, Pose b, float t) => new(Vector3.Lerp(a.position, b.position, t), Quaternion.Slerp(a.rotation, b.rotation, t), Mathf.Lerp(a.fov, b.fov, t), Mathf.Lerp(a.near, b.near, t));
        }
        RaceFlow flow; Camera cam; ChaseCamera chase; InputAction cycle;
        public InputAction CycleAction => cycle;
        // The saved choice (read from the settings, so a different save in use is followed too).
        public View PlayerView => SplitScreen.Active ? View.Chase : (View)Mathf.Clamp(flow.Save.Settings.cameraView, 0, 3); // 0.90 Part D: the chase view only in split-screen
        public string PlayerViewName => flow && flow.Save != null ? Names[(int)PlayerView] : null;
        public void NextPlayerView(int d = 1) => SetPlayerView((View)(((int)PlayerView + d + Names.Length) % Names.Length));
        View lastView;
        float baseFov, baseNear, baseSmooth, baseHeading;
        bool touched;
        // 1 = the chosen view, 0 = eased out to the chase pose (wipeout, reset); calmSince: when the vehicle last settled.
        float viewWeight = 1, calmSince = -1;
        Pose last, blendFrom; float blend = 1, blendLength = .35f;
        public string ShownView { get; private set; } = "Chase";
        public bool HeadHidden => hidden.Count > 0;

        public static void Attach(RaceFlow owner)
        {
            var camera = Camera.main; if (!camera) return;
            var chase = camera.GetComponent<ChaseCamera>(); if (!chase) return;
            var views = camera.GetComponent<CameraViews>(); if (!views) views = camera.gameObject.AddComponent<CameraViews>();
            views.flow = owner; views.cam = camera; views.chase = chase;
            views.baseFov = camera.fieldOfView; views.baseNear = camera.nearClipPlane;
            views.baseSmooth = chase.positionSmoothTime; views.baseHeading = chase.headingResponse;
            views.lastView = views.PlayerView;
            views.last = new Pose(camera.transform.position, camera.transform.rotation, camera.fieldOfView, camera.nearClipPlane);
            Current = views;
        }
        void Awake()
        {
            cycle = new InputAction("Change view", InputActionType.Button);
            cycle.AddBinding("<Keyboard>/v"); cycle.AddBinding("<Gamepad>/buttonWest"); cycle.Enable();
        }
        void OnDestroy() { cycle?.Dispose(); if (Current == this) Current = null; }
        void Update()
        {
            if (!flow || flow.Save == null || TrailerMode.Active) return;
            bool driving = flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Countdown;
            if (!driving || MenuInput.Blocked || flow.GetComponent<ExplorationMap>()?.OwnsInput == true || SplitScreen.Active) return;
            if (cycle.WasPressedThisFrame()) NextPlayerView();
        }
        public void SetPlayerView(View view)
        {
            if (view == PlayerView) return;
            flow.Save.Settings.cameraView = (int)view; flow.Save.SaveSettings(); // 0.79: RaceHud shows the camera hint with the new view
        }
        void StartBlend(float length = .35f) { blendFrom = last; blend = 0; blendLength = length; }

        void LateUpdate()
        {
            if (!flow || flow.Save == null || !cam || !chase) return;
            var car = flow.Race.vehicle;
            if (!chase.enabled || !car) { Release(); return; }
            Settle(car);
            if (lastView != PlayerView) { lastView = PlayerView; StartBlend(); }
            var chasePose = new Pose(chase.ChasePosition, chase.ChaseRotation, baseFov, baseNear);
            Pose pose; string shown;
            if (TrailerMode.Active) { pose = TrailerPose(chasePose, car, out shown); }
            else
            {
                trailerShot = null;
                chase.offsetScale = PlayerView == View.FarChase ? new Vector3(1, 1.55f, 1.5f) : Vector3.one;
                chase.positionSmoothTime = baseSmooth; chase.headingResponse = baseHeading;
                bool own = PlayerView == View.FirstPerson || PlayerView == View.Front;
                pose = own ? Pose.Lerp(chasePose, PlayerView == View.FirstPerson ? FirstPerson(car, chasePose) : FrontView(car, chasePose), Mathf.SmoothStep(0, 1, viewWeight)) : chasePose;
                shown = Names[(int)PlayerView];
                if (!own && blend >= 1 && PlayerView == View.Chase) { Hide(null, false); if (touched) Release(); last = chasePose; ShownView = shown; return; }
            }
            if (blend < 1) { blend = Mathf.Min(1, blend + Time.unscaledDeltaTime / blendLength); pose = Pose.Lerp(blendFrom, pose, Mathf.SmoothStep(0, 1, blend)); }
            // 0.83 Part F: the player's head parts are hidden exactly while this frame's camera is inside the head, decided
            // from the pose actually used (after any blend), so no view - first person, a blend into or out of it, a wipeout
            // ease-out or a Trailer Mode camera - ever shows a bald, faceless rider or the inside of the head.
            Hide(car, InsideHead(car, pose.position));
            transform.SetPositionAndRotation(pose.position, pose.rotation);
            cam.fieldOfView = pose.fov; cam.nearClipPlane = pose.near; touched = true; last = pose; ShownView = shown;
        }
        // The scene's own camera settings, the plain chase camera and a visible head.
        void Release()
        {
            Hide(null, false);
            if (!touched) return;
            touched = false; cam.fieldOfView = baseFov; cam.nearClipPlane = baseNear;
            chase.offsetScale = Vector3.one; chase.positionSmoothTime = baseSmooth; chase.headingResponse = baseHeading;
        }
        // Wipeouts and resets: first person and front ease out to the chase pose until the vehicle is upright again and has
        // been settled for 0.4 s, then ease back in.
        void Settle(ArcadeVehicle car)
        {
            var config = car.GetComponent<VehicleConfiguration>(); var respawn = car.GetComponent<VehicleRespawn>();
            bool rough = (config && config.WipedOut) || (respawn && respawn.Pending) || car.transform.up.y < .35f;
            if (rough) calmSince = -1; else if (calmSince < 0) calmSince = Time.time;
            bool settled = calmSince >= 0 && Time.time - calmSince > .4f;
            viewWeight = Mathf.MoveTowards(viewWeight, settled ? 1 : 0, Time.unscaledDeltaTime / (settled ? .45f : .3f));
        }
        public bool Rough => viewWeight < .99f;

        // ---------- first person ----------
        Transform visual; Transform eyeHost; Vector3 eyeLocal; bool classicEyes;
        readonly List<(Transform part, Vector3 centre)> eyeParts = new();
        Quaternion fpRotation; float fpY; bool fpFresh = true; Transform fpFor;
        // The rider's eyes: the Blender rider's "__eyes" part; the classic models' head (merged into their body meshes) is
        // left behind the camera, which sits just in front of the face.
        bool Eyes(ArcadeVehicle car, out Vector3 eye, out Vector3 forward)
        {
            var root = car.transform.Find("Vehicle visual");
            if (root != visual)
            {
                visual = root; eyeParts.Clear(); eyeHost = null; classicEyes = false; hidden.Clear(); fpFresh = true;
                if (root)
                {
                    foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                        // 0.78 fix: only the face's eyes (the Shorts socks share the "eyes" material slot).
                        if (r.name.EndsWith("__eyes") && r.name.Contains("_Base_") && r.GetComponent<MeshFilter>() is MeshFilter f && f.sharedMesh) eyeParts.Add((r.transform, f.sharedMesh.bounds.center));
                    if (eyeParts.Count == 0)
                    {
                        var p = car.GetComponent<VehicleConfiguration>().Profile; classicEyes = true; eyeHost = root;
                        // VehicleVisual.Person: head at hip + (0, .735, .18) * scale; the camera 8 cm ahead of the face.
                        eyeLocal = p.Small ? new Vector3(0, 1.26f, .10f) : new Vector3(-.37f, .72f, .43f);
                    }
                }
            }
            eye = forward = default;
            forward = car.transform.forward;
            if (classicEyes) { if (!eyeHost) return false; eye = eyeHost.TransformPoint(eyeLocal); return true; }
            if (eyeParts.Count == 0 || !eyeParts[0].part) return false;
            foreach (var (part, centre) in eyeParts) eye += part.TransformPoint(centre);
            // Blender rider (Tools/Blender/rider.py): eyes 9 cm ahead of the head centre, the nose tip 12 cm. The camera sits
            // 4.5 cm in front of the eyes, just clear of the face, so the head (one mesh with the hands) is behind it.
            eye = eye / eyeParts.Count + forward * .045f;
            return true;
        }
        Pose FirstPerson(ArcadeVehicle car, Pose fallback)
        {
            if (!Eyes(car, out var eye, out _)) return fallback;
            // Leans a little with the motorcycle (40 %), a small look-ahead into corners; bumps and landings are softened.
            var config = car.GetComponent<VehicleConfiguration>();
            float lean = visual && config.Profile.Motorcycle ? Mathf.DeltaAngle(0, visual.localEulerAngles.z) * .4f : 0;
            float look = Mathf.Clamp(car.Body.angularVelocity.y * Mathf.Rad2Deg * .22f, -10, 10);
            // Looking a little down: the bars, grips, hands and front wheel at the bottom of the view (cars: the wheel and dash).
            // The Trail Four's grips sit lower and nearer under the rider's eyes than the Needle 600's.
            bool atv = config.profileId == "atv", small = config.Profile.Small;
            return Mounted(car, eye, Quaternion.Euler(atv ? 24 : small ? 16 : 5, look, lean), baseFov + (atv ? 15 : small ? 10 : 7));
        }
        // Front: clear of the bodywork - bumper level on the cars, just ahead of the bars on the motorcycle and ATV.
        Pose FrontView(ArcadeVehicle car, Pose fallback)
        {
            var p = car.GetComponent<VehicleConfiguration>().Profile;
            var local = p.Small ? new Vector3(0, p.Motorcycle ? (p.Id == "scrambler" ? 1.2f : .98f) : .92f, p.Size.z * .5f - .1f) : new Vector3(0, .3f, p.Size.z * .5f + .06f);
            float look = Mathf.Clamp(car.Body.angularVelocity.y * Mathf.Rad2Deg * .12f, -6, 6);
            return Mounted(car, car.transform.TransformPoint(local), Quaternion.Euler(0, look, 0), baseFov + 5);
        }
        Pose Mounted(ArcadeVehicle car, Vector3 at, Quaternion extra, float fov)
        {
            var target = car.transform.rotation * extra;
            if (fpFresh || fpFor != car.transform) { fpRotation = target; fpY = at.y; fpFresh = false; fpFor = car.transform; }
            float dt = Time.deltaTime;
            fpRotation = Quaternion.Slerp(fpRotation, target, 1 - Mathf.Exp(-12 * dt));
            fpY = Mathf.Lerp(fpY, at.y, 1 - Mathf.Exp(-30 * dt));
            at.y += Mathf.Clamp(fpY - at.y, -.08f, .08f);
            return new Pose(at, fpRotation, fov, .05f);
        }
        // The camera is inside the rider's head (hair and hat included): within 17 cm of the head centre, which is 13.5 cm
        // behind the first-person eye point (Eyes: the camera sits 4.5 cm ahead of the eyes, the eyes 9 cm ahead of the centre).
        bool InsideHead(ArcadeVehicle car, Vector3 camera)
        {
            if (!car || !Eyes(car, out var eye, out var forward) || classicEyes) return false;
            return (camera - (eye - forward * .135f)).sqrMagnitude < .17f * .17f;
        }
        // 0.83 Part F: an outside camera that takes over from this one (the winner shot) shows the whole rider.
        public void ShowHead() => Hide(null, false);
        // The player's own face details, hair, brows and hat cast their shadow but are not drawn in first person.
        readonly List<(Renderer renderer, ShadowCastingMode mode)> hidden = new();
        void Hide(ArcadeVehicle car, bool value)
        {
            if (!value)
            {
                foreach (var (r, mode) in hidden) if (r) r.shadowCastingMode = mode;
                hidden.Clear(); return;
            }
            if (hidden.Count > 0 && hidden[0].renderer) return;
            hidden.Clear();
            if (!Eyes(car, out _, out _) || classicEyes) return;
            foreach (var r in visual.GetComponentsInChildren<Renderer>(true))
            {
                var n = r.name; int cut = n.IndexOf("__"); if (cut < 0) continue;
                string slot = n.Substring(cut + 2);
                bool head = n.Contains("_Hair_") || n.Contains("_HairTop_") || n.Contains("_Hat_") || (n.Contains("_Base_") && (slot == "eyes" || slot == "pupil" || slot == "mouth" || slot == "hair"));
                if (!head) continue;
                hidden.Add((r, r.shadowCastingMode)); r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }
    }
}
