using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        //  情況 1：如果碰到的物件是小怪
        if (other.CompareTag("Monster"))
        {
            // 優先處理普通怪血量
            HealthPoint monsterHealth = other.GetComponent<HealthPoint>();
            if (monsterHealth != null)
            {
                monsterHealth.TakeDamage(99999, false); // 造成致死傷害
            }
            else
            {
                // 處理 Boss 血量（保底防禦）
                BossHealth bossHealth = other.GetComponent<BossHealth>();
                if (bossHealth != null)
                {
                    bossHealth.TakeDamage(99999, false);
                }
                else
                {
                    // 如果身上沒有任何血量組件，直接物理銷毀，防止效能浪費
                    Destroy(other.gameObject);
                }
            }

            Debug.Log($"【懸崖防禦】{other.name} 已被 KillZone 處決！");
            return; // 小怪處理完，直接折返
        }

        //  情況 2：如果碰到的是玩家
        if (other.CompareTag("Player"))
        {
            Debug.Log("【懸崖防禦】偵測到玩家掉落，已將控制權交給 AchievementTrigger！");
        }
    }
}