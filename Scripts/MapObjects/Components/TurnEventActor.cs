using Mut8.Scripts.Actions;

namespace Mut8.Scripts.MapObjects.Components;

/// <summary>
/// A special actor that represents turn events in the priority queue.
/// This actor executes at regular intervals (e.g., every 100 time units)
/// and can be used to trigger periodic effects like regeneration, status effects, etc.
/// </summary>
internal class TurnEventActor : Actor
{
    /// <summary>
    /// Event fired when a turn executes. Other systems can subscribe to this event.
    /// </summary>
    public event Action? OnTurn;

    /// <summary>
    /// The interval at which this turn event should execute (in time units).
    /// </summary>
    public int TurnInterval { get; set; }

    public TurnEventActor(int turnInterval = 100) : base(turnInterval)
    {
        TurnInterval = turnInterval;
    }

    /// <summary>
    /// Gets the action for the turn event.
    /// The turn event always performs a wait action equal to its interval.
    /// </summary>
    public override IAction? GetAction()
    {
        // Return a special turn event action
        return new TurnEventAction(this);
    }

    public void InvokeOnTurnCallbacks()
    {
        OnTurn?.Invoke();
    }
}
