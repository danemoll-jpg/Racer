#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Racer {
// 0.99 targeted checks (muted, isolated save), added in front of the 0.98 chain:
//   setup99   controller only (no mouse, no keyboard): Settings > Gameplay's two new rows (Hidden police On / Off and its difficulty), the Cop vehicle row in the Cop vs Runner and
//             Speed Patrol setups, the Garage's two locked police silhouettes with their goals; every control reachable, focus kept, B goes back
public sealed partial class Report080Checks {
 IEnumerator Run099(string[] a) => a[0] switch { "setup99" => Setup099(), "garage99" => Garage099(), _ => Run098(a) };

 IEnumerator Setup099() {
  yield return DanCopy095(); Pads090(); SplitScreen.Solo = true; SplitScreen.P2Device = null; SplitScreen.CopVehicle = "police"; PoliceProgressForCheck();
  // Settings > Gameplay
  yield return Choose091("settings"); var s = flow.Save.Settings; bool on0 = s.hiddenPolice; int d0 = s.hiddenPoliceDifficulty;
  yield return Goto091("hidden-police"); yield return Press091(GamepadButton.DpadRight); Check(s.hiddenPolice != on0, $"Hidden police row: right on the D-pad flips it ({on0} -> {s.hiddenPolice}); label '{Sel91.GetComponentInChildren<UnityEngine.UI.Text>().text.Trim()}'");
  yield return Press091(GamepadButton.DpadLeft); Check(s.hiddenPolice == on0, "Hidden police row: left flips it back");
  yield return Goto091("hidden-police-difficulty"); yield return Press091(GamepadButton.DpadRight); int d1 = s.hiddenPoliceDifficulty; yield return Press091(GamepadButton.DpadRight); int d2 = s.hiddenPoliceDifficulty;
  Check(d1 == (d0 + 1) % 3 && d2 == (d0 + 2) % 3, $"Hidden police difficulty row: right steps Easy / Normal / Hard and wraps ({new[] { "Easy", "Normal", "Hard" }[d0]} -> {new[] { "Easy", "Normal", "Hard" }[d1]} -> {new[] { "Easy", "Normal", "Hard" }[d2]}); label '{Sel91.GetComponentInChildren<UnityEngine.UI.Text>().text.Trim()}'");
  yield return Late(() => Shot4k("99-settings-gameplay")); yield return Walk091("Settings-Gameplay", false);
  s.hiddenPoliceDifficulty = d0; s.hiddenPolice = on0; yield return Press091(GamepadButton.East); yield return Settle091();
  // the Cop vs Runner setup (1 player) and the Speed Patrol setup
  yield return Choose091("police"); Check(Page90 == "police", $"POLICE CHASE opens its setup ({Page90})");
  yield return Goto091("police-copvehicle"); Check(SplitScreen.CopVehicle == "police", $"Cop vs Runner setup has the 'Cop vehicle' row, Patrol Car first (row label '{Sel91.GetComponentInChildren<UnityEngine.UI.Text>().text.Trim()}')");
  yield return Press091(GamepadButton.DpadRight); Check(SplitScreen.CopVehicle == "policebike", $"right on the D-pad chooses the Police Bike ({SplitScreen.CopVehicle}); details '{Details095}'");
  yield return Late(() => Shot4k("99-setup-copvsrunner-bike")); yield return Walk091("Cop vs Runner setup (1 player), bike chosen", true, () => Page90 == "");
  yield return Choose091("police"); yield return Goto091("police-game"); yield return PressUntil098(() => SplitScreen.PoliceGame == SplitScreen.Game.SpeedPatrol, GamepadButton.DpadRight, 8);
  yield return Goto091("police-copvehicle"); yield return Press091(GamepadButton.DpadLeft); Check(SplitScreen.CopVehicle == "police", "Speed Patrol setup: the Cop vehicle row steps back to the Patrol Car with left");
  yield return Press091(GamepadButton.DpadRight); Check(SplitScreen.CopVehicle == "policebike", "Speed Patrol setup: right chooses the Police Bike"); yield return Late(() => Shot4k("99-setup-patrol-bike"));
  yield return Walk091("Speed Patrol setup (1 player), bike chosen", true, () => Page90 == ""); SplitScreen.CopVehicle = "police";
  yield return Garage099(false);
 }
 IEnumerator Garage099(bool fresh = true) {
  if (fresh) { yield return DanCopy095(); Pads090(); PoliceProgressForCheck(); }
  // the garage: the two police vehicles are locked silhouettes with their goals (no police progress on this isolated save)
  yield return Choose091("garage"); yield return Settle091(); var lockedSeen = new List<string>();
  for (int i = 0; i < 40 && !(lockedSeen.Contains("police") && lockedSeen.Contains("policebike")); i++)
  {
   yield return Goto091("profile-0"); yield return Press091(GamepadButton.DpadRight); yield return Settle091();
   var locked = Menus089.GarageLockedVehicle; if (locked != null && !lockedSeen.Contains(locked)) { lockedSeen.Add(locked); Note($"garage: locked '{locked}' -> title '{Title095}'; details '{Details095}'"); yield return Late(() => Shot4k("99-garage-locked-" + locked)); }
  }
  Check(lockedSeen.Contains("police") && lockedSeen.Contains("policebike"), $"the garage list ends with the locked police vehicles: {string.Join(", ", lockedSeen)}; the goals read '{PoliceProgress.CarGoal}' / '{PoliceProgress.BikeGoal}'");
  yield return Walk091("Garage with a locked vehicle", false); yield return Press091(GamepadButton.East); yield return Settle091();
 }
 void PoliceProgressForCheck() { PoliceProgress.Load(Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT"), "no-police-progress")); }
}
}
#endif
