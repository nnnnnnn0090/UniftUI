using UnityEngine;
using UnityEngine.UI;

namespace UniftUI
{
    /// <summary>
    /// SwiftUI の <c>Spacer(minLength:)</c>。親が <see cref="VerticalLayoutGroup"/> のときは縦方向のみ、
    /// <see cref="HorizontalLayoutGroup"/> のときは横方向のみ伸縮し、それ以外（ZStack 等）は両軸で伸びます。
    /// コンストラクタ引数は主軸の<strong>最小</strong>長だけで、スタックに余白があれば <c>flexibleWidth</c>/<c>flexibleHeight</c> により<strong>それより大きくなります</strong>。
    /// ピクセル固定の隙間は SwiftUI と同様 <c>Spacer().Frame(width:)</c> / <c>Frame(height:)</c> で指定してください。
    /// </summary>
    public class SpacerElement : UIElement
    {
        /// <summary>主軸に沿った最小長さ（<see cref="UniftView.Spacer(float)"/> の <c>minLength</c>。VStack では高さ、HStack では幅）。</summary>
        private readonly float minAlongAxis;

        public SpacerElement(float minAlongAxis = 0)
        {
            this.minAlongAxis = minAlongAxis;
            UIContext.Add(this);
        }

        public override GameObject Build(Transform parent)
        {
            GameObject spacerObj = new GameObject("Spacer");
            spacerObj.transform.SetParent(parent, false);

            Image background = null;
            if (backgroundColor != Color.clear)
            {
                background = spacerObj.AddComponent<Image>();
                background.color = backgroundColor;
            }

            LayoutElement layoutElement = spacerObj.AddComponent<LayoutElement>();

            // 親がレイアウトグループを付けないラッパーのときがあるため、祖先を辿って主軸を解決（SwiftUI の Spacer は親スタックの主軸にのみ効く）。
            bool verticalMain = false;
            bool horizontalMain = false;
            Transform t = parent;
            while (t != null)
            {
                bool hasH = t.GetComponent<HorizontalLayoutGroup>() != null;
                bool hasV = t.GetComponent<VerticalLayoutGroup>() != null;
                // 同一ノードに両方ある場合は横並び（行）を優先（稀な構成）
                if (hasH)
                {
                    horizontalMain = true;
                    break;
                }

                if (hasV)
                {
                    verticalMain = true;
                    break;
                }

                t = t.parent;
            }

            if (horizontalMain && !verticalMain)
            {
                infiniteWidth = true;
                infiniteHeight = false;
                layoutElement.minWidth = minAlongAxis;
                layoutElement.flexibleWidth = 10000f;
                layoutElement.minHeight = 0f;
                layoutElement.flexibleHeight = 0f;
            }
            else if (verticalMain && !horizontalMain)
            {
                infiniteWidth = false;
                infiniteHeight = true;
                layoutElement.minWidth = 0f;
                layoutElement.flexibleWidth = 0f;
                layoutElement.minHeight = minAlongAxis;
                layoutElement.flexibleHeight = 10000f;
            }
            else
            {
                infiniteWidth = true;
                infiniteHeight = true;
                layoutElement.minWidth = minAlongAxis;
                layoutElement.minHeight = minAlongAxis;
                layoutElement.flexibleWidth = 10000f;
                layoutElement.flexibleHeight = 10000f;
            }

            if (preferredWidth > 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
                layoutElement.flexibleWidth = 0;
            }

            if (preferredHeight > 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
                layoutElement.flexibleHeight = 0;
            }

            ApplyAllEffects(spacerObj, background);

            return spacerObj;
        }
    }
}
