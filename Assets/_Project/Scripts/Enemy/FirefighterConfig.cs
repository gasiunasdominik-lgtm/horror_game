using UnityEngine;

namespace Vchod.Enemy
{
    [CreateAssetMenu(menuName = "Vchod/Firefighter Config", fileName = "FirefighterConfig")]
    public sealed class FirefighterConfig : ScriptableObject
    {
        public FirefighterSettings settings = new();
    }
}
