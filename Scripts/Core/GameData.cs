using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mut8.Scripts.Core;

/// <summary>
/// Contains all game balance constants for tuning gameplay.
/// </summary>
internal static class GameData
{
    public static float PlayerBaseMaxHP { get; private set; } = 100f;
    public static float PlayerBaseAttackPower { get; private set; } = 10f;
    public static float PlayerBaseDefense { get; private set; } = 5f;

    public static float MaxGeneValue { get; private set; } = 100f;
    public static float StoutGeneHPMultiplier { get; private set; } = 4f;
    public static float StrongGeneAttackMultiplier { get; private set; } = 5f;
    public static float ResilientGeneDefenseMultiplier { get; private set; } = 3f;
    public static float QuickGeneSpeedMultiplier { get; private set; } = 2f;
    public static float SmartGeneXPMultiplier { get; private set; } = 1f;
    public static float StealthyGeneDetectionMultiplier { get; private set; } = 1f;
    public static float PhotosyntheticGeneRegenMultiplier { get; private set; } = 5f;
    
    // Entity and terrain data
    public static JObject EntitiesData { get; private set; }
    public static Dictionary<string, JObject> Entities { get; private set; }
    public static Dictionary<string, JObject> Terrain { get; private set; }

    private static bool _isLoaded = false;

    /// <summary>
    /// Loads all game data from JSON files.
    /// </summary>
    public static void LoadData()
    {
        if (_isLoaded) return;

        string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Data");
            
        LoadConstants(Path.Combine(basePath, "Constants.json"));
        LoadEntities(Path.Combine(basePath, "Entities.json"));
            
        _isLoaded = true;
    }

    private static void LoadConstants(string filePath)
    {
        if (!File.Exists(filePath))
        {
            System.Diagnostics.Debug.WriteLine($"Constants file not found at {filePath}.");
            return;
        }

        string json = File.ReadAllText(filePath);
        JObject? constants = JsonConvert.DeserializeObject<JObject>(json);

        if (constants == null)
            return;

        PlayerBaseMaxHP = constants["PlayerBaseMaxHP"]?.Value<float>() ?? PlayerBaseMaxHP;
        PlayerBaseAttackPower = constants["PlayerBaseAttackPower"]?.Value<float>() ?? PlayerBaseAttackPower;
        PlayerBaseDefense = constants["PlayerBaseDefense"]?.Value<float>() ?? PlayerBaseDefense;

        MaxGeneValue = constants["MaxGeneValue"]?.Value<float>() ?? MaxGeneValue;
        StoutGeneHPMultiplier = constants["StoutGeneHPMultiplier"]?.Value<float>() ?? StoutGeneHPMultiplier;
        StrongGeneAttackMultiplier = constants["StrongGeneAttackMultiplier"]?.Value<float>() ?? StrongGeneAttackMultiplier;
        ResilientGeneDefenseMultiplier = constants["ResilientGeneDefenseMultiplier"]?.Value<float>() ?? ResilientGeneDefenseMultiplier;
        QuickGeneSpeedMultiplier = constants["QuickGeneSpeedMultiplier"]?.Value<float>() ?? QuickGeneSpeedMultiplier;
        SmartGeneXPMultiplier = constants["SmartGeneXPMultiplier"]?.Value<float>() ?? SmartGeneXPMultiplier;
        StealthyGeneDetectionMultiplier = constants["StealthyGeneDetectionMultiplier"]?.Value<float>() ?? StealthyGeneDetectionMultiplier;
        PhotosyntheticGeneRegenMultiplier = constants["PhotosyntheticGeneRegenMultiplier"]?.Value<float>() ?? PhotosyntheticGeneRegenMultiplier;
    }

    private static void LoadEntities(string filePath)
    {
        if (!File.Exists(filePath))
        {
            System.Diagnostics.Debug.WriteLine($"Entities file not found at {filePath}.");
            EntitiesData = new JObject();
            Entities = new Dictionary<string, JObject>();
            Terrain = new Dictionary<string, JObject>();
            return;
        }

        var json = File.ReadAllText(filePath);
        EntitiesData = JsonConvert.DeserializeObject<JObject>(json);

        // Parse entities
        Entities = new Dictionary<string, JObject>();
        if (EntitiesData["entities"] is JObject entitiesObj)
        {
            foreach (var prop in entitiesObj.Properties())
            {
                Entities[prop.Name] = prop.Value as JObject;
            }
        }

        // Parse terrain
        Terrain = new Dictionary<string, JObject>();
        if (EntitiesData["terrain"] is JObject terrainObj)
        {
            foreach (var prop in terrainObj.Properties())
            {
                Terrain[prop.Name] = prop.Value as JObject;
            }
        }
    }

    /// <summary>
    /// Gets entity data by key.
    /// </summary>
    public static JObject GetEntityData(string key)
    {
        return Entities.TryGetValue(key, out var data) ? data : null;
    }

    /// <summary>
    /// Gets terrain data by key.
    /// </summary>
    public static JObject GetTerrainData(string key)
    {
        return Terrain.TryGetValue(key, out var data) ? data : null;
    }
}