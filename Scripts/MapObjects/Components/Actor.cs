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
        /// The current energy level of this actor. When energy reaches the threshold,
        /// the actor can take a turn.
        /// </summary>
        public int Energy { get; set; }

        /// <summary>
        /// How much energy this actor gains per game loop iteration.
        /// Higher speed means more frequent turns.
        /// </summary>
        public int Speed { get; set; } = 100;

        /// <summary>
        /// The energy threshold required to take a turn.
        /// </summary>
        public const int EnergyThreshold = 100;

        /// <summary>
        /// The next action queued for this actor (primarily used for player input).
        /// </summary>
        private IAction? _nextAction;

        public Actor() 
            : base(false, false, false, false)
        {
            Energy = 0;
        }

        public Actor(int speed) 
            : base(false, false, false, false)
        {
            Speed = speed;
            Energy = 0;
        }

        /// <summary>
        /// Checks if this actor has enough energy to take a turn.
        /// </summary>
        public bool CanTakeTurn() => Energy >= EnergyThreshold;

        /// <summary>
        /// Grants energy to this actor based on their speed.
        /// </summary>
        public void GainEnergy() => Energy += Speed;

        /// <summary>
        /// Consumes energy after performing an action.
        /// </summary>
        public void ConsumeEnergy(int amount = EnergyThreshold) => Energy -= amount;

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
            var action = _nextAction;
            _nextAction = null; // Only perform once
            return action;
        }
    }
}