using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Player Live Stats")]
    public int gold = 100;
    public int potionCount = 0;
    public int currentHealth = 100;

    [Header("Settings")]
    public int maxHealth = 100;

    // 專門給「全新開局」或「主選單 Start」呼叫的完全重置
    public void ResetAllToDefault()
    {
        gold = 100;
        potionCount = 0;
        currentHealth = maxHealth;
    }
}