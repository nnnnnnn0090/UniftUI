using UnityEngine;
using UnityEngine.UI;

namespace UniftUI
{
    /// <summary>
    /// SwiftUI の <c>Grid</c> と同様、同一列インデックスのセル幅を全行で最大コンテンツ幅に揃えます。
    /// </summary>
    internal static class GridColumnSynchronizer
    {
        internal static void Apply(RectTransform gridContainerRt)
        {
            if (gridContainerRt == null)
                return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridContainerRt);

            int maxCols = 0;
            foreach (Transform rowT in gridContainerRt)
            {
                if (rowT.name != "GridRow")
                    continue;
                maxCols = Mathf.Max(maxCols, rowT.childCount);
            }

            if (maxCols == 0)
                return;

            float[] colMaxPreferred = new float[maxCols];

            foreach (Transform rowT in gridContainerRt)
            {
                if (rowT.name != "GridRow")
                    continue;
                for (int j = 0; j < rowT.childCount && j < maxCols; j++)
                {
                    RectTransform cellRt = rowT.GetChild(j) as RectTransform;
                    if (cellRt == null)
                        continue;
                    float pw = LayoutUtility.GetPreferredWidth(cellRt);
                    colMaxPreferred[j] = Mathf.Max(colMaxPreferred[j], pw);
                }
            }

            foreach (Transform rowT in gridContainerRt)
            {
                if (rowT.name != "GridRow")
                    continue;
                for (int j = 0; j < rowT.childCount && j < maxCols; j++)
                {
                    RectTransform cellRt = rowT.GetChild(j) as RectTransform;
                    if (cellRt == null)
                        continue;
                    float w = colMaxPreferred[j];
                    LayoutElement le = cellRt.GetComponent<LayoutElement>();
                    if (le == null)
                        le = cellRt.gameObject.AddComponent<LayoutElement>();
                    le.minWidth = w;
                    le.preferredWidth = w;
                    le.flexibleWidth = 0;
                }
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridContainerRt);
        }
    }
}
