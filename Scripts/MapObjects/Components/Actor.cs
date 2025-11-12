using Mut8.Scripts.Actions;
using Mut8.Scripts.Core;
using SadRogue.Integration;
using SadRogue.Integration.Components;

namespace Mut8.Scripts.MapObjects.Components;

/// <summary>
/// Component that enables an entity to participate in the turn-based action system.
/// Entities with this component can generate and perform actions during gameplay.
/// Actions add time costs directly to the actor's Time value, and actors are processed
/// in order by their Time value in a priority queue.
/// </summary>
internal class Actor : RogueLikeComponentBase<RogueLikeEntity>
{
    /// <summary>
    /// The current time value of this actor in the turn queue.
    /// Lower values mean the actor acts sooner.
    /// Actions add their time cost to this value.
    /// </summary>
    public int Time { get; set; }

    /// <summary>
    /// The next action queued for this actor (primarily used for player input).
    /// </summary>
    private IAction? _nextAction;

    public Actor()
        : base(false, false, false, false)
    {
        Time = 0;
    }

    public Actor(int initialTime)
        : base(false, false, false, false)
    {
        Time = initialTime;
    }

    public override void OnAdded(IScreenObject host)
    {
        base.OnAdded(host);

        Engine.MainGame!.GameLoop.AddActor(this);
    }

    public override void OnRemoved(IScreenObject host)
    {
        base.OnRemoved(host);

        Engine.MainGame!.GameLoop.RemoveActor(this);
    }

    /// <summary>
    /// Sets the next action to be performed by this actor (used for player input).
    /// </summary>
    public void SetNextAction(IAction action)
    {
        _nextAction = action;
    }

    /// <summary>
    /// Gets the action this actor wants to perform.
    /// For AI-controlled actors, this should be overridden or handled via a separate AI component.
    /// For player-controlled actors, this returns the queued action from input.
    /// </summary>
    public virtual IAction? GetAction()
    {
        // First check if there's a queued action (from player input)
        if (_nextAction != null)
        {
            var action = _nextAction;
            return action;
        }

        // Otherwise, check for AI component
        var aiComponent = Parent?.AllComponents.GetFirstOrDefault<DemoEnemyAI>();
        if (aiComponent != null)
        {
            return aiComponent.GenerateAction();
        }

        // No action available
        return null;
    }
        
    public void ClearNextAction()
    {
        _nextAction = null;
    }

    public float GetActionCostMultiplier()
    {
        // Modify the cost based on the Quick gene
        Genome genome = Parent.AllComponents.GetFirstOrDefault<Genome>();
        float quickGeneValue = genome.GetGeneNormalized(Gene.Quick);
        float speedMultiplier = 1f + (GameData.QuickGeneSpeedMultiplier - 1f) * quickGeneValue;
        return 1f / speedMultiplier;
    }
}