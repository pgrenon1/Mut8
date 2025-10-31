using SadConsole;
using SadRogue.Primitives;
using System;
using System.Linq;

namespace Mut8.Scripts.UI
{
    /// <summary>
    /// A layout group that arranges children vertically from top to bottom.
    /// Children can be sized automatically based on flex values or use fixed sizes.
    /// </summary>
    public class VerticalLayoutGroup : LayoutGroup
    {
        public VerticalLayoutGroup(int width, int height) : base(width, height)
        {
        }

        protected override void PerformLayout()
        {
            if (_layoutHandles.Count == 0) return;

            // Calculate available space
            int contentWidth = Size.X - Padding.X - Padding.Width;
            int totalHeight = Size.Y - Padding.Y - Padding.Height;
            int totalSpacing = Spacing * (_layoutHandles.Count - 1);

            // Subtract spacing from available height
            totalHeight -= totalSpacing;

            // Calculate total fixed height and total flex
            int fixedHeight = 0;
            float totalFlex = 0f;

            foreach (var handle in _layoutHandles)
            {
                if (handle.FixedSize > 0)
                {
                    fixedHeight += handle.FixedSize;
                }
                else
                {
                    totalFlex += handle.FlexGrow;
                }

                // Add padding to fixed height
                fixedHeight += handle.Padding.Y + handle.Padding.Height;
            }

            // Calculate flexible space
            int flexibleHeight = Math.Max(0, totalHeight - fixedHeight);

            // Position and size each child
            int currentX = Padding.X;
            int currentY = Padding.Y;

            foreach (var handle in _layoutHandles)
            {
                // Calculate child height
                int childHeight;
                if (handle.FixedSize > 0)
                {
                    childHeight = handle.FixedSize;
                }
                else
                {
                    childHeight = CalculateChildSize(handle, flexibleHeight, totalFlex);
                }

                // Calculate child width (fills available width minus padding)
                int childWidth = contentWidth - handle.Padding.X - handle.Padding.Width;

                // Apply padding offset
                int childX = currentX + handle.Padding.X;
                int childY = currentY + handle.Padding.Y;

                // Set child position
                handle.Child.Position = new Point(childX, childY);

                // If the child is a ScreenSurface or similar, try to resize it
                if (handle.Child is ScreenSurface surface)
                {
                    // Calculate size in cells (assuming font cell size)
                    // This is a simplified approach - you may need to adjust based on your needs
                    int cellWidth = Math.Max(1, childWidth);
                    int cellHeight = Math.Max(1, childHeight);

                    // Only resize if the surface supports it
                    try
                    {
                        surface.Surface.View = new Rectangle(0, 0, cellWidth, cellHeight);
                    }
                    catch
                    {
                        // Silently fail if resize not supported
                    }
                }

                // Move to next position
                currentY += childHeight + handle.Padding.Y + handle.Padding.Height + Spacing;
            }
        }
    }
}
