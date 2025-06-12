using System.Collections.Generic;
using UnityEngine;

namespace UnityEssentials
{
    public static class EventBus<T> where T : IEvent
    {
        private static readonly HashSet<IEventBinding<T>> s_bindings = new();

        public static void Register(EventBinding<T> binding) => s_bindings.Add(binding);
        public static void Deregister(EventBinding<T> binding) => s_bindings.Remove(binding);

        public static void Raise(T @event)
        {
            var snapshot = new HashSet<IEventBinding<T>>(s_bindings);

            foreach (var binding in snapshot)
                if (s_bindings.Contains(binding))
                {
                    binding.OnEvent.Invoke(@event);
                    binding.OnEventNoArgs.Invoke();
                }
        }

        private static void Clear()
        {
            Debug.Log($"Clearing {typeof(T).Name} bindings");
            s_bindings.Clear();
        }
    }
}
