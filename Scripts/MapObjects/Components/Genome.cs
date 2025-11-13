using GoRogue.GameFramework;
using Mut8.Scripts.Core;
using Mut8.Scripts.Utils;
using SadConsole.Entities;
using SadRogue.Integration.Components;
using SadRogue.Integration.FieldOfView.Memory;

namespace Mut8.Scripts.MapObjects.Components;

public enum Gene
{
    Strong,
    Quick,
    Resilient,
    Smart,
    Stout,
    Stealthy,
    Photosynthetic
}

internal class Genome : RogueLikeComponentBase<IGameObject>
{
    private readonly Dictionary<Gene, float> _genes;
    private readonly Dictionary<Gene, List<Action<float, float>>> _geneCallbacks;
    private bool _isSpent;

    public IReadOnlyDictionary<Gene, float> Genes => _genes;
    public bool IsSpent => _isSpent;

    private static readonly CellDecorator SpentDecorator = new CellDecorator(new Color(Color.Red, 0.5f), 2548, Mirror.None);
    private static readonly List<CellDecorator> SpentDecoratorList = [SpentDecorator];

    public Genome() : base(false, false, false, false)
    {
        _genes = new Dictionary<Gene, float>();
        _geneCallbacks = new Dictionary<Gene, List<Action<float, float>>>();
        _isSpent = false;
    }

    public Genome(Dictionary<Gene, float> initialGenes) : base(false, false, false, false)
    {
        _genes = new Dictionary<Gene, float>(initialGenes);
        _geneCallbacks = new Dictionary<Gene, List<Action<float, float>>>();
        _isSpent = false;
    }

    public void RegisterGeneChangedCallback(Gene gene, Action<float, float> callback)
    {
        if (!_geneCallbacks.ContainsKey(gene))
        {
            _geneCallbacks[gene] = new List<Action<float, float>>();
        }

        _geneCallbacks[gene].Add(callback);
    }

    public void UnregisterGeneChangedCallback(Gene gene, Action<float, float> callback)
    {
        if (_geneCallbacks.TryGetValue(gene, out var callbacks))
        {
            callbacks.Remove(callback);
        }
    }

    public void TriggerGeneChangedCallback(Gene gene, float oldValue, float newValue)
    {
        if (!_geneCallbacks.TryGetValue(gene, out List<Action<float, float>>? callbacks))
            return;

        foreach (Action<float, float> callback in callbacks)
        {
            callback(oldValue, newValue);
        }
    }

    /// <summary>
    /// Gets the normalized gene value (0.0 to 1.0).
    /// </summary>
    public float GetGeneNormalized(Gene gene, float defaultValue = 0f)
    {
        float rawValue = GetGeneRaw(gene, defaultValue);
        return rawValue / GameData.MaxGeneValue;
    }

    /// <summary>
    /// Gets the raw (unnormalized) gene value.
    /// </summary>
    public float GetGeneRaw(Gene gene, float defaultValue = 0f)
    {
        return _genes.GetValueOrDefault(gene, defaultValue);
    }

    /// <summary>
    /// Sets a gene value by name.
    /// </summary>
    public void SetGene(string geneName, int value)
    {
        if (!Enum.TryParse(geneName, true, out Gene gene))
        {
            throw new ArgumentException($"Unknown gene name: {geneName}");
        }

        SetGene(gene, value);
    }

    public void SetGene(Gene gene, float value)
    {
        float oldValue = _genes.GetValueOrDefault(gene, 0f);
        float newValue = MathF.Min(value, GameData.MaxGeneValue);

        if (newValue.IsEqualWithTolerance(oldValue))
            return;

        _genes[gene] = newValue;

        TriggerGeneChangedCallback(gene, oldValue, newValue);

        Entity? parent = Parent as Entity;
        if (parent.IsPlayer())
            Engine.MainGame?.MessagePanel?.AddMessage(parent, $"Gene {gene} changed from {oldValue:F2} to {newValue:F2}.");
    }

