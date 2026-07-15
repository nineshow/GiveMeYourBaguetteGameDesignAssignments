using UnityEngine;
using System.Collections;

public class ComicTimer : MonoBehaviour
{
    [Header("Count Down Setting")]
    [Tooltip("display duration")]
    public float displayDuration = 5f;
    
    private void Start()
    {
        // 開始倒計時
        StartCoroutine(CountdownToNextLevel());
    }

    private IEnumerator CountdownToNextLevel()
    {
        // 等待指定的秒數
        yield return new WaitForSeconds(displayDuration);

        // 時間到，調用你原本寫好的 LoadNextLevel 方法
        TriggerNextLevel();
    }
    public void TriggerNextLevel()
    {
        // 停止協程，防止手動跳過後，時間到了又觸發一次載入
        StopAllCoroutines(); 

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
        else
        {
            Debug.LogError("ComicTimer: 找不到 LevelManager.instance！請確保場景中有 LevelManager 物件。");
        }
    }
}