using UnityEngine;
using Vchod.Player;

namespace Vchod.World
{
    /// <summary>
    /// Fire door to the emergency stairs at the far end of each floor: the second way between
    /// floors. Look up to climb, look down to descend (only one way on the top and ground floors).
    /// </summary>
    public sealed class FireDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private float lookThreshold = 12f;

        private ModuleLayoutBuilder _house;
        private PlayerController _player;

        public void Setup(ModuleLayoutBuilder house, PlayerController player)
        {
            _house = house;
            _player = player;
        }

        private bool CanUp => _house != null && !_house.IsTopFloor;
        private bool CanDown => _house != null && _house.Floor > 0;

        /// <summary>Up unless looking down (or already at the top).</summary>
        private bool GoesUp
        {
            get
            {
                if (!CanDown) return true;
                if (!CanUp) return false;
                return _player == null || _player.Pitch <= lookThreshold;
            }
        }

        public string Prompt
        {
            get
            {
                if (_house == null) return null;
                var dir = GoesUp ? "nahoru" : "dolů";
                return CanUp && CanDown
                    ? $"E  Nouzové schodiště {dir}   (pohled dolů = dolů)"
                    : $"E  Nouzové schodiště {dir}";
            }
        }

        public void Interact(GameObject interactor)
        {
            if (_house != null) _house.TakeFireStairs(GoesUp);
        }
    }
}
