using UnityEngine;

namespace Vchod.Player
{
    /// <summary>Anything the player can use with the interact button (light button, later doors, windows, hiding spots).</summary>
    public interface IInteractable
    {
        string Prompt { get; }
        void Interact(GameObject interactor);
    }
}
