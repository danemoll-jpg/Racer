using UnityEngine;
namespace Racer
{
    // One authored culvert/grate, driven by the same active-course/free-roam state
    // as existing course closures. Free roam leaves the drain accessible.
    public sealed class ReverseShortcutWorldState : MonoBehaviour
    {
        public GameObject grate;
        public GameObject ridgeGate;
        RaceDirector race;
        void OnEnable() { race=FindAnyObjectByType<RaceDirector>(); Refresh(); }
        public void Refresh()
        {
            bool closed=race && !race.FreeRoam && !race.reverseCourse;
            if(grate && grate.activeSelf!=closed) grate.SetActive(closed);
            if(ridgeGate && ridgeGate.activeSelf!=closed) ridgeGate.SetActive(closed);
        }
        void Update()=>Refresh();
    }
}
