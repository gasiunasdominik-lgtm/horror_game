using System.Collections.Generic;
using UnityEngine;
using Vchod.Core;
using Vchod.Player;

namespace Vchod.World
{
    /// <summary>
    /// A wardrobe or table the player can hide in. A wardrobe hides completely, but its door
    /// creaks (noise the firefighter hears). A table is silent, but a torch close by finds you.
    /// </summary>
    public sealed class HidingSpot : MonoBehaviour, IInteractable
    {
        private static readonly List<HidingSpot> Active = new();

        [SerializeField] private HidingKind kind = HidingKind.Wardrobe;
        [SerializeField] private Transform viewPoint;
        [SerializeField] private Transform exitPoint;
        [Tooltip("Where the firefighter stands to open or look under it.")]
        [SerializeField] private Transform checkPoint;
        [SerializeField] private float enterNoiseRadius = 6f;
        [SerializeField] private float yawLimit = 50f;
        [SerializeField] private string enterPrompt = "E  Schovat se";
        [SerializeField] private string exitPrompt = "E  Vylézt";

        private PlayerController _occupant;

        public static IReadOnlyList<HidingSpot> All => Active;
        public HidingKind Kind => kind;
        public Vector3 CheckPoint => checkPoint.position;
        public bool IsOccupied => _occupant != null && _occupant.HiddenIn == (IInteractable)this;
        public string Prompt => IsOccupied ? exitPrompt : enterPrompt;

        private void OnEnable() => Active.Add(this);

        private void OnDisable()
        {
            Active.Remove(this);
            if (IsOccupied) Leave(silent: true);
        }

        public void Interact(GameObject interactor)
        {
            if (IsOccupied)
            {
                Leave(silent: false);
                return;
            }
            var player = interactor.GetComponentInParent<PlayerController>();
            if (player == null || player.IsHidden) return;
            _occupant = player;
            player.EnterHiding(this, viewPoint.position, viewPoint.eulerAngles.y, yawLimit);
            MakeNoise();
        }

        /// <summary>The firefighter opened it: the player is pulled out in front of it.</summary>
        public void Expose()
        {
            if (IsOccupied) Leave(silent: true);
        }

        private void Leave(bool silent)
        {
            var player = _occupant;
            _occupant = null;
            player.ExitHiding(exitPoint.position, exitPoint.eulerAngles.y);
            if (!silent) MakeNoise();
        }

        private void MakeNoise()
        {
            if (enterNoiseRadius > 0f)
                EventBus.Publish(new NoiseMade { Position = transform.position, Radius = enterNoiseRadius });
        }
    }
}
