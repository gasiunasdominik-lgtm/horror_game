using UnityEngine;

namespace Vchod.DebugTools
{
    /// <summary>
    /// Spawns the debug tools by code, so no scene references a type that does not
    /// exist in release builds (this assembly compiles only in Editor and Development builds).
    /// </summary>
    public sealed class DebugToolsRoot : MonoBehaviour
    {
        private static DebugToolsRoot _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Spawn()
        {
            if (_instance != null) return;
            var go = new GameObject("[DebugTools]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<DebugToolsRoot>();
            go.AddComponent<DebugContext>();
            go.AddComponent<PlaytestLogger>();
            go.AddComponent<DebugCheats>();
            go.AddComponent<BugReporter>();
            go.AddComponent<DebugOverlay>();
        }
    }
}
