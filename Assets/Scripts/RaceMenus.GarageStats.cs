using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.82 Part D: five bars on the garage's vehicle page for the vehicle in the preview, drawn from its real profile
    // numbers and scaled across all ten vehicles so they compare honestly (the lowest of the ten shows a short bar, the
    // highest a full one). Display only: it sits in the option list under the Vehicle row (never over the preview) and
    // takes no selection, so controller and mouse navigation are unchanged.
    public sealed partial class RaceMenus
    {
        static readonly (string label, Func<VehicleProfile, float> value)[] GarageStats =
        {
            ("Top speed", p => p.Speed), ("Acceleration", p => p.Acceleration), ("Grip", p => p.Grip),
            ("Handling", p => p.Response), ("Weight / contact", p => p.Mass),
        };
        RectTransform statBlock; readonly List<RectTransform> statFills = new();
        public static float StatFraction(int stat, VehicleProfile p)
        {
            var values = VehicleProfile.All.Select(GarageStats[stat].value).ToArray(); float min = values.Min(), max = values.Max();
            return max > min ? Mathf.Lerp(.12f, 1, (GarageStats[stat].value(p) - min) / (max - min)) : 1;
        }
        public float[] ShownStats => statFills.Select(f => f.anchorMax.x).ToArray();

        void ShowGarageStats(VehicleProfile profile)
        {
            if (!statBlock)
            {
                statBlock = Rect("Vehicle stats", content); statBlock.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = GarageStats.Length * 24 + 8;
                var layout = statBlock.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); layout.spacing = 2; layout.padding = new RectOffset(10, 10, 4, 4);
                layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandHeight = false;
                foreach (var (label, _) in GarageStats)
                {
                    var row = Rect(label, statBlock); row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 22;
                    var text = Label("Label", row, 17, 0); text.rectTransform.anchorMin = new Vector2(0, 0); text.rectTransform.anchorMax = new Vector2(.36f, 1); text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
                    text.alignment = TextAnchor.MiddleLeft; text.text = label; text.color = new Color(.86f, .86f, .8f);
                    var track = Rect("Bar", row); track.anchorMin = new Vector2(.38f, .25f); track.anchorMax = new Vector2(1, .75f); track.offsetMin = track.offsetMax = Vector2.zero;
                    track.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, .12f);
                    var fill = Rect("Fill", track); fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0, 1); fill.offsetMin = fill.offsetMax = Vector2.zero;
                    fill.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.3f, 1, .88f); statFills.Add(fill);
                }
            }
            statBlock.gameObject.SetActive(true);
            for (int i = 0; i < statFills.Count; i++) statFills[i].anchorMax = new Vector2(StatFraction(i, profile), 1);
        }
    }
}
