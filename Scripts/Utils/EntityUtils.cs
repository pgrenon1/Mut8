using Mut8.Scripts.Core;
using Mut8.Scripts.MapObjects.Components;
using Newtonsoft.Json.Linq;
using SadConsole.Entities;
using SadRogue.Integration;

namespace Mut8.Scripts.Utils;

public static class EntityUtils
{
    public static bool IsPlayer(this Entity? entity)
    {
        return entity is { Name: "Player" };
    }
}