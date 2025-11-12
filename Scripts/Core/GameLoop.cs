using Mut8.Scripts.Actions;
using Mut8.Scripts.MapObjects;
using Mut8.Scripts.MapObjects.Components;
using Mut8.Scripts.Utils;

namespace Mut8.Scripts.Core;

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
    private PriorityQueue<Actor, (int time, int insertionOrder)> _actorQueue = new();
    private int _insertionCounter = 0;
    private readonly TurnEventActor _turnEventActor;
    private bool _isPlayerTurn = false;
    
    public GameLoop()
    {
        _turnEventActor = new TurnEventActor(100);
        AddActor(_turnEventActor);
    }

    /// <summary>
    /// Adds an actor to the game loop priority queue.
    /// </summary>
    public void AddActor(Actor actor)
    {
        _actorQueue.Enqueue(actor, (actor.Time, _insertionCounter++));
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
            _actorQueue.Enqueue(a, (time, _insertionCounter));
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
                
            // Peek at the next actor without removing them
            _actorQueue.TryPeek(out Actor currentActor, out _);
            
            // Check if this actor is the player's turn
            bool previousIsPlayer = _isPlayerTurn;
            _isPlayerTurn = currentActor != null && currentActor.Parent.IsPlayer();

            // If this is the player's turn, 
            if (!previousIsPlayer && _isPlayerTurn)
            {
                _isPlayerTurn = true;
                OnPlayerTurnStart(currentActor);
            }
                
            // Check if this is a player waiting for input
            if (_isPlayerTurn)
            {
                IAction? action = currentActor.GetAction();
                if (action == null)
                {
                    // Player has no action ready, stop processing without dequeuing
                    break;
                }
            }
            else
            {
                _isPlayerTurn = false;
            }
                
            // Now actually dequeue since we know we can process this actor
            currentActor = _actorQueue.Dequeue();

            // Try to process the current actor's turn
            ProcessActorTurn(currentActor);

            // Actor has taken their action, put them back in the queue with updated time
            _actorQueue.Enqueue(currentActor, (currentActor.Time, _insertionCounter++));
        }
    }

    private void OnPlayerTurnStart(Actor playerActor)
    {
        playerActor.Parent!.AllComponents.GetFirstOrDefault<GeneScanner>()?.Refresh();
    }

    /// <summary>
    /// Processes a single actor's turn.
    /// Returns true if an action was processed, false if waiting for player input.
    /// </summary>
    private void ProcessActorTurn(Actor actor)
    {
        var action = actor.GetAction();

        if (action == null)
        {
            // AI has no action, skip by adding minimal time
            actor.Time += 100;
            return;
        }
            
        actor.ClearNextAction();
            
        var result = action.Perform();

        while (result.Alternate != null)
        {
            if (result.TimeCost > 0)
            {
                // Get the time cost from the action and multiply it by the actor's speed modifier if needed
                var statComponent = actor.Parent!.GetSadComponent<Stats>();
                float speedMultiplier = statComponent?.GetSpeedMultiplier() ?? 1.0f;

                actor.Time += (int)Math.Round(result.TimeCost * speedMultiplier);
            }

            action = result.Alternate;
            result = action.Perform();
        }

        if (result.Succeeded)
        {
            actor.Time += result.TimeCost;
            // PrintQueue();
        }
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
            _actorQueue.Enqueue(actor, (actor.Time, _insertionCounter++));
        }
    }
        
    /// <summary>
    /// Prints the current state of the actor queue to debug output.
    /// </summary>
    private void PrintQueue()
    {
        var tempList = new List<(Actor actor, int time)>();
            
        // Collect all actors from the queue
        while (_actorQueue.Count > 0)
        {
            var actor = _actorQueue.Dequeue();
            tempList.Add((actor, actor.Time));
        }
            
        // Print the queue state
        System.Diagnostics.Debug.WriteLine("=== Actor Queue ===");
        foreach (var (actor, time) in tempList.OrderBy(x => x.time))
        {
            var actorName = actor.Parent?.Name ?? actor.GetType().Name;
            System.Diagnostics.Debug.WriteLine($"  {actorName} - Time: {time}");
        }
        System.Diagnostics.Debug.WriteLine("==================");
            
        // Restore the queue
        foreach (var (actor, time) in tempList)
        {
            _actorQueue.Enqueue(actor, (time, _insertionCounter++));
        }
    }

    public TurnEventActor? GetTurnEventActor()
    {
        return _turnEventActor;
    }
}