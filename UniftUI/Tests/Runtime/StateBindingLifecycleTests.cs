using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UniftUI.Tests
{
    public class StateBindingLifecycleTests
    {
        [Test]
        public void BindingRegistry_ReRegisterPropertyRemovesOldStateIndex()
        {
            var registry = new BindingRegistry();
            var first = new State<int>(0);
            var second = new State<int>(0);
            int firstUpdates = 0;
            int secondUpdates = 0;

            registry.Register("value", first, () => firstUpdates++, BindingKind.Visual);
            registry.Register("value", second, () => secondUpdates++, BindingKind.Visual);

            Assert.That(registry.ObservedStates.Contains(first), Is.False);
            Assert.That(registry.ObservedStates.Contains(second), Is.True);

            registry.ApplyForState(first);
            registry.ApplyForState(second);

            Assert.That(firstUpdates, Is.EqualTo(0));
            Assert.That(secondUpdates, Is.EqualTo(1));
        }

        [Test]
        public void StateSubscriptionGroup_DeduplicatesAndClearsSubscriptions()
        {
            var state = new State<int>(0);
            var group = new StateSubscriptionGroup();
            int changes = 0;

            Assert.That(group.Subscribe(state, () => changes++), Is.True);
            Assert.That(group.Subscribe(state, () => changes++), Is.False);
            Assert.That(group.Count, Is.EqualTo(1));

            state.Value = 1;
            Assert.That(changes, Is.EqualTo(1));

            group.Clear();
            state.Value = 2;
            Assert.That(changes, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ElementAnimationHost_DeduplicatesStateNotificationsAndUnsubscribes()
        {
            var parent = new GameObject("AnimationHostParent");
            var state = new State<int>(0);
            var element = new BindingProbeElement(state);
            GameObject built = null;

            try
            {
                built = element.Build(parent.transform);
                yield return null;

                Assert.That(element.UpdateCount, Is.EqualTo(2));

                element.ResetUpdateCount();
                state.Value = 1;
                yield return null;
                Assert.That(element.UpdateCount, Is.EqualTo(2));

                built.SetActive(false);
                yield return null;
                state.Value = 2;
                yield return null;
                Assert.That(element.UpdateCount, Is.EqualTo(2));

                built.SetActive(true);
                yield return null;
                int afterEnableCount = element.UpdateCount;
                Assert.That(afterEnableCount, Is.InRange(2, 4));
                if (afterEnableCount > 2)
                    Assert.That(element.LastValue, Is.EqualTo(2));

                state.Value = 3;
                yield return null;
                int afterEnabledChangeCount = afterEnableCount + 2;
                Assert.That(element.UpdateCount, Is.EqualTo(afterEnabledChangeCount));
                Assert.That(element.LastValue, Is.EqualTo(3));

                UnityEngine.Object.DestroyImmediate(built);
                built = null;
                state.Value = 4;
                yield return null;
                Assert.That(element.UpdateCount, Is.EqualTo(afterEnabledChangeCount));
            }
            finally
            {
                if (built != null)
                    UnityEngine.Object.DestroyImmediate(built);
                UnityEngine.Object.DestroyImmediate(parent);
            }
        }

        [UnityTest]
        public IEnumerator ContentRebuildObserver_DeduplicatesAndUnsubscribes()
        {
            var host = new GameObject("ContentRebuildObserverHost");
            var state = new State<int>(0);
            int rebuilds = 0;

            try
            {
                var observer = host.AddComponent<ContentRebuildObserver>();
                observer.Initialize(new State[] { state, state }, () => rebuilds++);

                state.Value = 1;
                yield return null;
                Assert.That(rebuilds, Is.EqualTo(1));

                host.SetActive(false);
                yield return null;
                state.Value = 2;
                yield return null;
                Assert.That(rebuilds, Is.EqualTo(1));

                host.SetActive(true);
                yield return null;
                state.Value = 3;
                yield return null;
                Assert.That(rebuilds, Is.EqualTo(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [UnityTest]
        public IEnumerator ContentRebuildObserver_DestroyUnsubscribes()
        {
            var host = new GameObject("ContentRebuildObserverHost");
            var state = new State<int>(0);
            int rebuilds = 0;

            var observer = host.AddComponent<ContentRebuildObserver>();
            observer.Initialize(new State[] { state }, () => rebuilds++);

            state.Value = 1;
            yield return null;
            Assert.That(rebuilds, Is.EqualTo(1));

            UnityEngine.Object.DestroyImmediate(host);
            state.Value = 2;
            yield return null;
            Assert.That(rebuilds, Is.EqualTo(1));
        }

        [Test]
        public void UIContextPush_RestoresNestedScopes()
        {
            var outer = new ScopeProbeElement();
            var inner = new ScopeProbeElement();
            UIContext.Current = outer;

            try
            {
                using (UIContext.Push(inner))
                {
                    Assert.That(UIContext.Current, Is.SameAs(inner));
                }

                Assert.That(UIContext.Current, Is.SameAs(outer));
            }
            finally
            {
                UIContext.Current = null;
            }
        }

        [Test]
        public void MaterializeContent_RestoresUIContextWhenContentThrows()
        {
            var previous = new ScopeProbeElement();
            var owner = new ScopeProbeElement();
            UIContext.Current = previous;

            try
            {
                Assert.Throws<InvalidOperationException>(() =>
                    owner.Materialize(() => { throw new InvalidOperationException("boom"); }));

                Assert.That(UIContext.Current, Is.SameAs(previous));
            }
            finally
            {
                UIContext.Current = null;
            }
        }

        [UnityTest]
        public IEnumerator ContentRebuildObserver_RepeatedRebuildsDoNotDuplicateChildren()
        {
            Canvas canvas = CreateCanvas();
            var count = new State<int>(1);

            try
            {
                UIElements.VStack(() =>
                {
                    for (int i = 0; i < count.Value; i++)
                        UIElements.Text("row " + i).frame(width: 80, height: 20);
                }, new State[] { count }, spacing: 2f)
                .Build(canvas);

                yield return null;

                count.Value = 2;
                yield return null;
                count.Value = 3;
                yield return null;
                count.Value = 1;
                yield return null;

                Assert.That(canvas.GetComponentsInChildren<TMPro.TMP_Text>(true).Length, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(canvas.gameObject);
            }
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("TestCanvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);
            return canvas;
        }

        private sealed class ScopeProbeElement : UIElement, ILayoutContainer
        {
            private readonly List<UIElement> children = new List<UIElement>();

            public void Materialize(Action content)
            {
                MaterializeContent(content, children);
            }

            public void AddChild(UIElement child)
            {
                if (child != null)
                    children.Add(child);
            }

            public void RemoveChild(UIElement child)
            {
                if (child != null)
                    children.Remove(child);
            }

            public void ReplaceChild(UIElement oldChild, UIElement newChild)
            {
                int index = children.IndexOf(oldChild);
                if (index >= 0)
                    children[index] = newChild;
            }

            public IEnumerable<UIElement> GetChildren()
            {
                return children;
            }
        }

        private sealed class BindingProbeElement : UIElement
        {
            public BindingProbeElement(State<int> state)
            {
                AddPropertyBinding(state, () => ApplyValue(state), "first", BindingKind.Visual);
                AddPropertyBinding(state, () => ApplyValue(state), "second", BindingKind.Visual);
            }

            public int UpdateCount { get; private set; }
            public int LastValue { get; private set; }

            public void ResetUpdateCount()
            {
                UpdateCount = 0;
            }

            private void ApplyValue(State<int> state)
            {
                UpdateCount++;
                LastValue = state.Value;
            }

            public override GameObject Build(Transform parent)
            {
                GameObject root = CreateElementRoot("BindingProbe", parent);
                SetupDynamicEffects(root);
                return root;
            }
        }
    }
}
