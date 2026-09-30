using UnityEngine;

namespace Racer
{
    // Only authored in Backyard Reverse. The visible timber and its collision
    // leave together in free roam; Forward uses its unchanged scene.
    public sealed class BackyardReverseBarriers : MonoBehaviour
    {
        public RaceDirector race;
        public GameObject[] closures;
        public void Refresh()
        {
            bool active = race && race.courseId == "backyard-reverse-v1-main" && !race.FreeRoam;
            foreach (var closure in closures)
                if (closure && closure.activeSelf != active) closure.SetActive(active);
        }
        void OnEnable() => Refresh();
        void Update() => Refresh();
    }
}
