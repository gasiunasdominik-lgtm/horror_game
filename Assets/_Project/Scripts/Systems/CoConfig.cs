using UnityEngine;

namespace Vchod.Systems
{
    [CreateAssetMenu(menuName = "Vchod/CO Config", fileName = "CoConfig")]
    public sealed class CoConfig : ScriptableObject
    {
        public CoSettings model = new();
        public SymptomSettings symptoms = new();
        public DetectorSettings detector = new();
    }
}
