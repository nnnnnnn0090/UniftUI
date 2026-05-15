namespace UniftUI
{
    /// <summary>
    /// SwiftUI の <c>ScrollIndicators</c>（<c>.hidden</c> / <c>.visible</c> / <c>.automatic</c>）に相当します。
    /// </summary>
    public enum ScrollIndicatorVisibility
    {
        Hidden,
        Visible,
        Automatic
    }

    /// <summary>
    /// SwiftUI の軸指定（<c>axes: .vertical</c> など）に相当します。
    /// <c>Vertical | Horizontal</c> で両軸です。
    /// </summary>
    [System.Flags]
    public enum UniftUIScrollAxis
    {
        Vertical = 1,
        Horizontal = 2,
    }
}
