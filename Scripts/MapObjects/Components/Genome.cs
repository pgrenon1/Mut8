using GoRogue.GameFramework;
using Mut8.Scripts.Core;
using Mut8.Scripts.Utils;
using SadConsole.Entities;
using SadRogue.Integration.Components;

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
        
    public IReadOnlyDictionary<Gene, float> Genes => _genes;

    public Genome() : base(false, false, false, false)
    {
        _genes = new Dictionary<Gene, float>();
        _geneCallbacks = new Dictionary<Gene, List<Action<float, float>>>();
    }

    public Genome(Dictionary<Gene, float> initialGenes) : base(false, false, false, false)
    {
        _genes = new Dictionary<Gene, float>(initialGenes);
        _geneCallbacks = new Dictionary<Gene, List<Action<float, float>>>();
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
    public float GetGene(Gene gene, float defaultValue = 0f)
    {
        float rawValue = GetRawGene(gene, defaultValue);
        return rawValue / GameData.MaxGeneValue;
    }

    /// <summary>
    /// Gets the raw (unnormalized) gene value.
    /// </summary>
    public float GetRawGene(Gene gene, float defaultValue = 0f)
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
        _genes[gene] = value;
            
        TriggerGeneChangedCallback(gene, oldValue, value);
            
        if ((Parent as Entity).IsPlayer())
            Engine.MainGame?.MessagePanel?.AddMessage($"Gene {gene} changed from {oldValue:F2} to {value:F2}.");
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

    public void Mutate(IReadOnlyDictionary<Gene, float> scannerSurroundingGenes)
    {
        foreach (Gene gene in scannerSurroundingGenes.Keys)
        {
            float currentValue = GetRawGene(gene);
            float surroundingValue = scannerSurroundingGenes[gene];
                
            // Increase the gene value based on surrounding value,
            if (surroundingValue > 0f)
            {
                float increase = surroundingValue;
                SetGene(gene, currentValue + increase);
            }
        }
    }
}