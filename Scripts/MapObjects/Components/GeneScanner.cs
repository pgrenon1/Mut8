using GoRogue.GameFramework;
using SadRogue.Integration;
using SadRogue.Integration.Components;
using SadRogue.Primitives.GridViews;

namespace Mut8.Scripts.MapObjects.Components;

/// <summary>
/// Component that monitors surrounding tiles for Genome components.
/// </summary>
internal class GeneScanner : RogueLikeComponentBase<RogueLikeEntity>
{
    private readonly List<Genome> _surroundingGenomes;

    public IReadOnlyList<Genome> SurroundingGenomes => _surroundingGenomes;

    public Action OnGeneScannerUpdated;
    
    public GeneScanner() : base(false, false, false, false)
    {
        _surroundingGenomes = new List<Genome>();
    }

    public override void OnAdded(IScreenObject host)
    {
        base.OnAdded(host);
            
        // Register for position changes
        host.PositionChanged += OnPositionChanged;
        // Initial scan
        UpdateSurroundingGenomes();
    }

    public override void OnRemoved(IScreenObject host)
    {
        base.OnRemoved(host);
            
        // Unregister from position changes
        host.PositionChanged -= OnPositionChanged;
    }

    private void OnPositionChanged(object? sender, ValueChangedEventArgs<Point> e)
    {
        UpdateSurroundingGenomes();
    }

    /// <summary>
    /// Scans the 8 surrounding tiles for Genome components.
    /// Only includes genomes that are not spent and have genes.
    /// </summary>
    private void UpdateSurroundingGenomes()
    {
        if (Parent?.CurrentMap == null) 
            return;

        // Clear previous data
        _surroundingGenomes.Clear();

        // Check all 8 adjacent positions
        Direction[] adjacentPositions = AdjacencyRule.EightWay.DirectionsOfNeighborsCache;

        for (int i = 0; i < adjacentPositions.Length; i++)
        {
            Direction direction = adjacentPositions[i];
            Point checkPos = Parent.Position + direction;

            // Skip if position is out of bounds
            if (!Parent.CurrentMap.Contains(checkPos)) 
                continue;

            // Check all objects at this position for Genome components
            foreach (IGameObject obj in Parent.CurrentMap.GetObjectsAt(checkPos))
            {
                Genome? genome = obj.GoRogueComponents.GetFirstOrDefault<Genome>();
                if (genome == null || genome.Genes.Count == 0 || genome.IsSpent) 
                    continue;

                _surroundingGenomes.Add(genome);
            }
        }

        OnGeneScannerUpdated?.Invoke();
    }

    /// <summary>
    /// Manually triggers an update of surrounding genomes.
    /// </summary>
    public void Refresh()
    {
        UpdateSurroundingGenomes();
    }
}