using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 1.3.0-review4 evidence only (Report106PlayerCheck: -p106check <dir> -racerTestSave <dir>, a copy of Dan's save):
    //   -p106kind map1300r4   the campaign with the controller only: main menu > CAMPAIGN > Dan's run; for each chapter's events below, the event row
    //                         highlighted and the map beside the list shot (the park events, a speed trap, two jumps, a smash, tricks, land it clean,
    //                         and a race for comparison), the caption noted; chapter 5's Park Competition reached by D-pad down from Red Clay Session
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator Map1300r4()
        {
            Conditions(); yield return PanelLoop("map-start"); yield return ToCampaign(); yield return ToSlot(false);
            string Caption() { var t = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).FirstOrDefault(x => x.name == "Course" && x.transform.parent && x.transform.parent.name == "Course map panel"); return t ? t.text.Replace("\n", " | ") : "(no caption)"; }
            var shots = new (int chapter, string id)[] { (5, "c5-park-session"), (5, "c5-park-competition"), (1, "c1-hwy92-trap"), (2, "c2-opening-jump"), (3, "c3-fence-smash"), (4, "c4-summit-flight"), (5, "c5-summit-tricks"), (5, "c5-land-it-clean"), (5, "c5-stunt-opener"), (1, "c1-first-lap") };
            int open = -1;
            foreach (var (chapter, id) in shots)
            {
                if (open != chapter) { open = chapter; yield return Choose("chapter-" + chapter); yield return new WaitForSecondsRealtime(1); if (!ActiveButton("cev-" + id)) { Note($"  chapter {chapter} not opened by A (locked on this save): opened directly"); Menu.OpenCampaign(chapter); yield return new WaitForSecondsRealtime(1); } }
                if (id == "c5-park-competition" && Focus == "cev-c5-park-session") yield return Press(GamepadButton.DpadDown); else yield return FocusOn("cev-" + id);
                yield return new WaitForSecondsRealtime(.8f);
                Note($"{id}: focus '{Focus}', page '{Page}', caption: {Caption()}");
                yield return Snap($"map-{Screen.width}x{Screen.height}-{id}");
            }
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f); Note($"B: page '{Page}'");
        }
    }
}
