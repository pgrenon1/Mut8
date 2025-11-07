using Mut8.Scripts.MapObjects.Components;
using SadRogue.Integration;

namespace Mut8.Scripts.Actions;

internal class MutateAction : ActorAction
{
    public MutateAction(RogueLikeEntity entity) : base(entity)
    {
        
    }

    public override ActionResult Perform()
    {
        Genome? genome = Parent!.AllComponents.GetFirstOrDefault<Genome>();
        if (genome == null)
            return ActionResult.Failure;

        GeneScanner? scanner = Parent!.AllComponents.GetFirstOrDefault<GeneScanner>();
        if (scanner == null)
            return ActionResult.Failure;

        if (scanner.SurroundingGenomes.Count == 0)
            return ActionResult.Failure;

        // Find the top gene from each surrounding genome and sum them up
        Dictionary<Gene, float> aggregatedGenes = new Dictionary<Gene, float>();

        foreach (Genome surroundingGenome in scanner.SurroundingGenomes)
        {
            // Find the top gene in this genome
            (Gene gene, float geneValue)? topGeneTuple = surroundingGenome.GetHighestGene();
            if (topGeneTuple == null)
                continue;
            
            Gene topGene = topGeneTuple.Value.gene;
            float topValue = topGeneTuple.Value.geneValue;

            // Add to aggregated genes
            if (topValue > 0f)
            {
                if (!aggregatedGenes.TryGetValue(topGene, out var existing))
                {
                    aggregatedGenes[topGene] = topValue;
                }
                else
                {
                    aggregatedGenes[topGene] = existing + topValue;
                }
            }

            // Mark the source genome as spent
            surroundingGenome.MarkAsSpent();
        }

        // Perform the mutation with the aggregated genes
        genome.Mutate(aggregatedGenes);

        return ActionResult.Success;
    }

    private RogueLikeEntity Parent => Entity;
}