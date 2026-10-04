using UnityEngine;

namespace Racer
{
    // 0.74: things that exist only in Free Roam (the Backyard Reverse storm-drain tunnel brought into the scenes of the other
    // courses). While Free Roam runs: the content is shown, the listed terrain tiles and batched tree meshes use their Free
    // Roam shapes (the gully and the tunnel mouths, trees re-grounded on them) and the listed objects stand at their Free
    // Roam positions. In races and menus everything is exactly as authored, so the other courses' races are unchanged.
    public sealed class FreeRoamOnly : MonoBehaviour
    {
        public GameObject content;
        public MeshFilter[] meshes; public Mesh[] roamMeshes;
        public Transform[] moved; public Vector3[] roamPositions;
        Mesh[] raceMeshes, raceColliders; Vector3[] racePositions; RaceDirector race; int shown = -1;
        public bool Shown => shown == 1;
        void Awake()
        {
            race = FindAnyObjectByType<RaceDirector>();
            raceMeshes = new Mesh[meshes?.Length ?? 0]; raceColliders = new Mesh[raceMeshes.Length];
            for (int i = 0; i < raceMeshes.Length; i++) { raceMeshes[i] = meshes[i] ? meshes[i].sharedMesh : null; var c = meshes[i] ? meshes[i].GetComponent<MeshCollider>() : null; raceColliders[i] = c ? c.sharedMesh : null; }
            racePositions = new Vector3[moved?.Length ?? 0]; for (int i = 0; i < racePositions.Length; i++) racePositions[i] = moved[i] ? moved[i].position : Vector3.zero;
            Apply(false);
        }
        void Update() { bool roam = race && race.FreeRoam; if ((roam ? 1 : 0) != shown) Apply(roam); }
        void Apply(bool roam)
        {
            shown = roam ? 1 : 0;
            if (content) content.SetActive(roam);
            for (int i = 0; i < raceMeshes.Length; i++)
            {
                var mf = meshes[i]; if (!mf) continue; var m = roam && roamMeshes[i] ? roamMeshes[i] : raceMeshes[i];
                if (mf.sharedMesh == m) continue; mf.sharedMesh = m;
                var mc = mf.GetComponent<MeshCollider>(); if (mc) { mc.sharedMesh = null; mc.sharedMesh = roam && roamMeshes[i] ? m : raceColliders[i]; }
            }
            for (int i = 0; i < racePositions.Length; i++) if (moved[i]) moved[i].position = roam ? roamPositions[i] : racePositions[i];
            Physics.SyncTransforms();
        }
    }
}
