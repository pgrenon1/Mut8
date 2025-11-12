using Mut8.Scripts.MapObjects.Components;
using Mut8.Scripts.Maps;
using SadRogue.Integration;
using SadRogue.Integration.FieldOfView.Memory;
using GoRogue.Factories;
using GoRogue.GameFramework;
using GoRogue.Random;
using Mut8.Scripts.Core;
using SadRogue.Primitives.GridViews;
using Newtonsoft.Json.Linq;
using SadRogue.Integration.Keybindings;

namespace Mut8.Scripts.MapObjects;

/// Note that SadConsole components cannot be attached directly to `RogueLikeCell` or `MemoryAwareRogueLikeCell`
/// instances for reasons pertaining to performance.
internal static class MapObjectFactory
{
    public static AdvancedFactory<string, Point, RogueLikeCell>? TerrainFactory;
    public static AdvancedFactory<string, Point, RogueLikeEntity>? EntityFactory;
        
    // Store the generated map for bitmask calculations
    private static ISettableGridView<bool>? _generatedMap;

    static MapObjectFactory()
    {
        CreateTerrainFactory();
        CreateEntityFactory();
    }

    /// <summary>
    /// Sets the generated map for bitmask calculations.
    /// </summary>
    /// <param name="generatedMap">The GoRogue generated map (true = wall, false = floor)</param>
    public static void SetGeneratedMap(ISettableGridView<bool> generatedMap)
    {
        _generatedMap = generatedMap;
    }

    internal static IGameObject Floor(Point pos) => TerrainFactory!.Create("floor", pos);
    internal static IGameObject Wall(Point pos) => TerrainFactory!.Create("wall", pos);
    internal static IGameObject Tree(Point pos) => TerrainFactory!.Create("tree", pos);
    internal static IGameObject Flower(Point pos) => TerrainFactory!.Create("flower", pos);
    
    /// <summary>
    /// Parses a color from JSON token - supports both uint (hex) and string (color names)
    /// </summary>
    private static Color ParseColor(JToken? colorToken, Color defaultColor)
    {
        if (colorToken == null)
            return defaultColor;

        // Try to parse as uint (hex value)
        if (colorToken.Type == JTokenType.Integer)
        {
            return new Color(colorToken.Value<uint>());
        }

        // Try to parse as string (color name)
        if (colorToken.Type == JTokenType.String)
        {
            string colorName = colorToken.Value<string>() ?? "";

            // Use reflection to find matching Color field
            var colorType = typeof(Color);
            var field = colorType.GetField(colorName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.IgnoreCase);

            if (field != null && field.FieldType == typeof(Color))
            {
                return (Color)(field.GetValue(null) ?? defaultColor);
            }
        }

        return defaultColor;
    }

    public static RogueLikeEntity CreatePlayer(Point pos)
    {
        RogueLikeEntity player = CreateEntity( "player", pos);

        // Motion control
        var motionControl = new CustomKeybindingsComponent();
        motionControl.SetMotions(KeybindingsComponent.ArrowMotions);
        motionControl.SetMotions(KeybindingsComponent.NumPadAllMotions);
        player.AllComponents.Add(motionControl);

        // FOV controller
        player.AllComponents.Add(new PlayerFOVController());

        // Reveal all tiles component (F3 key)
        player.AllComponents.Add(new RevealAllTilesComponent());

        // GeneScanner
        player.AllComponents.Add(new GeneScanner());
        
        return player;
    }
    
    private static void CreateEntityFactory()
    {
        EntityFactory = new AdvancedFactory<string, Point, RogueLikeEntity>();

        // Load entities from JSON data
        foreach (var entityKey in GameData.Entities.Keys)
        {
            EntityFactory.Add(new LambdaAdvancedFactoryBlueprint<string, Point, RogueLikeEntity>(
                entityKey,
                pos =>
                {
                    RogueLikeEntity entity = CreateEntity(entityKey, pos);

                    entity.AllComponents.Add(new DemoEnemyAI());

                    return entity;
                }
            ));
        }
    }

