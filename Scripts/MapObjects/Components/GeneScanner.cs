using GoRogue.GameFramework;
using SadRogue.Integration;
using SadRogue.Integration.Components;
using SadRogue.Primitives.GridViews;

namespace Mut8.Scripts.MapObjects.Components;

/// <summary>
/// Component that monitors surrounding tiles for Genome components and tracks the highest gene values.
/// </summary>
internal class GeneScanner : RogueLikeComponentBase<RogueLikeEntity>
{
    private readonly Dictionary<Gene, float> _surroundingGenes;
        
    public IReadOnlyDictionary<Gene, float> SurroundingGenes => _surroundingGenes;

    public GeneScanner() : base(false, false, false, false)
    {
        _surroundingGenes = new Dictionary<Gene, float>();
    }

    public override void OnAdded(IScreenObject host)
    {
        base.OnAdded(host);
            
        // Register for position changes
        host.PositionChanged += OnPositionChanged;
        // Initial scan
        UpdateSurroundingGenes();
    }

    public override void OnRemoved(IScreenObject host)
    {
        base.OnRemoved(host);
            
        // Unregister from position changes
        host.PositionChanged -= OnPositionChanged;
    }

    private void OnPositionChanged(object? sender, ValueChangedEventArgs<Point> e)
    {
        UpdateSurroundingGenes();
    }

    /// <summary>
    /// Scans the 8 surrounding tiles for Genome components and sums values for top genes.
    /// If multiple genomes have the same gene as their top gene, those values are added together.
    /// </summary>
    private void UpdateSurroundingGenes()
    {
        if (Parent?.CurrentMap == null) return;

        // Clear previous data
        _surroundingGenes.Clear();

        // Check all 8 adjacent positions
        Direction[] adjacentPositions = AdjacencyRule.EightWay.DirectionsOfNeighborsCache;
            
        for (int i = 0; i < adjacentPositions.Length; i++)
        {
            Direction direction = adjacentPositions[i];
            Point checkPos = Parent.Position + direction;
                
            // Skip if position is out of bounds
            if (!Parent.CurrentMap.Contains(checkPos)) continue;

            // Check all objects at this position for Genome components
            foreach (IGameObject obj in Parent.CurrentMap.GetObjectsAt(checkPos))
            {
                Genome? genome = obj.GoRogueComponents.GetFirstOrDefault<Genome>();
                if (genome == null || genome.Genes.Count == 0) continue;

                // Find the top gene (gene with highest value) in this genome
                Gene topGene = 0;
                float topValue = 0f;
                    
                foreach (KeyValuePair<Gene, float> genePair in genome.Genes)
                {
                    if (genePair.Value > topValue)
                    {
                        topGene = genePair.Key;
                        topValue = genePair.Value;
                    }
                }

                // Add the top gene's value to our dictionary (sum if already exists)
                if (topValue > 0f)
                {
                    if (!_surroundingGenes.TryGetValue(topGene, out var existing))
                    {
                        _surroundingGenes[topGene] = topValue;
                    }
                    else
                    {
                        _surroundingGenes[topGene] = existing + topValue;
                    }
                }
            }
        }
            
        System.Diagnostics.Debug.WriteLine($"SurroundingGenes: {_surroundingGenes.Count}");
    }

    /// <summary>
    /// Manually triggers an update of surrounding genes.
    /// </summary>
    public void Refresh()
    {
        UpdateSurroundingGenes();
    }

    /// <summary>
    /// Gets the summed value for a specific gene in the surrounding area.
    /// </summary>
    public float GetSurroundingGene(Gene gene)
    {
        return _surroundingGenes.TryGetValue(gene, out var value) ? value : 0f;
    }

    /// <summary>
    /// Checks if a specific gene exists in the surrounding area.
    /// </summary>
    public bool HasSurroundingGene(Gene gene)
    {
        return _surroundingGenes.ContainsKey(gene);
    }
}