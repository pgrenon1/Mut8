using SadConsole.UI.Controls;

namespace Mut8.Scripts.UI.Controls;

public class LayeredProgressBar : ProgressBar
{
    public enum ChildOffsetType
    {
        FromStart,
        FromEnd
    }

    public class ChildProgressBar
    {
        public ProgressBar Bar { get; set; }
        public ChildOffsetType OffsetType { get; set; }

        public ChildProgressBar(ProgressBar bar, ChildOffsetType offsetType)
        {
            Bar = bar;
            OffsetType = offsetType;
        }
    }

    private List<ChildProgressBar> _childBars = new List<ChildProgressBar>();

    public IReadOnlyList<ChildProgressBar> ChildBars => _childBars.AsReadOnly();

    public LayeredProgressBar(int width, int height, HorizontalAlignment horizontalAlignment) : base(width, height, horizontalAlignment)
    {
    }

    public LayeredProgressBar(int width, int height, VerticalAlignment verticalAlignment) : base(width, height, verticalAlignment)
    {
    }

    public void AddChildBar(ProgressBar bar, ChildOffsetType offsetType)
    {
        _childBars.Add(new ChildProgressBar(bar, offsetType));

        // Ensure proper alignment for FromEnd so fill grows to the right from the boundary
        if (offsetType == ChildOffsetType.FromEnd && IsHorizontal)
        {
            try
            {
                bar.HorizontalAlignment = HorizontalAlignment.Left;
            }
            catch { /* Ignore if vertical bar; handled in UpdateChildBar */ }
        }

        UpdateChildBar(bar, offsetType);
    }

    public void RemoveChildBar(ProgressBar bar)
    {
        _childBars.RemoveAll(c => c.Bar == bar);
    }

    public void ClearChildBars()
    {
        _childBars.Clear();
    }

    public void SetProgress(float progress)
    {
        Progress = progress;
        UpdateAllChildBars();
    }

    private void UpdateAllChildBars()
    {
        foreach (var child in _childBars)
        {
            UpdateChildBar(child.Bar, child.OffsetType);
        }
    }

    private void UpdateChildBar(ProgressBar childBar, ChildOffsetType offsetType)
    {
        float parentProgress = Progress;
        float childProgress = childBar.Progress;

        // Ensure child shares our Y position
        childBar.Position = (childBar.Position.X, Position.Y);

        if (IsHorizontal)
        {
            if (offsetType == ChildOffsetType.FromStart)
            {
                // Child begins at parent's start; clamp to not exceed parent fill
                childBar.Progress = Math.Min(childProgress, parentProgress);
                // Align position with parent start
                childBar.Position = (Position.X, Position.Y);
            }
            else // FromEnd
            {
                // Compute available space to the right of the parent's current fill
                float availableSpace = 1.0f - parentProgress;
                float clampedChild = Math.Min(childProgress, availableSpace);
                childBar.Progress = clampedChild;

                // Force child to start at the end of the parent's current fill
                // So its left edge equals parent's start + parent's filled cells
                childBar.Position = (Position.X + fillSize, Position.Y);
            }
        }
        else
        {
            // Vertical bars (future): mirror logic using Y/height if needed
            if (offsetType == ChildOffsetType.FromStart)
            {
                childBar.Progress = Math.Min(childProgress, parentProgress);
                childBar.Position = (Position.X, Position.Y);
            }
            else
            {
                float availableSpace = 1.0f - parentProgress;
                childBar.Progress = Math.Min(childProgress, availableSpace);
                // For vertical, FromEnd would start from bottom/top based on VerticalAlignment; left as-is for now
            }
        }
    }
}