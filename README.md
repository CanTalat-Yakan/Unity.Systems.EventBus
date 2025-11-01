# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# Event Bus

> Quick overview: Strongly‑typed, lightweight event bus for decoupled messaging between MonoBehaviours. Events implement `IEvent`; listeners register `EventBinding<T>` and receive callbacks when `EventBus<T>.Raise` is called.

A minimal, generic publish/subscribe utility intended for scene‑local messaging. Event types are plain interfaces implementing `IEvent`. Listeners create an `EventBinding<T>` and register it with the corresponding `EventBus<T>`. When an event is raised, registered bindings are invoked in a safe snapshot so modifications during dispatch do not break iteration. In the editor, all buses are cleared on exiting Play Mode.

![screenshot](Documentation/Screenshot.png)

## Features
- Strongly‑typed events
  - Define any number of event types by implementing `IEvent`
  - Generic static bus per event type: `EventBus<T>`
- Simple bindings
  - `EventBinding<T>` supports both `Action<T>` (with payload) and `Action` (no‑args) handlers, plus additive `Add/Remove`
- Safe dispatch
  - Snapshot taken on `Raise` prevents collection modification issues during callbacks
- Editor integration
  - All buses cleared automatically on Exiting Play Mode (via `EventBusUtilities`)
- Minimal surface
  - `Register`, `Deregister`, and `Raise` only; no external dependencies

## Requirements
- Unity 6000.0+
- Add your event types and listener scripts to a scene
- Uses `EventBusUtilities` to initialize and clear buses; it relies on type discovery for `IEvent` implementors

## Usage
1) Define an event type
```csharp
public struct PlayerDamaged : IEvent
{
    public int Amount;
    public Vector3 HitPoint;
}
```

2) Listen for an event
```csharp
public class DamageListener : MonoBehaviour
{
    private EventBinding<PlayerDamaged> _binding;

    private void OnEnable()
    {
        _binding = new EventBinding<PlayerDamaged>(OnPlayerDamaged);
        EventBus<PlayerDamaged>.Register(_binding);

        // Optionally also react without payload
        _binding.Add(() => Debug.Log("Damage received"));
    }

    private void OnDisable()
    {
        EventBus<PlayerDamaged>.Deregister(_binding);
        _binding = null;
    }

    private void OnPlayerDamaged(PlayerDamaged evt)
    {
        Debug.Log($"Damage: {evt.Amount} at {evt.HitPoint}");
    }
}
```

3) Raise an event
```csharp
// From anywhere (main thread)
EventBus<PlayerDamaged>.Raise(new PlayerDamaged { Amount = 10, HitPoint = transform.position });
```

Additional notes
- Multiple handlers can be added to the same binding via `Add(Action<T>)` and `Add(Action)`; remove with `Remove(...)`
- Always deregister in `OnDisable` (or when no longer needed) to avoid stale listeners

## How It Works
- Event storage
  - Each `EventBus<T>` maintains a `HashSet<IEventBinding<T>>` of current bindings
- Dispatch
  - On `Raise`, a snapshot of current bindings is taken; each binding is checked for continued membership and invoked (`OnEvent(@event)` then `OnEventNoArgs()`)
- Lifecycle helpers
  - `EventBusUtilities` discovers `IEvent` types at startup and prepares their bus types; on Exiting Play Mode, it reflects a private static `Clear` on each bus to release bindings

## Notes and Limitations
- Threading: The bus is not thread‑safe; publish/subscribe on the main thread
- Lifetime: Bindings are static‑lifetime per app domain; deregister to prevent leaks
- Exceptions: A listener throwing will propagate unless you guard inside your handler
- Ordering: `HashSet` is unordered; handler invocation order is unspecified
- Scope: This is a simple in‑process bus; no persistence, buffering, or cross‑scene routing is provided

## Files in This Package
- `Runtime/EventBus.cs` – Generic bus with Register/Deregister/Raise and internal Clear
- `Runtime/EventBinding.cs` – Binding with payload and no‑args actions, add/remove helpers; defines `IEvent`
- `Runtime/EventBusUtilities.cs` – Type discovery, PlayerLoop/editor hooks, bus initialization, and global clear
- `Runtime/Examples/` – Simple usage examples
- `Runtime/UnityEssentials.EventBus.asmdef` – Runtime assembly definition

## Tags
unity, event-bus, pubsub, messaging, decoupled, events, runtime
