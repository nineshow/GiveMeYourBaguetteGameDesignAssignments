using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideManager : MonoBehaviour
{
    public static GuideManager Instance;

    [Header("Guide Data")]
    [Tooltip("請把 Hierarchy 裡的 guildPane1, guildPane2 依序拖進來")]
    public GameObject[] guildPanels;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 👈 修正：這裡用 gameObject 銷毀整個重複的物件更安全
        }
    }

    void Start()
    {
        // 遊戲啟動時，預設自動隱藏所有圖案面板，防止編輯器裡沒關掉導致穿幫
        HideGuide();
    }
    
    /// <summary>
    /// 根據編號喚出對應的圖案面板（0 代表第一個面板）
    /// </summary>
    public void ShowGuildPanel(int panelIndex)
    {
        // 1. 先關閉當前所有打開的面板，避免重疊
        HideGuide();

        // 2. 安全防禦：檢查傳進來的編號有沒有超出陣列範圍
        if (panelIndex >= 0 && panelIndex < guildPanels.Length)
        {
            if (guildPanels[panelIndex] != null)
            {
                guildPanels[panelIndex].SetActive(true); // 喚出對應的圖案面板
                Debug.Log($"【Guild】成功喚出面板：{guildPanels[panelIndex].name}");
            }
        }
        else
        {
            Debug.LogWarning($"【Guild】錯誤：找不到編號為 {panelIndex} 的面板！");
        }
    }

    /// <summary>
    /// 隱藏所有 Guild 圖案面板
    /// </summary>
    public void HideGuide()
    {
        if (guildPanels == null) return;

        // 遍歷整個陣列，把裡面所有面板通通關掉
        for (int i = 0; i < guildPanels.Length; i++)
        {
            if (guildPanels[i] != null)
            {
                guildPanels[i].SetActive(false);
            }
        }
    }
}