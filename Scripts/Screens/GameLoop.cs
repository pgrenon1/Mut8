using Mut8.Scripts.MapObjects;
using Mut8.Scripts.MapObjects.Components;

namespace Mut8.Scripts.Screens
{
    /// <summary>
    /// Manages the turn-based game loop, processing actors in sequence based on their time values.
    /// Uses a time-based turn system where actors perform actions that consume time units.
    /// </summary>
    internal class GameLoop
    {
        private readonly Queue<Actor> _actorsQueue = new();
        private Actor? _currentActor;
        private int _turnNumber;

        public int TurnNumber => _turnNumber;

        /// <summary>
        /// Adds an actor to the game loop.
        /// </summary>
        public void AddActor(Actor actor)
        {
            _actorsQueue.Enqueue(actor);
        }

        /// <summary>
        /// Removes an actor from the game loop.
        /// </summary>
        public void RemoveActor(Actor actor)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the game loop, executing actor turns based on the time-based system.
        /// </summary>
        public void Process()
        {
            if (_actorsQueue.Count == 0 && _currentActor == null)
                return;

            // Keep processing actors until we hit a player waiting for input
            // This allows AI actors to take their turns automatically
            const int maxIterations = 100; // Prevent infinite loops
            int iterations = 0;

            while (iterations < maxIterations)
            {
                iterations++;

                // If no current actor, get the next one from the queue
                if (_currentActor == null)
                {
                    _currentActor = GetNextActor();
                    if (_currentActor == null)
                        return;

                    _currentActor.StartTurn();
                }

                // Try to process the current actor's turn
                bool turnProcessed = ProcessCurrentActorTurn();

                // If turn wasn't processed (waiting for player input), break the loop
                if (!turnProcessed)
                    break;
            }
        }

        /// <summary>
        /// Gets the next actor to act based on their time value.
        /// Returns the actor with the lowest time value.
        /// </summary>
        private Actor? GetNextActor()
        {
            if (_actorsQueue.Count == 0)
                return null;

            return _actorsQueue.Dequeue();
        }

        /// <summary>
        /// Processes the current actor's turn, allowing them to perform actions until their time runs out.
        /// Returns true if a turn was processed, false if waiting for player input.
        /// </summary>
        private bool ProcessCurrentActorTurn()
        {
            if (_currentActor == null)
                return false;

            // Get the action the actor wants to perform
            var action = _currentActor.GetAction();

            // If no action available (e.g., waiting for player input or AI has no valid action)
            if (action == null)
            {
                // Check if this is the player (has CustomKeybindingsComponent) - if so, wait for input
                var isPlayer = _currentActor.Parent?.AllComponents.GetFirstOrDefault<CustomKeybindingsComponent>() != null;

                if (isPlayer)
                {
                    // Player is waiting for input, return false to stop processing
                    return false;
                }
                else
                {
                    // AI has no action, skip this actor by ending their turn
                    _currentActor.ConsumeTime(_currentActor.RemainingTimeUnits);
                }
            }
            else
            {
                if (_currentActor.Parent is Player)
                {
                    _turnNumber++;
                }

                // Perform the action
                var result = action.Perform();

                // Handle alternate actions (e.g., bumping into an enemy becomes an attack)
                while (result.Alternate != null)
                {
                    // Consume time for the original action if it had a cost
                    if (result.TimeCost > 0)
                    {
                        _currentActor.ConsumeTime(result.TimeCost);
                    }

                    // Perform the alternate action
                    action = result.Alternate;
                    result = action.Perform();
                }

                // Consume time for the performed action
                if (result.Succeeded)
                {
                    _currentActor.ConsumeTime(result.TimeCost);
                }
            }

            // Check if the actor's turn is over
            if (_currentActor.IsTurnOver())
            {
                // Add the remaining time (which may be negative) to the actor's time value
                _currentActor.Time += Math.Abs(_currentActor.RemainingTimeUnits);

                // Add the actor to the end of the queue
                _actorsQueue.Enqueue(_currentActor);

                // Clear current actor so we get the next one from the queue
                _currentActor = null;
            }

            return true; // Turn was processed, continue loop
        }
    }
}