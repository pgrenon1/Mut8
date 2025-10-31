using Mut8.Scripts.Actions;
using SadRogue.Integration;
using SadRogue.Integration.Components;

namespace Mut8.Scripts.MapObjects.Components
{
    /// <summary>
    /// Component that enables an entity to participate in the turn-based action system.
    /// Entities with this component can generate and perform actions during gameplay.
    /// </summary>
    internal class Actor : RogueLikeComponentBase<RogueLikeEntity>
    {
        /// <summary>
        /// The current time value of this actor in the turn queue.
        /// Lower values mean the actor acts sooner.
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// The base amount of time units this actor gets per turn.
        /// This can be modified by stats or status effects.
        /// </summary>
        public int BaseTimeUnitsPerTurn { get; set; } = 100;

        /// <summary>
        /// The remaining time units available for this actor's current turn.
        /// Actions consume from this pool.
        /// </summary>
        public int RemainingTimeUnits { get; set; }

        /// <summary>
        /// The next action queued for this actor (primarily used for player input).
        /// </summary>
        private IAction? _nextAction;

        public Actor() 
          : base(false, false, false, false)
        {
            Time = 0;
            RemainingTimeUnits = 0;
        }

        public Actor(int baseTimeUnitsPerTurn) 
        : base(false, false, false, false)
        {
            BaseTimeUnitsPerTurn = baseTimeUnitsPerTurn;
            Time = 0;
            RemainingTimeUnits = 0;
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
        /// Gets the time units available for this turn, potentially modified by stats or status effects.
        /// </summary>
        public int GetTimeUnitsPerTurn()
        {
            // TODO: Apply modifiers from stats, status effects, etc.
            return BaseTimeUnitsPerTurn;
        }

        /// <summary>
        /// Starts a new turn for this actor, granting them their time unit pool.
        /// </summary>
        public void StartTurn()
        {
            RemainingTimeUnits = GetTimeUnitsPerTurn();
        }

        /// <summary>
        /// Consumes time units after performing an action.
        /// </summary>
        public void ConsumeTime(int amount)
        {
            RemainingTimeUnits -= amount;
        }

        /// <summary>
        /// Checks if this actor's turn is over (no more time units available).
        /// </summary>
        public bool IsTurnOver() => RemainingTimeUnits <= 0;

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
                _nextAction = null;
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
    }
}