namespace UniftUI
{
    /// <summary>Scroll bar visibility for scroll views.</summary>
    public enum ScrollIndicatorVisibility
    {
        Hidden,
        Visible,
        Automatic
    }

    /// <summary>Scroll axis flags for indicator and position APIs.</summary>
    [System.Flags]
    public enum UniftUIScrollAxis
    {
        Vertical = 1,
        Horizontal = 2,
    }
}
