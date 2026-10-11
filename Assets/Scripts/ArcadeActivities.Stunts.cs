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
    public sealed partial class ArcadeActivities
    {
        public const float ComboGap = 2f, MinStuntAir = .6f, MinStuntDistance = 4f;
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
        public int LastStuntPoints { get; private set; }
        public Vector3 LastStuntRotation { get; private set; } // pitch, yaw, roll turned in that flight (degrees)
        // the flight
        bool sFlying, sBarrel; float sGround, sAir, sStable, sSince; Vector3 sTurn, sTakeoff, sLanding;
        // the loop
        float loopAcc, loopPrev = float.NaN, loopOut = 1;
        string StuntHud
        {
            get
            {
                if (!StuntsOn) return "";
                if (ComboLinks > 0) return $"COMBO x{ComboLinks}  ·  {ComboTotal:N0} pts" + (ComboLinks < MaxLinks ? $"   (next within {Mathf.Max(0, ComboGap - sSince):0.0} s)" : "");
                return "";
            }
        }
        void StuntReset() { sFlying = false; sBarrel = false; sGround = sAir = sStable = 0; sTurn = Vector3.zero; loopAcc = 0; loopPrev = float.NaN; loopOut = 1; }
        void StuntSession() { StuntReset(); ComboLinks = ComboPoints = 0; ComboInPark = true; sSince = 0; }

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
                    sFlying = true; sAir = 0; sStable = 0; sTurn = Vector3.zero; sTakeoff = previous;
                    sBarrel = StuntPark.Current && Vector3.Distance(previous, StuntPark.BarrelLip) < 7 && Vector3.Dot(car.Body.linearVelocity, StuntPark.BarrelForward) > 3;
                }
                else sGround = 0;
                return;
            }
            sTurn += w;
            if (configuration.WipedOut) { Bail("Wiped out"); return; }
            if (!grounded) { sAir += dt; sStable = 0; return; }
            if (sStable == 0) sLanding = p;
            sStable += dt; if (sStable < .3f) return;
            // down on the wheels
            sFlying = false; sGround = sStable;
            if (sAir < MinStuntAir || Vector3.ProjectOnPlane(sLanding - sTakeoff, Vector3.up).magnitude < MinStuntDistance && Mathf.Abs(sTurn.y) < 300) return; // a hop: no jump, and no break in the combo
            Landed();
        }
        // the tricks of a flight that ended on its wheels
        void Landed()
        {
            int flips = Mathf.RoundToInt(sTurn.x / 360), rolls = Mathf.RoundToInt(sTurn.z / 360), spins = Mathf.FloorToInt((Mathf.Abs(sTurn.y) + 30) / 360);
            var names = new List<string>(); int points = 0;
            if (flips != 0) { int n = Mathf.Abs(flips); names.Add((n == 2 ? "DOUBLE " : n >= 3 ? "TRIPLE " : "") + (flips < 0 ? "BACKFLIP" : "FRONTFLIP")); points += FlipPoints * n; }
            if (rolls != 0) { int n = Mathf.Abs(rolls); names.Add((n == 2 ? "DOUBLE " : n >= 3 ? "TRIPLE " : "") + (sBarrel ? "BARREL ROLL" : "ROLL")); points += RollPoints * n + (sBarrel ? BarrelPoints : 0); }
            if (spins > 0) { names.Add(spins * 360 + " SPIN"); points += SpinPoints * spins; }
            float distance = Vector3.ProjectOnPlane(sLanding - sTakeoff, Vector3.up).magnitude;
            int jump = Mathf.RoundToInt(Mathf.Min(distance, 250) * 10 + Mathf.Min(sAir, 12) * 100);
            Tricks += names.Count; LastStuntRotation = new Vector3(sTurn.x, sTurn.y, sTurn.z);
            string line = names.Count > 0 ? string.Join(" + ", names) : $"CLEAN JUMP / {DisplayUnits.Jump(distance)} / {sAir:0.00} s";
            Link(points + jump, line, StuntPark.OnPark(sLanding) && StuntPark.OnPark(sTakeoff));
        }
        void Link(int points, string line, bool inPark)
        {
            if (ComboLinks < MaxLinks) ComboLinks++;
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
            if (bail != null) Message($"BAILED / {bail}\nCOMBO ENDED · x{links} · {total:N0} pts", 4);
            else if (links > 1) Message($"COMBO x{links} / {total:N0} pts", 3.5f);
            ComboBanked?.Invoke(total, park);
            // the best single combo in Free Roam (player 1, alone; not the Stunt event)
            if (!SplitScreen.Active && car == race.vehicle && !DeveloperLocationHud.Inspecting) StuntProgress.RecordCombo(total, configuration.profileId);
        }
        void Bail(string why)
        {
            if (!sFlying) return;
            bool tried = sAir >= MinStuntAir || sTurn.magnitude > 90;
            sFlying = false; sGround = 0; Bails++;
            if (tried || ComboLinks > 0) EndCombo(why);
        }
        // ActivityLandingContact: a hit while upside down or on the side ends the flight as a bail
        bool InLoop => StuntPark.Current && loopOut < .2f && loopAcc > 60;
        void StuntContact(Vector3 normal)
        {
            if (!StuntsOn || !sFlying || InLoop) return;
            if (normal.y > .4f && car.transform.up.y < .3f) Bail(car.transform.up.y < 0 ? "Landed on the roof" : "Landed on the side");
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
                Link(LoopPoints, "LOOP", true);
            }
        }
    }
}
