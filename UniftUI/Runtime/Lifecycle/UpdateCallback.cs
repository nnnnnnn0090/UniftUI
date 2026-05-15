using UnityEngine;
using System;

namespace UniftUI
{
    public class UpdateCallback : MonoBehaviour
    {
        private Action callback;
        private bool isInitialized = false;
        private bool hasError = false;
        private const int MAX_ERROR_COUNT = 3;
        private int errorCount = 0;
        
        // 追加: 更新頻度の制御
        // private int updateInterval = 1; // 実行する頻度（1=毎フレーム）
        // private int frameCounter = 0;
        private bool updatesEnabled = true;

        public void Initialize(Action callback)
        {
            if (callback == null) return;

            this.callback = callback;
            this.isInitialized = true;
        }

        /// <summary>互換用。更新間隔パラメータは未使用です。</summary>
        public void Initialize(Action callback, int updateInterval = 1)
        {
            Initialize(callback);
        }

        private void Update()
        {
            // 初期化済み、エラーなし、コールバックが有効、有効フラグがtrueの場合のみ実行
            if (isInitialized && !hasError && callback != null && updatesEnabled)
            {
                // // 更新頻度の制御
                // frameCounter++;
                // if (frameCounter < updateInterval) return;
                // frameCounter = 0;
                
                try
                {
                    // 直接コールバックを実行（オーバーヘッド削減）
                    callback.Invoke();
                }
                catch (Exception e)
                {
                    // エラーログをStringBuilderやstring interpolationを使わず最適化
                    Debug.LogError("UpdateCallback: アクション実行中にエラー発生: " + e.Message);
                    errorCount++;
                    if (errorCount >= MAX_ERROR_COUNT)
                    {
                        hasError = true;
                        Debug.LogWarning("UpdateCallback: 更新処理を停止します。エラー回数: " + errorCount);
                        // コールバック参照を解放してメモリリークを防止
                        callback = null;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            callback = null;
        }
        
        // 追加: エラー状態をリセット
        public void ResetErrorState()
        {
            errorCount = 0;
            hasError = false;
        }
        
        // 追加: 実行を一時停止/再開 - コードを修正
        public void SetEnabled(bool enabledState)
        {
            this.updatesEnabled = enabledState;
        }
    }
}
