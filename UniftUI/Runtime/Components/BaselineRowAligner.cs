using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UniftUI
{
    /// <summary>
    /// SwiftUI の <c>HStack(alignment: .firstTextBaseline)</c> / <c>.lastTextBaseline</c> に近い、
    /// 行内テキストのベースラインを揃える後処理（TMP のメッシュ情報を使用）。
    /// </summary>
    internal static class BaselineRowAligner
    {
        internal static void AlignIfNeeded(GameObject rowRoot, HStackAlignment alignment)
        {
            if (alignment != HStackAlignment.FirstTextBaseline && alignment != HStackAlignment.LastTextBaseline)
                return;

            RectTransform rowRt = rowRoot.GetComponent<RectTransform>();
            if (rowRt == null)
                return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rowRt);

            bool lastLine = alignment == HStackAlignment.LastTextBaseline;

            float targetY = float.MinValue;
            foreach (Transform child in rowRt)
            {
                TextMeshProUGUI tmp = child.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp == null)
                    continue;
                if (!TryGetBaselineInRowSpace(rowRt, tmp, lastLine, out float y))
                    continue;
                targetY = Mathf.Max(targetY, y);
            }

            if (targetY <= float.MinValue + 1f)
                return;

            foreach (Transform child in rowRt)
            {
                TextMeshProUGUI tmp = child.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp == null)
                    continue;
                if (!TryGetBaselineInRowSpace(rowRt, tmp, lastLine, out float y))
                    continue;
                float dy = targetY - y;
                if (Mathf.Abs(dy) < 0.01f)
                    continue;
                RectTransform crt = child as RectTransform;
                if (crt != null)
                    crt.anchoredPosition += new Vector2(0f, dy);
            }

            Canvas.ForceUpdateCanvases();
        }

        private static bool TryGetBaselineInRowSpace(RectTransform rowRt, TextMeshProUGUI tmp, bool lastLine, out float yInRow)
        {
            yInRow = 0f;
            tmp.ForceMeshUpdate(true);
            if (tmp.textInfo == null || tmp.textInfo.lineCount == 0)
                return false;

            int idx = lastLine ? tmp.textInfo.lineCount - 1 : 0;
            var line = tmp.textInfo.lineInfo[idx];

            // TMP のバージョンにより LineInfo に origin が無い。行の水平中心は lineExtents から取得する。
            Extents ext = line.lineExtents;
            float cx = (ext.min.x + ext.max.x) * 0.5f;
            if (float.IsNaN(cx) || Mathf.Abs(ext.min.x) > 30000f)
                cx = tmp.rectTransform.rect.center.x;

            Vector3 localBaseline = new Vector3(cx, line.baseline, 0f);
            Vector3 world = tmp.rectTransform.TransformPoint(localBaseline);
            Vector3 rowLocal = rowRt.InverseTransformPoint(world);
            yInRow = rowLocal.y;
            return true;
        }
    }
}
