using UnityEngine;

public class EnemySystem : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    // 新增：用于存放金币的预制体模板
    public GameObject coinPrefab; 

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage2(int damage)
    {
        currentHealth -= damage;
        Debug.Log("敌人受到伤害！剩余血量：" + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        
        // 新增：在敌人的当前位置，生成一个金币。Quaternion.identity 表示不旋转。
        Instantiate(coinPrefab, transform.position, Quaternion.identity);
        
        Destroy(gameObject);
    }
}