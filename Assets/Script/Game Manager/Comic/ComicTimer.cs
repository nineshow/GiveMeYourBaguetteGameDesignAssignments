using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComicTimer : MonoBehaviour
{
    private Button myButton; // 自動獲取自身的按鈕組件

    [Header("倒計時設定")]
    public float displayDuration = 5f;

    private void Start()
    {
        // 自動抓取自己身上的 Button 組件，完全不需要手動去 Inspector 拉格子！
        myButton = GetComponent<Button>(); 
        
        if(myButton != null)
        {
            myButton.interactable = false;
            Invoke("EnableButton", 0.5f); // 0.5秒剛好對齊過場淡入時間
        }
        
        StartCoroutine(CountdownToNextLevel());
    }

    private void EnableButton()
    {
        if(myButton != null) myButton.interactable = true;
    }

    private IEnumerator CountdownToNextLevel()
    {
        yield return new WaitForSeconds(displayDuration);
        TriggerNextLevel();
    }

    public void TriggerNextLevel()
    {
        // 1. 【第一順位】：第一時間物理鎖死按鈕，防點擊穿透與手速狗
        if (myButton != null)
        {
            myButton.interactable = false; 
        }

        // 2. 拔掉倒計時協程，防止自動時間到重複觸發
        StopAllCoroutines(); 

        // 3. 最後才把控制權交給 LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }
}