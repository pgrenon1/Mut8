using SadConsole;
using SadRogue.Primitives;
using System;
using System.Linq;

namespace Mut8.Scripts.UI
{
    /// <summary>
    /// A layout group that arranges children horizontally from left to right.
    /// Children can be sized automatically based on flex values or use fixed sizes.
    /// </summary>
    public class HorizontalLayoutGroup : LayoutGroup
    {
        public HorizontalLayoutGroup(int width, int height) : base(width, height)
        {
        }

        protected override void PerformLayout()
        {
            if (_layoutHandles.Count == 0) return;

            // Calculate available space
            int totalWidth = Size.X - Padding.X - Padding.Width;
            int contentHeight = Size.Y - Padding.Y - Padding.Height;
            int totalSpacing = Spacing * (_layoutHandles.Count - 1);

            // Subtract spacing from available width
            totalWidth -= totalSpacing;

            // Calculate total fixed width and total flex
            int fixedWidth = 0;
            float totalFlex = 0f;

            foreach (var handle in _layoutHandles)
            {
                if (handle.FixedSize > 0)
                {
                    fixedWidth += handle.FixedSize;
                }
                else
                {
                    totalFlex += handle.FlexGrow;
                }

                // Add padding to fixed width
                fixedWidth += handle.Padding.X + handle.Padding.Width;
            }

            // Calculate flexible space
            int flexibleWidth = Math.Max(0, totalWidth - fixedWidth);

            // Position and size each child
            int currentX = Padding.X;
            int currentY = Padding.Y;

            foreach (var handle in _layoutHandles)
            {
                // Calculate child width
                int childWidth;
                if (handle.FixedSize > 0)
                {
                    childWidth = handle.FixedSize;
                }
                else
                {
                    childWidth = CalculateChildSize(handle, flexibleWidth, totalFlex);
                }

                // Calculate child height (fills available height minus padding)
                int childHeight = contentHeight - handle.Padding.Y - handle.Padding.Height;

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
                currentX += childWidth + handle.Padding.X + handle.Padding.Width + Spacing;
            }
        }
    }
}
