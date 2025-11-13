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

        // Perform the mutation with the aggregated genes
        Dictionary<Gene, float> geneDeltas = genome.Mutate(scanner.SurroundingGenomes);

        // Transform the genes delta into a string to display in the message panel
        
        string geneDeltasString = string.Join(", ", geneDeltas.Select(kv => $"{kv.Key}: {kv.Value}"));
        
        Engine.MainGame.MessagePanel?.AddMessage(Parent, $"{Parent.Name} mutated! {geneDeltasString} [{GetCost()}]");
        
        return ActionResult.SuccessWithTime(GetCost());
    }

    private RogueLikeEntity Parent => Entity;
}