using System;
using UnityEngine.InputSystem;

namespace Vchod.Player
{
    /// <summary>
    /// Gameplay input defined in code: keyboard/mouse plus gamepad, no asset to keep in sync.
    /// </summary>
    public sealed class PlayerInputActions : IDisposable
    {
        public readonly InputAction Move = new("Move", InputActionType.Value);
        public readonly InputAction LookMouse = new("LookMouse", InputActionType.Value, "<Mouse>/delta");
        public readonly InputAction LookStick = new("LookStick", InputActionType.Value, "<Gamepad>/rightStick");
        public readonly InputAction Sprint = new("Sprint", InputActionType.Button);
        public readonly InputAction Interact = new("Interact", InputActionType.Button);

        public PlayerInputActions()
        {
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            Move.AddBinding("<Gamepad>/leftStick");

            Sprint.AddBinding("<Keyboard>/leftShift");
            Sprint.AddBinding("<Gamepad>/leftStickPress");

            Interact.AddBinding("<Keyboard>/e");
            Interact.AddBinding("<Gamepad>/buttonSouth");
        }

        public void Enable()
        {
            Move.Enable();
            LookMouse.Enable();
            LookStick.Enable();
            Sprint.Enable();
            Interact.Enable();
        }

        public void Disable()
        {
            Move.Disable();
            LookMouse.Disable();
            LookStick.Disable();
            Sprint.Disable();
            Interact.Disable();
        }

        public void Dispose()
        {
            Move.Dispose();
            LookMouse.Dispose();
            LookStick.Dispose();
            Sprint.Dispose();
            Interact.Dispose();
        }
    }
}
