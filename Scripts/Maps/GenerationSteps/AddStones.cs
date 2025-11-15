using GoRogue.MapGeneration;
using GoRogue.Random;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;

namespace Mut8.Scripts.Maps.GenerationSteps;

public class AddStones : GenerationStep
{
    public readonly string? WallFloorComponentTag;
    public readonly string? StoneComponentTag;
    
    public AddStones(string? wallFloorComponentTag = "WallFloor", string? stoneComponentTag = "Stones")
    {
        WallFloorComponentTag = wallFloorComponentTag;
        StoneComponentTag = stoneComponentTag;
    }

    protected override IEnumerator<object?> OnPerform(GenerationContext context)
    {
        ISettableGridView<bool> wallFloorContext = context.GetFirst<ISettableGridView<bool>>(WallFloorComponentTag);
        ISettableGridView<bool> stoneContext = new ArrayView<bool>(wallFloorContext.Width, wallFloorContext.Height);
        
        for (int x = 0; x < wallFloorContext.Width; x++)
        {
            for (int y = 0; y < wallFloorContext.Height; y++)
            {
                bool isStone = !wallFloorContext[x,y] && GlobalRandom.DefaultRNG.PercentageCheck(1f);
                if (isStone)
                {
                    wallFloorContext[x,y] = false;
                }

                stoneContext[x,y] = isStone;
            }
        }
        
        context.Add(stoneContext, StoneComponentTag);
        
        yield return null;
    }
}