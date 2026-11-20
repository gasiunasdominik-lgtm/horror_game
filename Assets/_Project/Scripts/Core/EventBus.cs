using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vchod.Core
{
    /// <summary>
    /// Minimal typed event bus so systems (and debug tools) can react to each other
    /// without direct references. Subscribers must unsubscribe in OnDisable.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            Handlers.TryGetValue(typeof(T), out var existing);
            Handlers[typeof(T)] = Delegate.Combine(existing, handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (!Handlers.TryGetValue(typeof(T), out var existing)) return;
            var remaining = Delegate.Remove(existing, handler);
            if (remaining == null) Handlers.Remove(typeof(T));
            else Handlers[typeof(T)] = remaining;
        }

        public static void Publish<T>(T evt) where T : struct
        {
            if (Handlers.TryGetValue(typeof(T), out var handler))
                ((Action<T>)handler)?.Invoke(evt);
        }

        public static void Clear() => Handlers.Clear();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();
    }
}
