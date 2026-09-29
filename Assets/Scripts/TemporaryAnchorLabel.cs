using UnityEngine;
namespace Racer {
    // Presentation only for the temporary, collider-free coordinate references.
    public sealed class TemporaryAnchorLabel : MonoBehaviour {
        void LateUpdate() {
            var camera=Camera.main;
            if(camera)transform.rotation=camera.transform.rotation;
        }
    }
}
