using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Button with a text label or custom child content.</summary>
    public class ButtonElement : UIElement, ILayoutContainer, IControlHitTargetSource
    {
        private string label;
        private UIElement customContent;
        private Action onClick;
        private Color textColor = new Color(0f, 0.4784314f, 1f, 1f);
        private bool hasExplicitTextColor;
        private TMP_FontAsset fontAsset = null;
        private float fontSize = 14f;
        private bool isBold;
        private bool isItalic;
        private bool isUnderlined;
        private bool isStrikethrough;
        private bool hasCustomContent = false;
        private List<UIElement> children = new List<UIElement>();
        private Button builtButton;
        private Image builtBackgroundImage;
        private IButtonStyle buttonStyle = ButtonStyles.Automatic();
        private GameObject builtButtonObject;
        private GameObject hitAreaObject;
        private bool isPressed;
        private bool isHovered;

        /// <summary>Creates a button that displays the given text label.</summary>
        public ButtonElement(string label, Action onClick)
        {
            this.backgroundColor = Color.clear;
            this.label = label;
            this.onClick = onClick;
            this.hasCustomContent = false;
            UIContext.Add(this);
        }

        /// <summary>Creates a button whose appearance is defined by the given child element.</summary>
        public ButtonElement(UIElement content, Action onClick)
        {
            this.backgroundColor = Color.clear;
            this.customContent = content;
            this.onClick = onClick;
            this.hasCustomContent = true;

            if (content != null)
            {
                AddChild(content);
            }

            UIContext.Add(this);
        }

        public void AddChild(UIElement child)
        {
            if (child != null)
            {
                children.Add(child);
            }
        }

        public void RemoveChild(UIElement child)
        {
            if (child != null)
            {
                children.Remove(child);
            }
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (oldChild == null || newChild == null) return;
            int index = children.IndexOf(oldChild);
            if (index != -1)
            {
                children[index] = newChild;
            }
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        bool IControlHitTargetSource.TryGetControlHitTarget(out ControlHitTarget target)
        {
            if (onClick == null)
            {
                target = default(ControlHitTarget);
                return false;
            }

            target = CreateHitTarget();
            return true;
        }

        private void InvokeButtonAction()
        {
            if (!IsInputAllowed())
                return;

            try
            {
                onClick?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniftUI] Button onClick error: {e.Message}");
            }
        }

        public ButtonElement SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            if (builtBackgroundImage != null)
            {
                builtBackgroundImage.color = color;
                ConfigureButtonTransition();
            }
            return this;
        }

        public ButtonElement SetTextColor(Color color)
        {
            hasExplicitTextColor = true;
            textColor = color;
            if (hasCustomContent)
                foreach (var child in children)
                    child.ForegroundColor(color);
            RebuildStyleBody();

            return this;
        }

        public ButtonElement SetButtonStyle(IButtonStyle style)
        {
            buttonStyle = style ?? ButtonStyles.Automatic();
            RebuildStyleBody();
            return this;
        }

        public ButtonElement SetFont(TMP_FontAsset font)
        {
            fontAsset = font;
            if (hasCustomContent)
            {
                foreach (var child in children)
                    child.Font(font);
            }
            RebuildStyleBody();

            return this;
        }

        public ButtonElement SetFontSize(float size)
        {
            fontSize = Mathf.Max(1f, size);
            if (hasCustomContent)
            {
                foreach (var child in children)
                    child.FontSize(fontSize);
            }
            RebuildStyleBody();

            return this;
        }

        public ButtonElement SetBold(bool bold)
        {
            isBold = bold;
            if (hasCustomContent)
                foreach (var child in children)
                    child.Bold();
            RebuildStyleBody();
            return this;
        }

        public ButtonElement SetItalic(bool italic)
        {
            isItalic = italic;
            if (hasCustomContent)
                foreach (var child in children)
                    child.Italic();
            RebuildStyleBody();
            return this;
        }

        public ButtonElement SetUnderline(bool underline)
        {
            isUnderlined = underline;
            if (hasCustomContent)
                foreach (var child in children)
                    child.Underline();
            RebuildStyleBody();
            return this;
        }

        public ButtonElement SetStrikethrough(bool strikethrough)
        {
            isStrikethrough = strikethrough;
            if (hasCustomContent)
                foreach (var child in children)
                    child.Strikethrough();
            RebuildStyleBody();
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject buttonObj = CreateElementRoot("Button", parent);
            builtButtonObject = buttonObj;

            Image image = AddImage(buttonObj, backgroundColor);
            builtBackgroundImage = image;

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;
            builtButton = button;
            ConfigureButtonTransition();

            var buttonLayout = buttonObj.AddComponent<UniftUISingleChildLayoutGroup>();
            buttonLayout.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);

            LayoutElementUtility.Configure(buttonObj, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);
            ConfigureInteractionTracker(buttonObj);
            RebuildStyleBody();

            button.onClick.AddListener(InvokeButtonAction);

            ApplyAllEffects(buttonObj, image);

            return buttonObj;
        }

        private void ConfigureButtonTransition()
        {
            if (builtButton == null)
                return;

            ConfigureSelectableColors(builtButton, backgroundColor);

            if (builtBackgroundImage != null)
                builtBackgroundImage.color = backgroundColor;
        }

        private void ConfigureInteractionTracker(GameObject buttonObj)
        {
            ControlInteractionTracker tracker = buttonObj.GetComponent<ControlInteractionTracker>();
            if (tracker == null)
                tracker = buttonObj.AddComponent<ControlInteractionTracker>();
            tracker.Initialize(CreateHitTarget());
        }

        private void EnsureHitArea(GameObject buttonObj)
        {
            EnsureControlHitArea(buttonObj, ref hitAreaObject, "ButtonHitArea", CreateHitTarget());
        }

        private ControlHitTarget CreateHitTarget()
        {
            return new ControlHitTarget(InvokeButtonAction, SetPressed, SetHovered, IsInputAllowed);
        }

        private void SetPressed(bool pressed)
        {
            bool next = pressed && IsInputAllowed();
            if (isPressed == next)
                return;

            isPressed = next;
            RebuildStyleBody();
        }

        private void SetHovered(bool hovered)
        {
            bool next = hovered && IsInputAllowed();
            if (isHovered == next)
                return;

            isHovered = next;
            RebuildStyleBody();
        }

        private void RebuildStyleBody()
        {
            if (builtButtonObject == null)
                return;

            ClearStyleBody();

            UIElement labelElement = CreateLabelElement();
            UIElement body = BuildStyleElement(labelElement) ?? labelElement;

            if ((preferredWidth >= 0f || infiniteWidth) && ChildMayFillWidth(body))
                body.WithInfiniteWidth();
            if ((preferredHeight >= 0f || infiniteHeight) && ChildMayFillHeight(body))
                body.WithInfiniteHeight();

            ApplyInheritedFont(body);
            body.Build(builtButtonObject.transform);
            EnsureHitArea(builtButtonObject);

            LayoutCore.ForceRebuildLayout(builtButtonObject);
        }

        private void ClearStyleBody()
        {
            var oldChildren = new List<GameObject>();
            foreach (Transform child in builtButtonObject.transform)
            {
                if (child != null)
                {
                    if (child.gameObject == hitAreaObject)
                        continue;
                    oldChildren.Add(child.gameObject);
                }
            }

            foreach (GameObject child in oldChildren)
            {
                child.SetActive(false);
                child.transform.SetParent(null, false);
                DestroyGameObject(child);
            }
        }

        private UIElement BuildStyleElement(UIElement labelElement)
        {
            ILayoutContainer parentContext = UIContext.Current;
            try
            {
                UIContext.Current = null;
                var configuration = new ButtonStyleConfiguration(labelElement, isPressed, isHovered);
                return (buttonStyle ?? ButtonStyles.Automatic()).MakeBody(configuration);
            }
            finally
            {
                UIContext.Current = parentContext;
            }
        }

        private UIElement CreateLabelElement()
        {
            if (hasCustomContent)
            {
                if (hasExplicitTextColor && customContent != null)
                    customContent.ForegroundColor(textColor);
                return customContent;
            }

            ILayoutContainer parentContext = UIContext.Current;
            try
            {
                UIContext.Current = null;
                TextElement text = new TextElement(label);
                text.SetTextColor(textColor);
                text.SetFontSize(fontSize);
                if (fontAsset != null)
                    text.SetFont(fontAsset);
                if (isBold)
                    text.SetBold(true);
                if (isItalic)
                    text.SetItalic(true);
                if (isUnderlined)
                    text.SetUnderline(true);
                if (isStrikethrough)
                    text.SetStrikethrough(true);
                return text;
            }
            finally
            {
                UIContext.Current = parentContext;
            }
        }

    }
}
