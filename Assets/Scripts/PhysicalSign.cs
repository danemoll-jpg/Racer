using UnityEngine;
namespace Racer
{
    // Marks lettering with a physical backing. Never blanket-disable world-space text.
    public sealed class PhysicalSign : MonoBehaviour { }
    public static class SceneryText
    {
        public static bool IsFloating(TextMesh text)
        {
            if (text.GetComponentInParent<PhysicalSign>()) return false;
            // Only known unmounted authoring labels; unknown/new text is preserved for review.
            return text.name == "Gate label" || text.name == "Direction arrow" ||
                text.name == "Race boundary sign" ||
                text.name.StartsWith("J1 ") || text.name.StartsWith("J2 ") || text.name.StartsWith("J3 ") || text.name.StartsWith("J4 ") || text.name.StartsWith("J5 ") || text.name.StartsWith("J6 ") ||
                text.name == "FOREST LOOP / MOTO + ATV" ||
                text.name == "ECHO CAVE / OPTIONAL / BRAKE FOR BENDS" || text.name == "CAVE GAP / STRAIGHT LANDING" || text.name == "CAVE LEFT / BRAKE FOR TURN";
        }
    }
}

