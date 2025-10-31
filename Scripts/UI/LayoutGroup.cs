using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mut8.Scripts.UI
{
    /// <summary>
    /// Represents layout properties for a child in a layout group.
    /// </summary>
    public struct LayoutHandle
    {
        /// <summary>
        /// The child screen object being laid out.
        /// </summary>
        public IScreenObject Child { get; internal set; }

        /// <summary>
        /// The weight/flex value for this child. Higher values get more space.
        /// Default is 1.0. Use 0 for fixed-size children.
        /// </summary>
        public float FlexGrow { get; set; }

        /// <summary>
        /// Fixed size in pixels. If > 0, the child maintains this size instead of flexing.
        /// For horizontal layouts, this is width. For vertical layouts, this is height.
        /// </summary>
        public int FixedSize { get; set; }

        /// <summary>
        /// Minimum size in pixels. The child won't shrink below this size.
        /// </summary>
        public int MinSize { get; set; }

        /// <summary>
        /// Maximum size in pixels. The child won't grow beyond this size. 0 means no limit.
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// Padding in pixels around this child (left, top, right, bottom).
        /// </summary>
        public Rectangle Padding { get; set; }

        public LayoutHandle(IScreenObject child, float flexGrow = 1f, int fixedSize = 0)
        {
            Child = child;
            FlexGrow = flexGrow;
            FixedSize = fixedSize;
            MinSize = 0;
            MaxSize = 0;
            Padding = Rectangle.Empty;
        }
    }

    /// <summary>
    /// Base class for layout groups that automatically arrange and size their children.
    /// </summary>
    public abstract class LayoutGroup : ScreenObject
    {
        protected List<LayoutHandle> _layoutHandles = new List<LayoutHandle>();
        private Point _size;
        private Rectangle _padding;
        private int _spacing;

        /// <summary>
        /// The size of this layout group in pixels.
        /// </summary>
        public Point Size
        {
            get => _size;
            set
            {
                if (_size == value) return;
                _size = value;
                PerformLayout();
            }
        }

        /// <summary>
        /// Padding around the entire layout group (left, top, right, bottom).
        /// </summary>
        public Rectangle Padding
        {
            get => _padding;
            set
            {
                if (_padding == value) return;
                _padding = value;
                PerformLayout();
            }
        }

        /// <summary>
        /// Spacing in pixels between children.
        /// </summary>
        public int Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                PerformLayout();
            }
        }

        /// <summary>
        /// Gets all layout handles for children in this group.
        /// </summary>
        public IReadOnlyList<LayoutHandle> LayoutHandles => _layoutHandles.AsReadOnly();

        public LayoutGroup(int width, int height) : base()
        {
            _size = new Point(width, height);
            _padding = Rectangle.Empty;
            _spacing = 0;
        }

        /// <summary>
        /// Adds a child to the layout with specified layout properties.
        /// </summary>
        public LayoutHandle AddChild(IScreenObject child, float flexGrow = 1f, int fixedSize = 0)
        {
            var handle = new LayoutHandle(child, flexGrow, fixedSize);
            _layoutHandles.Add(handle);
            Children.Add(child);
            PerformLayout();
            return handle;
        }

        /// <summary>
        /// Updates layout properties for a child.
        /// </summary>
        public void UpdateHandle(IScreenObject child, Action<LayoutHandle> updateAction)
        {
            int index = _layoutHandles.FindIndex(h => h.Child == child);
            if (index >= 0)
            {
                var handle = _layoutHandles[index];
                updateAction(handle);
                _layoutHandles[index] = handle;
                PerformLayout();
            }
        }

        /// <summary>
        /// Removes a child from the layout.
        /// </summary>
        public void RemoveChild(IScreenObject child)
        {
            int index = _layoutHandles.FindIndex(h => h.Child == child);
            if (index >= 0)
            {
                _layoutHandles.RemoveAt(index);
                Children.Remove(child);
                PerformLayout();
            }
        }

        /// <summary>
        /// Removes all children from the layout.
        /// </summary>
        public void ClearChildren()
        {
            _layoutHandles.Clear();
            Children.Clear();
        }

        /// <summary>
        /// Performs the layout calculation and positions/sizes all children.
        /// </summary>
        protected abstract void PerformLayout();

        /// <summary>
        /// Calculates the actual size for a child based on its handle properties and available space.
        /// </summary>
        protected int CalculateChildSize(LayoutHandle handle, int availableSpace, float totalFlex)
        {
            // If fixed size, use that
            if (handle.FixedSize > 0)
                return handle.FixedSize;

            // Calculate flex-based size
            int size = totalFlex > 0 ? (int)(availableSpace * (handle.FlexGrow / totalFlex)) : 0;

            // Apply constraints
            if (handle.MinSize > 0)
                size = Math.Max(size, handle.MinSize);
            if (handle.MaxSize > 0)
                size = Math.Min(size, handle.MaxSize);

            return size;
        }
    }
}
