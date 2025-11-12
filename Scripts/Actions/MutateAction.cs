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
        genome.Mutate(scanner.SurroundingGenomes);

        return ActionResult.SuccessWithTime(GetCost());
    }

    private RogueLikeEntity Parent => Entity;
}