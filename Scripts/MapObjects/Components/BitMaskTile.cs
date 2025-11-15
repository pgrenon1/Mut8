using GoRogue.MapGeneration;
using SadRogue.Integration;
using SadRogue.Integration.Components;
using Mut8.Scripts.Maps;
using SadRogue.Integration.FieldOfView.Memory;

namespace Mut8.Scripts.MapObjects.Components;

internal class BitMaskTile : RogueLikeComponentBase<RogueLikeCell>
{
    private readonly int[] _spriteIndexArray;
    private readonly string _tileGroupId;

    public BitMaskTile(int[] spriteIndexArray) : base(false, false, false, false)
    {
        _spriteIndexArray = spriteIndexArray;
            
        // Generate a unique ID from the sprite index array for comparison
        // We use a hash-based approach or concatenation
        _tileGroupId = string.Join(",", spriteIndexArray);
    }

    public BitMaskTile(char[] charArray) : base(false, false, false, false)
    {
        // Keep char values as-is (implicit conversion to int preserves the character)
        _spriteIndexArray = Array.ConvertAll(charArray, c => (int)c);
  
        // Generate a unique ID from the char array for comparison
        _tileGroupId = new string(charArray);
    }

    public override void OnAdded(IScreenObject host)
    {
        base.OnAdded(host);

        UpdateTileBasedOnNeighbors();
    }

    /// <summary>
    /// Updates the tile sprite based on neighboring tiles with the same tile group.
    /// </summary>
    public void UpdateTileBasedOnNeighbors()
    {
        Point pos = Parent.Position;
        int bitmask = GetBitmask(pos.X, pos.Y);
       
        // Map bitmask to sprite index
        // The bitmask value (0-15) corresponds to which neighbors match
        // We use it to index into the sprite array
        if (bitmask >= 0 && bitmask < _spriteIndexArray.Length)
        {
            Parent.Appearance.Glyph = _spriteIndexArray[bitmask];
            if (Parent is MemoryAwareRogueLikeCell cell)
            {
                // PG: I don't know why I need to reverse the array, but it works oupsi!
                cell.TrueAppearance.Glyph = _spriteIndexArray.Reverse().ToArray()[bitmask];
            }
        }
    }

    /// <summary>
    /// Calculates a bitmask based on which neighbors have the same tile group.
    /// </summary>
    /// <param name="x">X position of the tile</param>
    /// <param name="y">Y position of the tile</param>
    /// <returns>Bitmask value (0-15) indicating which neighbors match</returns>
    private int GetBitmask(int x, int y)
    {
        int mask = 0;

        // Check Up (bit 0)
        if (HasMatchingNeighbor(x, y - 1))
            mask |= 1;

        // Check Left (bit 1)
        if (HasMatchingNeighbor(x - 1, y))
            mask |= 2;

        // Check Right (bit 2)
        if (HasMatchingNeighbor(x + 1, y))
            mask |= 4;

        // Check Down (bit 3)
        if (HasMatchingNeighbor(x, y + 1))
            mask |= 8;

        return mask;
    }

    /// <summary>
    /// Checks if a neighboring cell has a BitMaskTile component with the same tile group ID.
    /// </summary>
    private bool HasMatchingNeighbor(int x, int y)
    {
        if (Parent == null)
        {
            throw new InvalidOperationException("Parent is null");
        }

        GameMap gameMap = (GameMap)Parent.CurrentMap!;

        // Check bounds
        if (x < 0 || x >= gameMap.Width || y < 0 || y >= gameMap.Height)
            return false;

        // Get the terrain cell at the position
        var terrain = gameMap.GetTerrainAt<RogueLikeCell>(new Point(x, y));
    
        if (terrain == null)
            return false;

        // Check if it has a BitMaskTile component with the same group ID
        var neighborBitMask = terrain.GoRogueComponents.GetFirstOrDefault<BitMaskTile>();
      
        return neighborBitMask != null && neighborBitMask._tileGroupId == _tileGroupId;
    }
}