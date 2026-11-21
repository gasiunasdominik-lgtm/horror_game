using System;
using UnityEngine;

namespace Vchod.DebugTools
{
    /// <summary>Everything needed to reproduce a moment of play. Serialized to state.json.</summary>
    [Serializable]
    public sealed class GameSnapshot
    {
        public string timestamp;
        public string buildVersion;
        public string unityVersion;
        public string gitHash;
        public bool isEditor;
        public bool isDevelopmentBuild;
        public string scene;
        public int seed;
        public string layoutHash = "n/a";
        public int reshuffleIndex = -1;
        public int floor = -1;
        public Vector3 playerPosition;
        public float playerYaw;
        public float coLevel = -1f;
        public int coStage = -1;
        public string enemyState = "n/a";
        public string activeEnding = "none";
        public bool lightOn;
        public float roundTime;
        public float fps;
        public float timeScale;
        public bool godMode;
    }
}
