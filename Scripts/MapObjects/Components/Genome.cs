using SadRogue.Integration;
using SadRogue.Integration.Components;

namespace Mut8.Scripts.MapObjects.Components
{
    public enum Gene
    {
        Strong,
        Quick,
        Resilient,
        Smart,
        Stout,
        Stealthy
    }

    internal class Genome : RogueLikeComponentBase<RogueLikeEntity>
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
            if (_geneCallbacks.TryGetValue(gene, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback(oldValue, newValue);
                }
            }
        }

        public float GetGene(Gene gene, float defaultValue = 0f)
        {
            if (_genes.TryGetValue(gene, out var value))
            {
                return value;
            }
            return defaultValue;
        }

        public void SetGene(Gene gene, float value)
        {
            var oldValue = _genes.TryGetValue(gene, out var existing) ? existing : 0f;
            _genes[gene] = value;
            
            TriggerGeneChangedCallback(gene, oldValue, value);
            
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
    }
}