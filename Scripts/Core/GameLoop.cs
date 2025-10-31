using Mut8.Scripts.MapObjects;
using Mut8.Scripts.MapObjects.Components;

namespace Mut8.Scripts.Core
{
    /// <summary>
    /// Manages the turn-based game loop using a priority queue system.
    /// Actors are processed in order of their Time value (lowest first).
    /// Action costs are added to each actor's Time value.
    /// 
    /// Example usage with turn events:
    /// <code>
    /// // Add regular actors
    /// gameLoop.AddActor(playerActor);     // Time = 0
    /// gameLoop.AddActor(enemyActor);      // Time = 0
    /// 
    /// // Add a turn event that fires every 100 time units
    /// var turnEvent = new TurnEventActor(100);
    /// gameLoop.AddActor(turnEvent);       // Time = 100
    /// 
    /// // Processing example:
    /// // 1. Player acts (Time: 0), costs 120 time → Player Time becomes 120
    /// // 2. Enemy acts (Time: 0), costs 50 time → Enemy Time becomes 50
    /// // 3. Enemy acts again (Time: 50), costs 100 time → Enemy Time becomes 150
    /// // 4. Turn event fires (Time: 100), costs 100 time → Turn Time becomes 200
    /// // 5. Player acts (Time: 120), costs 80 time → Player Time becomes 200
    /// // And so on...
    /// </code>
    /// </summary>
    internal class GameLoop
    {
        private readonly PriorityQueue<Actor, int> _actorQueue = new();
        private int _turnNumber;

        public int TurnNumber => _turnNumber;

        public GameLoop()
        {
            AddActor(new TurnEventActor(100));
            _turnNumber = 0;
        }

        /// <summary>
        /// Adds an actor to the game loop priority queue.
        /// </summary>
        public void AddActor(Actor actor)
        {
            _actorQueue.Enqueue(actor, actor.Time);
        }

        /// <summary>
        /// Removes an actor from the game loop.
        /// </summary>
        public void RemoveActor(Actor actor)
        {
            // Note: PriorityQueue doesn't have a direct Remove method
            // We'll need to rebuild the queue without this actor
            var tempList = new List<(Actor actor, int time)>();

            while (_actorQueue.Count > 0)
            {
                var currentActor = _actorQueue.Dequeue();
                if (currentActor != actor)
                {
                    tempList.Add((currentActor, currentActor.Time));
                }
            }

            foreach (var (a, time) in tempList)
            {
                _actorQueue.Enqueue(a, time);
            }
        }

        /// <summary>
        /// Processes the game loop, executing actor turns based on the priority queue.
        /// Actors with the lowest Time value act first.
        /// </summary>
        public void Process()
        {
            if (_actorQueue.Count == 0)
                return;

            // Keep processing actors until we hit a player waiting for input
            // This allows AI actors to take their turns automatically
            const int maxIterations = 100; // Prevent infinite loops
            int iterations = 0;

            while (iterations < maxIterations && _actorQueue.Count > 0)
            {
                iterations++;

                // Get the next actor (the one with the lowest time value)
                var currentActor = _actorQueue.Dequeue();

                // Try to process the current actor's turn
                bool turnProcessed = ProcessActorTurn(currentActor);

                // If turn wasn't processed (waiting for player input), 
                // put the actor back in the queue and break
                if (!turnProcessed)
                {
                    _actorQueue.Enqueue(currentActor, currentActor.Time);
                    break;
                }

                // Actor has taken their action, put them back in the queue with updated time
                _actorQueue.Enqueue(currentActor, currentActor.Time);
            }
        }

        /// <summary>
        /// Processes a single actor's turn.
        /// Returns true if an action was processed, false if waiting for player input.
        /// </summary>
        private bool ProcessActorTurn(Actor actor)
        {
            // Get the action the actor wants to perform
            var action = actor.GetAction();

            // If no action available
            if (action == null)
            {
                // Check if this is the player - if so, wait for input
                var isPlayer = actor.Parent?.AllComponents.GetFirstOrDefault<CustomKeybindingsComponent>() != null;

                if (isPlayer)
                {
                    // Player is waiting for input, return false to stop processing
                    return false;
                }
                else
                {
                    // AI has no action, skip by adding minimal time
                    actor.Time += 100; // Default action cost
                }
            }
            else
            {
                if (actor.Parent is Player)
                {
                    _turnNumber++;
                }

                // Perform the action
                var result = action.Perform();

                // Handle alternate actions (e.g., bumping into an enemy becomes an attack)
                while (result.Alternate != null)
                {
                    // Add time for the original action if it had a cost
                    if (result.TimeCost > 0)
                    {
                        actor.Time += result.TimeCost;
                    }

                    // Perform the alternate action
                    action = result.Alternate;
                    result = action.Perform();
                }

                // Add time cost for the performed action
                if (result.Succeeded)
                {
                    actor.Time += result.TimeCost;
                }
            }

            return true; // Turn was processed, continue loop
        }

        /// <summary>
        /// Normalizes all actor times by subtracting the minimum time value.
        /// This prevents overflow issues during very long games.
        /// Should be called periodically when time values get large.
        /// </summary>
        public void NormalizeActorTimes()
        {
            if (_actorQueue.Count == 0)
                return;

            // Find the minimum time value
            var tempList = new List<Actor>();
            int minTime = int.MaxValue;

            while (_actorQueue.Count > 0)
            {
                var actor = _actorQueue.Dequeue();
                tempList.Add(actor);
                if (actor.Time < minTime)
                    minTime = actor.Time;
            }

            // Subtract minimum time from all actors and re-enqueue
            foreach (var actor in tempList)
            {
                actor.Time -= minTime;
                _actorQueue.Enqueue(actor, actor.Time);
            }
        }
    }
}