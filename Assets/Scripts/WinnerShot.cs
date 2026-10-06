using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.82 Part E: the winner's celebration on camera. When the player's race ends in a race with opponents, the finish
    // panel waits about 2.5 s while the camera shows the winner from the front three-quarter side, following them: the
    // player if they won (their two-fist celebration from the line is playing), otherwise the AI winner, who raises both
    // fists again for the camera. Then the camera goes back and the finish panel and results come up as before. The confirm
    // button (A / Space / click) skips it. Camera and timing only: finishing order, times and records are not touched.
    public sealed class WinnerShot : MonoBehaviour
    {
        public const float Seconds = 2.5f;
        static WinnerShot current;
        public static bool Active => current && current.enabled;
        public static int Shots { get; private set; }
        public static string LastWinner { get; private set; } = "";
        ArcadeVehicle winner; float started; ChaseCamera chase; CameraViews views; bool chaseWas, viewsWas; InputAction skip;
        RaceFlow flow;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetProcess() { current = null; Shots = 0; }

        public static void Begin(RaceFlow flow, RacerState won, bool playerWon)
        {
            if (won == null || !won.Car || TrailerMode.Active || !Camera.main) return;
            if (!current) current = flow.gameObject.AddComponent<WinnerShot>();
            current.Play(flow, won.Car, playerWon); LastWinner = won.Name;
        }
        void Play(RaceFlow owner, ArcadeVehicle car, bool playerWon)
        {
            flow = owner; winner = car; started = Time.unscaledTime; enabled = true; Shots++;
            if (!playerWon) winner.GetComponent<RiderGestures>()?.Celebrate();
            chase = FindAnyObjectByType<ChaseCamera>(); views = CameraViews.Current;
            chaseWas = chase && chase.enabled; viewsWas = views && views.enabled;
            if (chase) chase.enabled = false; if (views) views.enabled = false;
            if (skip == null) { skip = new InputAction("Skip winner shot", InputActionType.Button); skip.AddBinding("<Gamepad>/buttonSouth"); skip.AddBinding("<Keyboard>/space"); skip.AddBinding("<Mouse>/leftButton"); }
            skip.Enable();
        }
        public static void Skip() { if (Active) current.End(); }
        void LateUpdate()
        {
            if (!winner || !flow || flow.State != RaceFlow.Stage.Racing || TrailerMode.Active || Time.unscaledTime - started >= Seconds || (Time.unscaledTime - started > .2f && skip.WasPressedThisFrame())) { End(); return; }
            var cam = Camera.main; if (!cam) return;
            var w = winner.transform; var fwd = Vector3.ProjectOnPlane(w.forward, Vector3.up).normalized; var right = Vector3.Cross(Vector3.up, fwd);
            var size = winner.GetComponent<VehicleConfiguration>()?.Profile.Size ?? new Vector3(1, 1, 3);
            float reach = Mathf.Max(4.5f, size.z * 1.3f);
            var eye = w.position + fwd * reach + right * reach * .6f + Vector3.up * 1.5f;
            cam.transform.position = eye; cam.transform.LookAt(w.position + Vector3.up * .9f);
        }
        void End()
        {
            enabled = false; skip?.Disable();
            if (chase) chase.enabled = chaseWas; if (views) views.enabled = viewsWas;
        }
        void OnDestroy() { if (enabled) End(); skip?.Dispose(); if (current == this) current = null; }
    }
}
