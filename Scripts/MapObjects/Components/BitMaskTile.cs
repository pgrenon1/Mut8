using SadRogue.Integration;
using SadRogue.Integration.Components;
using Mut8.Scripts.Maps;
using System.Linq;

namespace Mut8.Scripts.MapObjects.Components
{
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
            if (Parent?.CurrentMap is not GameMap map)
                return;

            Point pos = Parent.Position;
            int bitmask = GetBitmask(pos.X, pos.Y, map);
       
            // Map bitmask to sprite index
            // The bitmask value (0-15) corresponds to which neighbors match
            // We use it to index into the sprite array
            if (bitmask >= 0 && bitmask < _spriteIndexArray.Length)
            {
                Parent.Appearance.Glyph = _spriteIndexArray[bitmask];
            }
        }

        /// <summary>
        /// Calculates a bitmask based on which neighbors have the same tile group.
        /// </summary>
        /// <param name="x">X position of the tile</param>
        /// <param name="y">Y position of the tile</param>
        /// <param name="map">The game map</param>
        /// <returns>Bitmask value (0-15) indicating which neighbors match</returns>
        private int GetBitmask(int x, int y, GameMap map)
        {
            int mask = 0;

            // Check Up (bit 0)
            if (HasMatchingNeighbor(x, y - 1, map))
                mask |= 1;

            // Check Right (bit 1)
            if (HasMatchingNeighbor(x + 1, y, map))
                mask |= 2;

            // Check Down (bit 2)
            if (HasMatchingNeighbor(x, y + 1, map))
                mask |= 4;

            // Check Left (bit 3)
            if (HasMatchingNeighbor(x - 1, y, map))
                mask |= 8;

            return mask;
        }

        /// <summary>
        /// Checks if a neighboring cell has a BitMaskTile component with the same tile group ID.
        /// </summary>
        private bool HasMatchingNeighbor(int x, int y, GameMap map)
        {
            // Check bounds
            if (x < 0 || x >= map.Width || y < 0 || y >= map.Height)
                return false;

            // Get the terrain cell at the position
            var terrain = map.GetTerrainAt<RogueLikeCell>(new Point(x, y));
    
            if (terrain == null)
                return false;

            // Check if it has a BitMaskTile component with the same group ID
            var neighborBitMask = terrain.GoRogueComponents.GetFirstOrDefault<BitMaskTile>();
      
            return neighborBitMask != null && neighborBitMask._tileGroupId == _tileGroupId;
        }
    }
}
