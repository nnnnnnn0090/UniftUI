using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UniftUI.Internal;

namespace UniftUI
{
    public abstract partial class UIElement
    {
        protected void MaterializeContent(Action content, IList<UIElement> children)
        {
            if (children == null)
                return;

            children.Clear();
            if (content == null)
                return;

            using (UIContext.Push(this as ILayoutContainer))
                content.Invoke();
        }

        protected void BuildContentChildren(IList<UIElement> children, Transform parent,
            Action<UIElement> configureChild = null)
        {
            if (children == null || parent == null)
                return;

            foreach (UIElement child in children)
            {
                if (child == null)
                    continue;

                ApplyInheritedFont(child);
                configureChild?.Invoke(child);
                child.Build(parent);
            }
        }

        protected void SetupContentRebuildObserver(
            State[] states,
            GameObject observerObject,
            Transform contentParent,
            IList<UIElement> children,
            Action content,
            string ownerName,
            Action<UIElement> configureChild = null,
            Action afterRebuild = null)
        {
            if (states == null || states.Length == 0 || observerObject == null || contentParent == null)
                return;

            ContentRebuildObserver observer = observerObject.AddComponent<ContentRebuildObserver>();
            observer.Initialize(states, () => RebuildContent(
                observerObject,
                contentParent,
                children,
                content,
                ownerName,
                configureChild,
                afterRebuild));
        }

        protected void RebuildContent(
            GameObject observerObject,
            Transform contentParent,
            IList<UIElement> children,
            Action content,
            string ownerName,
            Action<UIElement> configureChild = null,
            Action afterRebuild = null)
        {
            if (observerObject == null || !observerObject || contentParent == null || !contentParent)
            {
                Debug.LogWarning($"[UniftUI] {ownerName} rebuild skipped: container was destroyed.");
                return;
            }

            try
            {
                ClearBuiltChildren(contentParent);
                MaterializeContent(content, children);
                BuildContentChildren(children, contentParent, configureChild);

                LayoutCore.ForceRebuildLayout(contentParent.gameObject);
                if (observerObject != contentParent.gameObject)
                    LayoutCore.ForceRebuildLayout(observerObject);

                Canvas.ForceUpdateCanvases();
                afterRebuild?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniftUI] {ownerName} rebuild error: {e.Message}\n{e.StackTrace}");
            }
        }

        protected void ClearBuiltChildren(Transform parent)
        {
            if (parent == null)
                return;

            List<GameObject> oldChildren = new List<GameObject>();
            foreach (Transform child in parent)
                if (child != null && child.gameObject != null)
                    oldChildren.Add(child.gameObject);

            foreach (GameObject child in oldChildren)
                DestroyGameObject(child);
        }

    }
}
