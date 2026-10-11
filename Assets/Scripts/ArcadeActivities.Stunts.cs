using System;
using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    // 0.106 Part B: stunt scoring, built on the jump scoring. In Free Roam (and the Stunt event, which runs on it) every flight is watched for tricks:
    // the vehicle's turn about its own axes from takeoff to touchdown gives flips (forward / back, each full 360), rolls (sideways 360) and spins
    // (flat 360s); the stunt park's loop and its barrel-roll ramp are tricks of their own. A flight counts when it lands on its wheels (two or more
    // wheels on the ground for 0.3 s, not wiped out): its jump points (as the clean jump's: 10 a metre and 100 a second in the air) and its tricks,
    // shown like "BACKFLIP + 360 SPIN / 412 pts". Landing on the roof or side, or wiping out, is a bail: that flight's tricks are lost ("BAILED")
    // and the combo ends. Combo: each scored flight (or loop) within 2 s on the ground of the last one adds a link; when 2 s pass on the ground the
    // combo is banked as its points times its links (x2, x3, ...). Works for every vehicle; the stunt vehicles can steer in the air (ArcadeVehicle).
    // Never in races, the campaign or the police games; nothing changes in them.
    // 0.107 Part A (Dan: "far too unforgiving"): no bail on touching down at an angle. From the first touch (body or wheel) the vehicle has a
    // recovery window (RecoverWindow): back on its wheels and driving on (wheels down and upright for 0.3 s) is a landing with the tricks turned so
    // far ("SKETCHY LANDING" when it first touched 60 degrees or more over; full points). A bail only when the roof itself hits the ground hard (a
    // contact on the top of the body, more than about 100 degrees over), when the vehicle is still on its roof or side as the window ends, or a
    // wipe-out. Part B: a flight that takes off inside the park counts wherever it lands. Part D: 5 s on the ground to chain the next trick.
    // 0.108 Part D: every combo banked in the park (the Stunt event and Free Roam, both players) can enter the Top 10 Best Combos (StuntProgress),
    // with its links and its tricks in short form (BF backflip, FF frontflip, R roll, BR barrel roll, 360 spin, LOOP, J a plain jump).
    public sealed partial class ArcadeActivities
    {
        public const float ComboGap = 5f, MinStuntAir = .6f, MinStuntDistance = 4f, RecoverWindow = 1.5f, RoofUp = -.17f, RoofHit = 3f;
        public const int FlipPoints = 300, RollPoints = 250, SpinPoints = 150, LoopPoints = 500, BarrelPoints = 300, MaxLinks = 9;
        public bool StuntsOn => race && race.FreeRoam && CampaignRun.Active == null && !(SplitScreen.Active && SplitScreen.Mode == SplitScreen.Kind.Police);
        // a combo banked: its points (after the multiplier) and whether every link of it was in the stunt park
        public event Action<int, bool> ComboBanked;
        public int ComboLinks { get; private set; }
        public int ComboPoints { get; private set; }
        public bool ComboInPark { get; private set; } = true;
        public int ComboTotal => ComboPoints * Mathf.Max(1, ComboLinks);
        public int BestCombo { get; private set; }           // this session's best banked combo
        public int Tricks { get; private set; }               // tricks landed this session
        public int Bails { get; private set; }
        public int Loops { get; private set; }
        public string LastStunt { get; private set; } = "";   // the last scored flight's line (checks)
        public int LastComboLinks { get; private set; }        // 0.108 Part D: the last banked combo's links, its tricks (short form) and Top 10 place
        public string LastComboTricks { get; private set; } = "";
        public int LastComboPlace { get; private set; }
        // 0.108 Part A: the time left to make the next trick of the combo going (0: none)
        public float ComboLeft => ComboLinks > 0 ? Mathf.Max(0, ComboGap - sSince) : 0;
        public string StuntFeedback => Time.time < feedbackUntil ? Feedback : null;
        public int LastStuntPoints { get; private set; }
        public Vector3 LastStuntRotation { get; private set; } // pitch, yaw, roll turned in that flight (degrees)
        public float LastTouchTilt { get; private set; } = -1;  // the last flight's tilt from upright at its first touch (degrees; checks)
        public string LastBail { get; private set; } = "";
        public bool LastSketchy { get; private set; }
        // 0.107 Part E: in the air or going round the loop (the Stunt event waits for this at 0:00)
        public bool StuntBusy => sFlying || InLoop;
        // the flight; sTouch: time since its first touch (-1: not touched yet)
        bool sFlying, sBarrel; float sGround, sAir, sStable, sSince, sTouch = -1, sTilt = -1; Vector3 sTurn, sTakeoff, sLanding;
        // the loop
        float loopAcc, loopPrev = float.NaN, loopOut = 1;
        readonly List<string> comboTricks = new();
        string StuntHud
        {
            get
            {
                if (!StuntsOn) return "";
                if (ComboLinks > 0) return $"COMBO x{ComboLinks}  ·  {ComboTotal:N0} pts" + (ComboLinks < MaxLinks ? $"   (next within {Mathf.Max(0, ComboGap - sSince):0.0} s)" : "");
                return "";
            }
        }
        void StuntReset() { sFlying = false; sBarrel = false; sGround = sAir = sStable = 0; sTouch = sTilt = -1; sTurn = Vector3.zero; loopAcc = 0; loopPrev = float.NaN; loopOut = 1; }
        void StuntSession() { StuntReset(); ComboLinks = ComboPoints = 0; ComboInPark = true; sSince = 0; comboTricks.Clear(); }

        void StuntStep(Vector3 p, float dt, bool grounded)
        {
            if (!StuntsOn) { if (ComboLinks > 0) EndCombo(null); StuntReset(); return; }
            if (StuntPark.Current) LoopStep(p, dt);
            if (InLoop) { sFlying = false; sGround = Mathf.Max(sGround, .2f); return; } // round the loop the wheels may lift at the top: not a flight
            var w = car.transform.InverseTransformDirection(car.Body.angularVelocity) * (dt * Mathf.Rad2Deg);
            bool any = car.GroundedWheels > 0;
            if (!sFlying)
            {
                // the time on the ground since the last link (not counted while going round the loop)
                if (any) { sGround += dt; if (ComboLinks > 0 && !(StuntPark.Current && loopOut < .2f)) { sSince += dt; if (sSince >= ComboGap) EndCombo(null); } }
                else if (sGround >= .2f && car.Body.linearVelocity.magnitude > 3 && !configuration.WipedOut)
                {
                    sFlying = true; sAir = 0; sStable = 0; sTouch = sTilt = -1; sTurn = Vector3.zero; sTakeoff = previous;
                    sBarrel = StuntPark.Current && Vector3.Distance(previous, StuntPark.BarrelLip) < 7 && Vector3.Dot(car.Body.linearVelocity, StuntPark.BarrelForward) > 3;
                }
                else sGround = 0;
                return;
            }
            sTurn += w;
            if (configuration.WipedOut) { Bail(car.transform.up.y < RoofUp ? "Landed on the roof" : "Landed on the side"); return; }
            if (any) Touch();
            if (sTouch >= 0) sTouch += dt;
            // on the wheels: two or more down and upright (or every wheel down, as on a steep ramp face)
            bool wheels = grounded && (car.transform.up.y > .5f || car.GroundedWheels >= car.suspensionPoints.Length);
            if (!wheels)
            {
                if (!grounded) sAir += dt;
                sStable = 0;
                if (sTouch >= RecoverWindow)
                {
                    // the window is over: still lying on the roof or side is a bail; bounced back up into the air, the next touch starts a new window
                    if (Lying()) { Bail(car.transform.up.y < RoofUp ? "Landed on the roof" : "Landed on the side"); return; }
                    sTouch = -1;
                }
                return;
            }
            if (sStable == 0) sLanding = p;
            sStable += dt; if (sStable < .3f) return;
            // down on the wheels
            sFlying = false; sGround = sStable; LastTouchTilt = sTilt; LastSketchy = sTilt >= 60; LastBail = "";
            if (sAir < MinStuntAir || Vector3.ProjectOnPlane(sLanding - sTakeoff, Vector3.up).magnitude < MinStuntDistance && Mathf.Abs(sTurn.y) < 300) return; // a hop: no jump, and no break in the combo
            Landed();
        }
        // the tricks of a flight that ended on its wheels
        void Landed()
        {
            int flips = Mathf.RoundToInt(sTurn.x / 360), rolls = Mathf.RoundToInt(sTurn.z / 360), spins = Mathf.FloorToInt((Mathf.Abs(sTurn.y) + 30) / 360);
            var names = new List<string>(); var shortNames = new List<string>(); int points = 0;
            if (flips != 0) { int n = Mathf.Abs(flips); names.Add((n == 2 ? "DOUBLE " : n >= 3 ? "TRIPLE " : "") + (flips < 0 ? "BACKFLIP" : "FRONTFLIP")); shortNames.Add((n > 1 ? n.ToString() : "") + (flips < 0 ? "BF" : "FF")); points += FlipPoints * n; }
            if (rolls != 0) { int n = Mathf.Abs(rolls); names.Add((n == 2 ? "DOUBLE " : n >= 3 ? "TRIPLE " : "") + (sBarrel ? "BARREL ROLL" : "ROLL")); shortNames.Add((n > 1 ? n.ToString() : "") + (sBarrel ? "BR" : "R")); points += RollPoints * n + (sBarrel ? BarrelPoints : 0); }
            if (spins > 0) { names.Add(spins * 360 + " SPIN"); shortNames.Add((spins * 360).ToString()); points += SpinPoints * spins; }
            float distance = Vector3.ProjectOnPlane(sLanding - sTakeoff, Vector3.up).magnitude;
            int jump = Mathf.RoundToInt(Mathf.Min(distance, 250) * 10 + Mathf.Min(sAir, 12) * 100);
            Tricks += names.Count; LastStuntRotation = new Vector3(sTurn.x, sTurn.y, sTurn.z);
            string line = names.Count > 0 ? string.Join(" + ", names) : $"{(LastSketchy ? "JUMP" : "CLEAN JUMP")} / {DisplayUnits.Jump(distance)} / {sAir:0.00} s";
            if (LastSketchy) line = "SKETCHY LANDING\n" + line;
            Link(points + jump, line, StuntPark.OnPark(sTakeoff), shortNames.Count > 0 ? string.Join("+", shortNames) : "J"); // 0.107 Part B: took off in the park: counts wherever it lands
        }
        void Link(int points, string line, bool inPark, string trick)
        {
            if (ComboLinks < MaxLinks) ComboLinks++;
            comboTricks.Add(trick);
            ComboPoints += points; ComboInPark &= inPark; sSince = 0;
            LastStunt = line; LastStuntPoints = points;
            Message($"{line} / {points:N0} pts" + (ComboLinks > 1 ? $"\nCOMBO x{ComboLinks} · {ComboTotal:N0}" : ""), 3.5f);
        }
        // the end of a combo: banked (points x links), or with a bail
        void EndCombo(string bail)
        {
            if (ComboLinks <= 0) { if (bail != null) Message("BAILED / " + bail, 3.5f); return; }
            int total = ComboTotal; bool park = ComboInPark; int links = ComboLinks;
            ComboLinks = ComboPoints = 0; ComboInPark = true; sSince = 0;
            BestCombo = Mathf.Max(BestCombo, total);
            LastComboLinks = links; LastComboTricks = string.Join(" · ", comboTricks); comboTricks.Clear();
            // 0.108 Part D: the Top 10 Best Combos (in the park; in the Stunt event only while the player's round still counts; human players only)
            int player = car == race.vehicle ? 1 : 2; LastComboPlace = 0;
            if (park && !DeveloperLocationHud.Inspecting && StuntEvent.Counts(car) && (player == 1 || SplitScreen.Active && !SplitScreen.P2Ai))
                LastComboPlace = StuntProgress.RecordComboTop(SplitScreen.Active ? SplitScreen.NameOf(player) : PlayerNames.Player, configuration.profileId, total, links, LastComboTricks);
            string top = LastComboPlace > 0 ? $"\nNEW TOP 10 COMBO — #{LastComboPlace}" : "";
            if (bail != null) Message($"BAILED / {bail}\nCOMBO ENDED · x{links} · {total:N0} pts" + top, 4);
            else if (links > 1) Message($"COMBO x{links} / {total:N0} pts" + top, 3.5f);
            else if (top != "") Message($"{LastStunt} / {total:N0} pts" + top, 3.5f);
            ComboBanked?.Invoke(total, park);
            // the best single combo in Free Roam (player 1, alone; not the Stunt event)
            if (!SplitScreen.Active && car == race.vehicle && !DeveloperLocationHud.Inspecting) StuntProgress.RecordCombo(total, configuration.profileId);
        }
        void Bail(string why)
        {
            if (!sFlying) return;
            bool tried = sAir >= MinStuntAir || sTurn.magnitude > 90;
            sFlying = false; sGround = 0; Bails++; LastBail = why; LastTouchTilt = sTilt;
            if (tried || ComboLinks > 0) EndCombo(why);
        }
        // the first touch of a flight (a wheel or the body): its tilt from upright, and the recovery window starts
        void Touch() { if (sTouch >= 0) return; sTouch = 0; sTilt = Vector3.Angle(car.transform.up, Vector3.up); }
        // lying on the roof or side: tipped 60 degrees or more with the ground just below
        bool Lying() => car.transform.up.y < .5f && car.gameObject.scene.GetPhysicsScene().Raycast(car.Body.worldCenterOfMass, Vector3.down, out _, 2.5f, car.groundMask, QueryTriggerInteraction.Ignore);
        // ActivityLandingContact: a body contact starts the recovery window; only the roof itself hitting the ground hard is an instant bail
        bool InLoop => StuntPark.Current && loopOut < .2f && loopAcc > 60;
        void StuntContact(Vector3 normal, Vector3? point, float speed)
        {
            if (!StuntsOn || !sFlying || InLoop) return;
            Touch();
            if (normal.y > .4f && point.HasValue && speed >= RoofHit && car.transform.up.y < RoofUp && OnRoof(point.Value)) Bail("Landed on the roof");
        }
        // a contact on the top of the body (not its nose or tail)
        bool OnRoof(Vector3 point)
        {
            var box = car.GetComponent<BoxCollider>(); var q = car.transform.InverseTransformPoint(point);
            if (!box) return Vector3.Dot(point - car.Body.worldCenterOfMass, car.transform.up) > .3f;
            return q.y > box.center.y + box.size.y * .3f && Mathf.Abs(q.z - box.center.z) < box.size.z * .4f;
        }
        // the loop: once round (330 degrees or more about its centre, staying in it), on the wheels
        void LoopStep(Vector3 p, float dt)
        {
            var rel = p - StuntPark.LoopOrigin; float r = StuntPark.LoopRadius;
            float fwd = Vector3.Dot(rel, StuntPark.LoopForward), up = rel.y, lat = Vector3.Dot(rel, StuntPark.LoopSide);
            float dist = new Vector2(fwd, up - r).magnitude;
            bool inside = lat > -4 && lat < StuntPark.LoopShift + 4 && dist > r - 3.2f && dist < r + 1.5f && Mathf.Abs(fwd) < r + 2.5f;
            if (!inside) { loopOut += dt; if (loopOut > .4f) { loopAcc = 0; loopPrev = float.NaN; } return; }
            loopOut = 0; float phi = Mathf.Atan2(fwd, r - up) * Mathf.Rad2Deg;
            if (!float.IsNaN(loopPrev)) { float d = Mathf.DeltaAngle(loopPrev, phi); if (d > 0) loopAcc += d; else if (d < -15) loopAcc = 0; }
            loopPrev = phi;
            if (loopAcc >= 330 && car.GroundedWheels >= 2 && !configuration.WipedOut)
            {
                loopAcc = float.NegativeInfinity; Loops++; Tricks++; sFlying = false; sGround = 1;
                Link(LoopPoints, "LOOP", true, "LOOP");
            }
        }
    }
}
