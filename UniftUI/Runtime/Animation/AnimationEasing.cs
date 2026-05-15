using UnityEngine;

namespace UniftUI
{
    /// <summary>
    /// アニメーションの緩急（イージング）を表す列挙型
    /// </summary>
    public enum AnimationEasing
    {
        /// <summary>一定速度 (線形)</summary>
        Linear,
        /// <summary>開始時にゆっくり、終了時に早く</summary>
        EaseIn,
        /// <summary>開始時に早く、終了時にゆっくり</summary>
        EaseOut,
        /// <summary>開始と終了がゆっくりで、途中が早い</summary>
        EaseInOut,
        /// <summary>弾むような効果がある出口の緩和</summary>
        EaseOutBounce,
        /// <summary>伸びてから元に戻るようなゴム的効果</summary>
        EaseOutElastic,
        /// <summary>目標を超えて戻る効果</summary>
        EaseOutBack,
        /// <summary>バネのような効果</summary>
        Spring
    }
}