    private static RogueLikeEntity CreateEntity(string entityKey, Point pos)
    {
        JObject entityData = GameData.GetEntityData(entityKey);

        Color foreground = ParseColor(entityData["foreground"], Color.White);
        Color background = ParseColor(entityData["background"], Color.Black);
        int glyph = GetGlyph(entityData);
        bool isWalkable = entityData["walkable"]?.Value<bool>() ?? false;
        bool isTransparent = entityData["transparent"]?.Value<bool>() ?? true;
        string name = entityData["name"]?.Value<string>() ?? entityKey;
        
        RogueLikeEntity entity = new RogueLikeEntity(
            foreground,
            background,
            glyph,
            isWalkable,
            isTransparent,
            layer: (int)GameMap.Layer.Monsters
        )
        {
            Position = pos,
            Name = name
        };
        
        SetupGenomeComponentOnGameObject(entity, entityData);
        
        // Actor component (enables turn-based actions)
        entity.AllComponents.Add(new Actor(0));

        // Add Health component
        if (entityData.TryGetValue("health", out JToken? healthData))
        {
            int maxHP = healthData["maxHP"]?.Value<int>() ?? 1;
            Health health = new Health(maxHP);
            float regen = healthData["regenHP"]?.Value<float>() ?? 0f;
            health.SetHealthRegen(regen);
            entity.AllComponents.Add(health);
        }

        // Add CombatStats component
        if (entityData.ContainsKey("attack") || entityData.ContainsKey("defense"))
        {
            int attack = entityData["attack"]?.Value<int>() ?? 1;
            int defense = entityData["defense"]?.Value<int>() ?? 0;
            entity.AllComponents.Add(new Stats(attack, defense));
        }

        return entity;
    }

    private static int GetGlyph(JObject data)
    {
        int glyph = data["glyph"]?.Value<int>() ?? 2306;

        if (data.TryGetValue("randomGlyphs", out JToken? randomGlyphs) && randomGlyphs is JArray glyphArray)
        {
            int randomIndex = GlobalRandom.DefaultRNG.NextInt(glyphArray.Count - 1);
            glyph = glyphArray[randomIndex].Value<int>();
        }

        return glyph;
    }

    private static void SetupGenomeComponentOnGameObject(IGameObject gameObject, JObject data)
    {
        if (data.TryGetValue("genome", out JToken? genomeData) && genomeData is JObject genomeObj)
        {
            Genome genome = new Genome();

            // Load gene values from JSON
            foreach (JProperty geneProp in genomeObj.Properties())
            {
                genome.SetGene(geneProp.Name, geneProp.Value.Value<int>());
            }

            gameObject.GoRogueComponents.Add(genome);
        }
    }

    private static void CreateTerrainFactory()
    {
        TerrainFactory = new AdvancedFactory<string, Point, RogueLikeCell>();

        // Load terrain from JSON data
        foreach (var terrainKey in GameData.Terrain.Keys)
        {
            JObject terrainData = GameData.GetTerrainData(terrainKey);
                
            TerrainFactory.Add(new LambdaAdvancedFactoryBlueprint<string, Point, RogueLikeCell>(
                terrainKey,
                pos =>
                {
                    bool isWalkable = !(terrainData["blocking"]?.Value<bool>() ?? false);
                    bool isTransparent = terrainData["transparent"]?.Value<bool>() ?? isWalkable;
                    int glyph = GetGlyph(terrainData);
                        
                    // Check if this terrain type uses bitmask sprites
                    var bitmaskSprites = terrainData["bitmaskSprites"]?.ToObject<int[]>();
                    if (bitmaskSprites != null && bitmaskSprites.Length == 16)
                    {
                        glyph = GetWallSpriteIndex(pos, bitmaskSprites, glyph);
                    }

                    var cell = new MemoryAwareRogueLikeCell(
                        pos,
                        ParseColor(terrainData["color"], Color.White),
                        Color.Black,
                        glyph,
                        (int)GameMap.Layer.Terrain,
                        walkable: isWalkable,
                        transparent: isTransparent
                    );

                    SetupGenomeComponentOnGameObject(cell, terrainData);

                    return cell;
                }
            ));
        }
    }

