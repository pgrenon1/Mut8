using Mut8.Scripts.MapObjects.Components;

namespace Mut8.Scripts.Actions;

/// <summary>
/// Special action for turn events that advances the turn counter.
/// </summary>
internal class TurnEventAction : IAction
{
    private readonly TurnEventActor _eventActor;

    public TurnEventAction(TurnEventActor eventActor)
    {
        _eventActor = eventActor;
    }

    public ActionResult Perform()
    {
        // This is where we would handle "once per turn" updates
        // For example:
        // - Regeneration for all actors
        // - Status effect ticks
        // - Environmental effects
        // - etc.

        _eventActor.InvokeOnTurnCallbacks();

        // For now, just return success with the turn interval as the cost
        // This ensures the turn event repeats at the correct interval
        return ActionResult.SuccessWithTime(_eventActor.TurnInterval);
    }
}