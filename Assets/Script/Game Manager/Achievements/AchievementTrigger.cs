using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementTrigger : MonoBehaviour
{
    [SerializeField] private string achievementID;
    [SerializeField] private bool isDie = false;
    
    private bool hasTriggered = false; // 狀態鎖：防止玩家在等待死亡的 3 秒內重複觸發

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 如果已經觸發過，直接攔截
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true; // 立刻上鎖    

            // 1. 無論死不死，先解鎖成就
            AchievementManager.Instance.Unlock(achievementID);
            
            // 2. 判斷是否需要執行延遲死亡
            if (isDie)
            {
                StartCoroutine(DelayedDeath(other.gameObject));
            }
        }
    }

    // 【全新功能】：等待成就播放完畢再死
    private IEnumerator DelayedDeath(GameObject player)
    {
        // 這裡可以加上一些「讓玩家暫時不能動」的邏輯，防止他在這 3 秒內跑走
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false; // 暫時鎖住玩家操作，讓他老老實實看著成就彈出來
            
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero; // 速度踩煞車
        }

        // ⏳ 關鍵：等待 3.2 秒（因為你的 AchievementManager 顯示時間是 3 秒，多給 0.2 秒收尾動畫）
        yield return new WaitForSeconds(3.2f);

        // 時間到了，成就看完了，安心地去吧！
        HealthPoint hp = player.GetComponent<HealthPoint>();
        if (hp != null)
        {
            hp.TakeDamage(100); // 扣血 100 觸發死亡
        }
    }
}