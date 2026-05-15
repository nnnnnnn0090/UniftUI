using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UniftUI
{
    public abstract class UniftView : MonoBehaviour
    {
        protected UIElement VStack(Action content)
        {
            return VStack(content, null, 8f, VStackAlignment.Center);
        }

        protected UIElement VStack(Action content, State[] states = null)
        {
            return VStack(content, states, 8f, VStackAlignment.Center);
        }

        protected UIElement VStack(Action content, State[] states = null, float spacing = 8f, 
            VStackAlignment alignment = VStackAlignment.Center)
        {
            VStackElement stack = new VStackElement(content, states, spacing, alignment);
            if (UIContext.Current != null)
            {
                UIContext.Current.AddChild(stack);
            }
            return stack;
        }

        protected UIElement HStack(Action content)
        {
            return HStack(content, null, 8f, HStackAlignment.Center);
        }

        protected UIElement HStack(Action content, float spacing = 8f)
        {
            return HStack(content, null, spacing, HStackAlignment.Center);
        }

        protected UIElement HStack(Action content, State[] states = null)
        {
            return HStack(content, states, 8f, HStackAlignment.Center);
        }

        protected UIElement HStack(Action content, State[] states = null, float spacing = 8f, 
            HStackAlignment alignment = HStackAlignment.Center)
        {
            HStackElement stack = new HStackElement(content, states, spacing, alignment);
            if (UIContext.Current != null)
            {
                UIContext.Current.AddChild(stack);
            }
            return stack;
        }

        protected GridElement Grid(Action content, State[] states = null, float horizontalSpacing = 8f,
            float verticalSpacing = 8f, HStackAlignment rowAlignment = HStackAlignment.Center)
        {
            GridElement grid = new GridElement(content, states, horizontalSpacing, verticalSpacing, rowAlignment);
            if (UIContext.Current != null)
                UIContext.Current.AddChild(grid);
            return grid;
        }

        protected GridRowElement GridRow(Action content)
        {
            if (UIContext.Current is GridElement grid)
                return new GridRowElement(grid, content);
            Debug.LogWarning("GridRow は Grid のコンテンツ内でのみ使用してください。");
            return new GridRowElement(null, content);
        }

        protected UIElement Text(string text)
        {
            return new TextElement(text);
        }

        protected UIElement Text(string text, State[] dependencyStates)
        {
            return new TextElement(text, dependencyStates);
        }

        protected UIElement Text(Func<string> content, State[] dependencyStates = null)
        {
            return new TextElement(content, dependencyStates);
        }

        protected UIElement Text(State<string> text, State[] additionalStates = null)
        {
            return new TextElement(text, additionalStates);
        }

        protected UIElement Button(string label, Action onClick)
        {
            return new ButtonElement(label, onClick);
        }
        
        protected UIElement Button(string label, Action onClick, Color backgroundColor, Color textColor)
        {
            ButtonElement button = new ButtonElement(label, onClick);
            button.SetBackgroundColor(backgroundColor);
            button.SetTextColor(textColor);
            return button;
        }
        
        protected UIElement Button(UIElement content, Action onClick)
        {
            UIContext.Current?.RemoveChild(content);
            return new ButtonElement(content, onClick);
        }

        protected ImageElement Image(Sprite sprite)
        {
            return new ImageElement(sprite);
        }

        /// <summary>
        /// SwiftUI の <c>Spacer(minLength:)</c> に相当する伸縮スペース。引数は主軸方向の<b>最小</b>長さ（HStack では最小幅、VStack では最小高さ）。
        /// 親スタックに余白があるとレイアウト上<strong>それ以上に広がります</strong>（固定ピクセルの隙間にはなりません）。
        /// SwiftUI と同様、ピクセル固定の隙間は <c>Spacer().Frame(width: …)</c> / <c>Frame(height: …)</c>（Swift の <c>.frame(width:height:)</c> に相当）で指定してください。
        /// </summary>
        /// <param name="minLength">主軸に沿った最小長（Swift の <c>minLength</c>）。</param>
        protected UIElement Spacer(float minLength = 0)
        {
            return new SpacerElement(minLength);
        }

        protected UIElement TabView(Action content, State<int> selectedIndex = null)
        {
            TabView tabView = new TabView(content, selectedIndex);
            if (UIContext.Current != null)
            {
                UIContext.Current.AddChild(tabView);
            }
            return tabView;
        }

        protected UIElement Tab(Action titleContent, Action content)
        {
            return new TabItem(titleContent, content);
        }

        protected UIElement Tab(string title, Action content)
        {
            return new TabItem(title, content);
        }

        protected UIElement Slider(State<int> value, float minValue, float maxValue)
        {
            return new SliderElement(value, minValue, maxValue);
        }

        protected ScrollViewElement ScrollView(Action content, State[] states = null, bool horizontal = false, bool vertical = true)
        {
            ScrollViewElement scrollView = new ScrollViewElement(content, states, horizontal, vertical);
            if (UIContext.Current != null)
            {
                UIContext.Current.AddChild(scrollView);
            }
            return scrollView;
        }

        protected UIElement Toggle(State<bool> isOn, string label, Action<bool> onValueChanged = null)
        {
            return new ToggleElement(isOn, label, onValueChanged);
        }

        protected UIElement ZStack(Action content)
        {
            return ZStack(content, null, ZStackAlignment.Center);
        }

        protected UIElement ZStack(Action content, State[] states = null)
        {
            return ZStack(content, states, ZStackAlignment.Center);
        }

        protected UIElement ZStack(Action content, State[] states = null, ZStackAlignment alignment = ZStackAlignment.Center)
        {
            ZStackElement stack = new ZStackElement(content, states, alignment);
            if (UIContext.Current != null)
            {
                UIContext.Current.AddChild(stack);
            }
            return stack;
        }

        protected TextFieldElement TextField(State<string> text, string placeholder, Action<string> onTextChanged = null)
        {
            return new TextFieldElement(text, placeholder, onTextChanged);
        }

        /// <summary>
        /// SwiftUI の <c>ForEach(0...9, id: \.self)</c>（閉区間）に相当。<paramref name="fromInclusive"/> と <paramref name="toInclusive"/> は両端を含みます。
        /// 半開区間 <c>0..&lt;10</c> は <see cref="ForEach(Range, Action{int})"/> に <c>0..10</c> を渡してください。
        /// </summary>
        protected void ForEach(int fromInclusive, int toInclusive, Action<int> content)
        {
            if (toInclusive < fromInclusive) return;
            foreach (int id in Enumerable.Range(fromInclusive, toInclusive - fromInclusive + 1))
                content(id);
        }

        /// <summary>
        /// SwiftUI の <c>ForEach(0..&lt;10, id: \.self)</c> に相当。C# の範囲 <c>0..10</c> は終端を含みません（<c>..</c> の右側は排他的）。
        /// </summary>
        protected void ForEach(Range range, Action<int> content)
        {
            if (content == null) return;
            if (range.Start.IsFromEnd || range.End.IsFromEnd)
            {
                Debug.LogWarning("ForEach(Range): ^（末尾から）を使う範囲は未対応です。");
                return;
            }

            int start = range.Start.Value;
            int end = range.End.Value;
            for (int i = start; i < end; i++)
                content(i);
        }

        /// <summary>
        /// SwiftUI の <c>ForEach(items, id: ...)</c> に相当。列の各要素でビューを組み立てます。
        /// </summary>
        protected void ForEach<T>(IEnumerable<T> data, Action<T> content)
        {
            foreach (T item in data)
                content(item);
        }

        // ─── Animation helpers ──────────────────────────────────────────────

        /// <summary>
        /// SwiftUI の <c>withAnimation(.spring()) { ... }</c> に相当。
        /// クロージャ内の State 変化をすべて指定したアニメーションで補間する。
        /// </summary>
        protected void withAnimation(Animation animation, Action changes)
        {
            AnimationContext.Push(animation);
            try { changes(); }
            finally { AnimationContext.Pop(); }
        }

        /// <summary>
        /// デフォルトアニメーション（easeInOut 0.35 秒）で State 変化をアニメートする。
        /// </summary>
        protected void withAnimation(Action changes) => withAnimation(Animation.Default, changes);
    }
}
