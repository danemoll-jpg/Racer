using System;
using System.Linq;
using UnityEngine;
namespace Racer
{
    // Explicit isolated validation launches only. Never writes player preferences.
    [DefaultExecutionOrder(32000)]
    public sealed class TestAudioMute:MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot(){var args=Environment.GetCommandLineArgs();if(!args.Contains("-racerTestSave")||args.Contains("-racerAudioCheck"))return;var go=new GameObject("Temporary test-only audio mute");DontDestroyOnLoad(go);go.AddComponent<TestAudioMute>();AudioListener.volume=0;}
        void LateUpdate(){AudioListener.volume=0;}
    }
}
