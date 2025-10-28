using Mut8.Scripts.Actions;
using Mut8.Scripts.MapObjects.Components;
using Mut8.Scripts.Maps;
using SadRogue.Integration;

namespace Mut8.Scripts
{
    /// <summary>
    /// Manages the turn-based game loop, processing actors in sequence based on their energy levels.
    /// </summary>
    internal class GameLoop
    {
        private readonly GameMap _map;
        private int _currentActorIndex = 0;

        public GameLoop(GameMap map)
        {
            _map = map;
        }

        /// <summary>
        /// Processes one step of the game loop.
        /// Returns true if a turn was taken, false if waiting for player input.
        /// </summary>
        public bool Process()
        {
            var actors = GetActors();
            if (actors.Count == 0) return false;

            // Process actors in a round-robin fashion
            int actorsProcessed = 0;
            while (actorsProcessed < actors.Count)
            {
                // Ensure index is valid (actors may have been removed)
                if (_currentActorIndex >= actors.Count)
                    _currentActorIndex = 0;

                var entity = actors[_currentActorIndex];
                var actorComponent = entity.AllComponents.GetFirstOrDefault<Actor>();

                if (actorComponent == null)
                {
                    // Move to next actor if this one lost its Actor component
                    _currentActorIndex = (_currentActorIndex + 1) % actors.Count;
                    actorsProcessed++;
                    continue;
                }

                // Grant energy to the actor
                actorComponent.GainEnergy();

                // Check if actor has enough energy to take a turn
                if (actorComponent.CanTakeTurn())
                {
                    var action = actorComponent.GetAction();

                    // If no action available (e.g., waiting for player input), bail out
                    if (action == null)
                    {
                        return false;
                    }

                    // Process the action (with alternate action support)
                    if (ProcessAction(action, actorComponent))
                    {
                        // Action succeeded, consume energy and move to next actor
                        actorComponent.ConsumeEnergy();
                        _currentActorIndex = (_currentActorIndex + 1) % actors.Count;
                        return true;
                    }
                    else
                    {
                        // Action failed, don't consume energy, don't advance turn
                        return false;
                    }
                }

                // Move to next actor
                _currentActorIndex = (_currentActorIndex + 1) % actors.Count;
                actorsProcessed++;
            }

            // No actor was ready to take a turn
            return false;
        }

        /// <summary>
        /// Processes an action, handling alternate actions.
        /// Returns true if the action succeeded, false otherwise.
        /// </summary>
        private bool ProcessAction(IAction action, Actor actorComponent)
        {
            while (true)
            {
                var result = action.Perform();

                if (!result.Succeeded)
                {
                    // Action failed
                    return false;
                }

                if (result.Alternate == null)
                {
                    // Action succeeded with no alternate
                    return true;
                }

                // Process alternate action
                action = result.Alternate;
            }
        }

        /// <summary>
        /// Gets all entities with an Actor component from the map.
        /// </summary>
        private List<RogueLikeEntity> GetActors()
        {
            var actors = new List<RogueLikeEntity>();

            foreach (var entity in _map.Entities.Items)
            {
                if (entity is RogueLikeEntity rogueLikeEntity &&
                    rogueLikeEntity.AllComponents.Contains<Actor>())
                {
                    actors.Add(rogueLikeEntity);
                }
            }

            return actors;
        }
    }
}