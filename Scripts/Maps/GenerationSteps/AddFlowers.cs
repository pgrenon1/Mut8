using GoRogue.MapGeneration;
using GoRogue.Random;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;

namespace Mut8.Scripts.Maps.GenerationSteps;

public class AddFlowers : GenerationStep
{
    public readonly string? WallFloorComponentTag;
    public readonly string? FlowerComponentTag;
    
    public AddFlowers(string? wallFloorComponentTag = "WallFloor", string? flowerComponentTag = "Flowers")
    {
        WallFloorComponentTag = wallFloorComponentTag;
        FlowerComponentTag = flowerComponentTag;
    }
    
    protected override IEnumerator<object?> OnPerform(GenerationContext context)
    {
        // Get the grid view component that stores/sets floor/wall status
        ISettableGridView<bool> wallFloorContext = context.GetFirst<ISettableGridView<bool>>(WallFloorComponentTag);
        
        // Make a new grid for the flower map
        ArrayView<bool> flowerMap = new ArrayView<bool>(wallFloorContext.Width, wallFloorContext.Height);
        
        // Fill the flower map with random values
        for (int x = 0; x < flowerMap.Width; x++)
        {
            for (int y = 0; y < flowerMap.Height; y++)
            {
                if (!wallFloorContext[x, y])
                    continue;
                
                flowerMap[x, y] = GlobalRandom.DefaultRNG.PercentageCheck(10f);
            }
        }
        
        context.Add(flowerMap, FlowerComponentTag);
        
        yield return null;
    }
}