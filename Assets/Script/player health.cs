using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        currentHealth = 50;
        Debug.Log("当前生命值：" + currentHealth);
    }

    // 打开背包回血
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        
        // 防止血量超过上限
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        Debug.Log("当前生命值：" + currentHealth);
    }
}