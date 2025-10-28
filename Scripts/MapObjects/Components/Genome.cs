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
    }

    internal class Genome : RogueLikeComponentBase<RogueLikeEntity>
    {
        private readonly Dictionary<Gene, float> _genes;
        
        public IReadOnlyDictionary<Gene, float> Genes => _genes;

        public event EventHandler<GeneChangedEventArgs>? GeneChanged;
        public event EventHandler? GenomeMutated;

        public Genome() : base(false, false, false, false)
        {
            _genes = new Dictionary<Gene, float>();
        }

        public Genome(Dictionary<Gene, float> initialGenes) : base(false, false, false, false)
        {
            _genes = new Dictionary<Gene, float>(initialGenes);
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
            
            GeneChanged?.Invoke(this, new GeneChangedEventArgs(gene, oldValue, value));
        }

        public bool HasGene(Gene gene)
        {
            return _genes.ContainsKey(gene);
        }

        public void RemoveGene(Gene gene)
        {
            if (_genes.TryGetValue(gene, out var oldValue) && _genes.Remove(gene))
            {
                GeneChanged?.Invoke(this, new GeneChangedEventArgs(gene, oldValue, null));
            }
        }

        public void Mutate(Gene gene, float newValue)
        {
            SetGene(gene, newValue);
            GenomeMutated?.Invoke(this, EventArgs.Empty);
        }

        public Genome Clone()
        {
            return new Genome(new Dictionary<Gene, float>(_genes));
        }
    }

    public class GeneChangedEventArgs : EventArgs
    {
        public Gene Gene { get; }
        public float? OldValue { get; }
        public float? NewValue { get; }

        public GeneChangedEventArgs(Gene gene, float? oldValue, float? newValue)
        {
            Gene = gene;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}