using UnityEngine;
using Vchod.Core;
using Vchod.Enemy;
using Vchod.Player;
using Vchod.World;

namespace Vchod.Systems
{
    /// <summary>Turns collapse, catch and entering a flat into exactly one ending per round.</summary>
    public sealed class EndingsManager : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private CoSystem co;

        private CollapseCause _cause = CollapseCause.Corridor;

        public EndingId Ending { get; private set; }

        private void OnEnable()
        {
            EventBus.Subscribe<CoCollapsed>(OnCollapsed);
            EventBus.Subscribe<PlayerCaught>(OnCaught);
            EventBus.Subscribe<EnteredFlat>(OnEnteredFlat);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CoCollapsed>(OnCollapsed);
            EventBus.Unsubscribe<PlayerCaught>(OnCaught);
            EventBus.Unsubscribe<EnteredFlat>(OnEnteredFlat);
        }

        private void OnEnteredFlat(EnteredFlat e)
        {
            if (Ending != EndingId.None) return;
            _cause = e.Home ? CollapseCause.Home : CollapseCause.Neighbour;
            co.ForceCollapse(); // the flats by the shaft: the air inside is lethal
        }

        private void OnCollapsed(CoCollapsed e) => End(EndingRules.ForCollapse());

        private void OnCaught(PlayerCaught e) => End(EndingRules.ForCatch(VentWindow.FreshAirAt(player.transform.position)));

        private void End(EndingId ending)
        {
            if (Ending != EndingId.None) return;
            Ending = ending;
            player.SetInputLock(this, true);
            EventBus.Publish(new RoundEnded { Ending = ending, Cause = _cause });
        }
    }
}
