namespace UniftUI
{
    public interface ILayoutContainer
    {
        void AddChild(UIElement child);
        void RemoveChild(UIElement child);
        void ReplaceChild(UIElement oldChild, UIElement newChild);
        System.Collections.Generic.IEnumerable<UIElement> GetChildren();
    }
}
