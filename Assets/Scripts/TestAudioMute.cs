using System;
using System.Linq;
using UnityEngine;
namespace Racer
{
    // Explicit isolated validation launches only. Never writes player preferences.
    [DefaultExecutionOrder(32000)]
    public sealed class TestAudioMute:MonoBehaviour
    {
        static float audibleUntil;
        public static void BriefAudibleCheck(float seconds){if(FindAnyObjectByType<TestAudioMute>())audibleUntil=Time.realtimeSinceStartup+Mathf.Clamp(seconds,0,2);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot(){var args=Environment.GetCommandLineArgs();if(!args.Contains("-racerTestSave")||args.Contains("-racerAudioCheck"))return;var go=new GameObject("Temporary test-only audio mute");DontDestroyOnLoad(go);go.AddComponent<TestAudioMute>();AudioListener.volume=0;}
        void LateUpdate(){AudioListener.volume=Time.realtimeSinceStartup<audibleUntil?.5f:0;}
    }
}
