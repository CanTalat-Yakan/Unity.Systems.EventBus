using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    /// <summary>
    /// Utility class for initializing and managing event buses in Unity projects.
    /// Handles both runtime and editor-time behaviors.
    /// </summary>
    public static class EventBusUtility
    {
        public static IReadOnlyList<Type> EventTypes { get; private set; }
        public static IReadOnlyList<Type> EventBusTypes { get; private set; }

#if UNITY_EDITOR
        public static PlayModeStateChange PlayModeState { get; private set; }

        /// <summary>
        /// Registers a callback to handle play mode state changes when the editor loads.
        /// </summary>
        [InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /// <summary>
        /// Called when the play mode state changes in the Unity Editor.
        /// Clears all event buses when exiting play mode.
        /// </summary>
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            PlayModeState = state;
            if (state == PlayModeStateChange.ExitingPlayMode)
                ClearAllBuses();
        }
#endif

        /// <summary>
        /// Called automatically before the first scene load at runtime.
        /// Initializes all event types and corresponding event buses.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            EventTypes = PredefinedAssemblyUtility.GetTypes(typeof(IEvent));
            EventBusTypes = InitializeAllBuses();

        }

        /// <summary>
        /// Creates a list of EventBus types, one for each event type.
        /// </summary>
        private static IReadOnlyList<Type> InitializeAllBuses()
        {
            List<Type> eventBusTypes = new();

            var typedef = typeof(EventBus<>);
            foreach (var eventType in EventTypes)
            {
                var busType = typedef.MakeGenericType(eventType);
                eventBusTypes.Add(busType);
                // Debug.Log($"Initialized EventBus<{eventType.Name}>");
            }

            return eventBusTypes;
        }

        /// <summary>
        /// Clears all event buses by invoking their private static Clear method via reflection.
        /// Used when exiting play mode or resetting systems.
        /// </summary>
        public static void ClearAllBuses()
        {
            if(EventBusTypes == null || EventBusTypes.Count == 0)
                return;

            // Debug.Log("Clearing all buses...");
            for (int i = 0; i < EventTypes.Count; i++)
            {
                var busType = EventBusTypes[i];
                var bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;
                var clearMethod = busType.GetMethod("Clear", bindingFlags);
                clearMethod.Invoke(null, null);
            }
        }
    }
}
