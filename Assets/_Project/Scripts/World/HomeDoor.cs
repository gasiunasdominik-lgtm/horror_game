using UnityEngine;
using Vchod.Core;
using Vchod.Player;

namespace Vchod.World
{
    /// <summary>
    /// The player's own door: the goal of the round. Their flat is next to the shaft with the gas
    /// (GDD ending 1), so going in is the bad ending; the endings manager decides what happens.
    /// </summary>
    public sealed class HomeDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "E  Odemknout";

        public string Prompt => prompt;

        public void Interact(GameObject interactor) => EventBus.Publish(new EnteredFlat { Home = true });
    }
}
