using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    public GameObject coinPrefab; 

     [Header("Health Settings")]
    public int maxHP = 250;
    public int currentHP;

    [Header("UI Settings")]
    public Image healthFill;

    [Header("Optional")]
    //if true=destroy gameobject, false=manually do something else
    public bool destroyOnDeath;

    void Start()
    {
        currentHP = 250;
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        PlayerCombat combat=GetComponent<PlayerCombat>();

        float multiplier=1f;

        //get the multiplier value
        if(combat!=null)
        {
            multiplier=combat.GetDamageMultiplier();
        }

        //calculate with multiplier
        int finalDamage=Mathf.RoundToInt(damage*multiplier);

        currentHP -= finalDamage;

        if (currentHP < 0) currentHP = 0;

        UpdateHealthUI();

        Debug.Log(gameObject.name + " HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        currentHP += healAmount;
        
        // 防止血量超过上限
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
        
        Debug.Log("当前生命值：" + currentHP);

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthFill != null)
        {
            // 将当前血量转换为 0 到 1 之间的小数，赋值给 fillAmount
            healthFill.fillAmount = (float)currentHP / maxHP;
        }
    }

    //if died use this
    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " died");

        //if destroyOnDeath=True, destroy
        if (destroyOnDeath)
        {
            Instantiate(coinPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
