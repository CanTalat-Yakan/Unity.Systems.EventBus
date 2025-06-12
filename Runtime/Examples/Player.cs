using UnityEngine;
using UnityEssentials;

namespace Examples.EventBus
{
    public class PlayerEvent : IEvent
    {
        public int Health;
        public int Mana;
    }

    public class Player : MonoBehaviour
    {
        private EventBinding<PlayerEvent> _binding;

        public void OnEnable()
        {
            _binding = new EventBinding<PlayerEvent>(HandleEventWithArgs);
            EventBus<PlayerEvent>.Register(_binding);
        }

        public void OnDisable() =>
            EventBus<PlayerEvent>.Deregister(_binding);

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                EventBus<PlayerEvent>.Raise(new PlayerEvent { Health = 100 });
        }

        private void HandleEventWithoutArgs() =>
           Debug.Log("Player event received without args.");

        private void HandleEventWithArgs(PlayerEvent playerEvent)
        {
            var health = playerEvent.Health;
            var mana = playerEvent.Mana;

            Debug.Log($"Player event received with args: Health = {health}, Mana = {mana}");
        }
    }
}