    /// <summary>
    /// Helper method to determine which sprite index to use based on a bitmask value.
    /// This method maps bitmask values (0-15) to sprite indices using a provided sprite array.
    /// </summary>
    /// <param name="bitmask">The bitmask value (0-15) indicating which neighbors match</param>
    /// <param name="spriteIndexArray">Array of sprite indices to choose from</param>
    /// <param name="defaultSpriteIndex">Default sprite index to use if bitmask is out of bounds</param>
    /// <returns>The sprite index to use for the given bitmask</returns>
    private static int GetSpriteIndexFromBitmask(int bitmask, int[] spriteIndexArray, int defaultSpriteIndex = 0)
    {
        // Validate bitmask range
        if (bitmask < 0 || bitmask >= spriteIndexArray.Length)
        {
            return defaultSpriteIndex;
        }

        return spriteIndexArray[bitmask];
    }

    /// <summary>
    /// Calculates a bitmask based on which neighbors have the same terrain type in the generated map.
    /// </summary>
    /// <param name="x">X position of the tile</param>
    /// <param name="y">Y position of the tile</param>
    /// <param name="isWall">Whether we're checking for wall neighbors (true) or floor neighbors (false)</param>
    /// <returns>Bitmask value (0-15) indicating which neighbors match the terrain type</returns>
    private static int CalculateBitmask(int x, int y, bool isWall)
    {
        if (_generatedMap == null)
            return 0;

        int mask = 0;

        // Check Up (bit 0)
        if (HasMatchingNeighbor(x, y - 1, isWall))
            mask |= 1;

        // Check Left (bit 1)
        if (HasMatchingNeighbor(x - 1, y, isWall))
            mask |= 2;

        // Check Right (bit 2)
        if (HasMatchingNeighbor(x + 1, y, isWall))
            mask |= 4;

        // Check Down (bit 3)
        if (HasMatchingNeighbor(x, y + 1, isWall))
            mask |= 8;

        return mask;
    }

    /// <summary>
    /// Checks if a neighboring cell has the same terrain type in the generated map.
    /// </summary>
    /// <param name="x">X coordinate to check</param>
    /// <param name="y">Y coordinate to check</param>
    /// <param name="isWall">Whether we're checking for wall (true) or floor (false)</param>
    /// <returns>True if the neighbor has the same terrain type</returns>
    private static bool HasMatchingNeighbor(int x, int y, bool isWall)
    {
        if (_generatedMap == null)
            return false;

        // Check bounds
        if (x < 0 || x >= _generatedMap.Width || y < 0 || y >= _generatedMap.Height)
            return false;

        // Check if the neighbor has the same terrain type
        return _generatedMap[x, y] == isWall;
    }

    /// <summary>
    /// Gets the appropriate sprite index for a wall tile based on its neighbors.
    /// </summary>
    /// <param name="pos">Position of the tile</param>
    /// <param name="wallSpriteArray">Array of wall sprite indices</param>
    /// <param name="defaultWallSprite">Default wall sprite index</param>
    /// <returns>The sprite index to use for the wall tile</returns>
    private static int GetWallSpriteIndex(Point pos, int[] wallSpriteArray, int defaultWallSprite = 0)
    {
        int bitmask = CalculateBitmask(pos.X, pos.Y, true); // true = checking for wall neighbors
            
        return GetSpriteIndexFromBitmask(bitmask, wallSpriteArray, defaultWallSprite);
    }
}