    public bool HasGene(Gene gene)
    {
        return _genes.ContainsKey(gene);
    }

    public void RemoveGene(Gene gene)
    {
        if (_genes.TryGetValue(gene, out var oldValue) && _genes.Remove(gene))
        {
            TriggerGeneChangedCallback(gene, oldValue, 0f);
        }
    }

    public Genome Clone()
    {
        return new Genome(new Dictionary<Gene, float>(_genes));
    }

    public Dictionary<Gene, float> Mutate(IReadOnlyList<Genome> sourceGenomes)
    {
        // Keep track of the genes that were affected by the mutation
        Dictionary<Gene, float> geneDeltas = new Dictionary<Gene, float>();
        
        // Get the genes that are in the source genomes
        Dictionary<Gene, float> genes = new Dictionary<Gene, float>();

        foreach (Genome sourceGenome in sourceGenomes)
        {
            // Find the top gene in this genome
            (Gene gene, float geneValue)? topGeneTuple = sourceGenome.GetHighestGene();
            if (topGeneTuple == null)
                continue;

            Gene topGene = topGeneTuple.Value.gene;
            float topValue = topGeneTuple.Value.geneValue;

            // Add to aggregated genes
            if (topValue > 0f)
            {
                if (!genes.TryGetValue(topGene, out var existing))
                {
                    genes[topGene] = topValue;
                }
                else
                {
                    genes[topGene] = existing + topValue;
                }

                sourceGenome.MarkAsSpent();
            }
        }

        // Apply the aggregated genes to this genome
        foreach (Gene gene in genes.Keys)
        {
            float currentValue = GetGeneRaw(gene);
            float surroundingValue = genes[gene];

            // Increase the gene value based on surrounding value,
            if (surroundingValue > 0f)
            {
                float increase = surroundingValue;
                
                SetGene(gene, currentValue + increase);
                
                float newValue = GetGeneRaw(gene);
                float geneDelta = newValue - currentValue;
                if (!geneDeltas.TryAdd(gene, geneDelta))
                    geneDeltas[gene] += geneDelta;
            }
        }

        // Update the gene scanner if any exists on this parent
        GeneScanner? geneScanner = Parent?.GoRogueComponents.GetFirstOrDefault<GeneScanner>();
        if (geneScanner != null)
        {
            geneScanner.Refresh();
        }

        // Decay the genes that are not affected by the mutation
        foreach (Gene gene in _genes.Keys)
        {
            if (genes.ContainsKey(gene))
                continue;
            
            float baseDecayRate = GameData.GeneDecayRate;
            float currentValue = GetGeneRaw(gene);
            float ratio = currentValue / GameData.MaxGeneValue;
            // Decay the gene. The higher the value, the more it will decay
            float newValue = MathF.Max(0f, currentValue - baseDecayRate * ratio);

            SetGene(gene, newValue);
        }
        
        return geneDeltas;
    }

    public void MarkAsSpent()
    {
        _isSpent = true;

        switch (Parent)
        {
            case Entity entity:
            {
                entity.AppearanceSingle!.Appearance.Decorators = SpentDecoratorList;
                break;
            }
            case MemoryAwareRogueLikeCell cell:
            {
                cell.TrueAppearance.Decorators = SpentDecoratorList;
                break;
            }
        }
    }

    /// <summary>
    /// Gets the gene with the highest value in this genome.
    /// Returns null if no genes exist.
    /// </summary>
    public (Gene gene, float geneValue)? GetHighestGene()
    {
        if (_genes.Count == 0)
            return null;

        Gene topGene = Gene.Strong;
        float topValue = float.MinValue;

        foreach (KeyValuePair<Gene, float> genePair in _genes)
        {
            if (genePair.Value > topValue)
            {
                topGene = genePair.Key;
                topValue = genePair.Value;
            }
        }

        return (topGene, topValue);
    }
}