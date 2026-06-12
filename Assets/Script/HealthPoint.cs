using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPoint : MonoBehaviour
{
    public GameObject coinPrefab; 

     [Header("Health Settings")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Optional")]
    //if true=destroy gameobject, false=manually do something else
    public bool destroyOnDeath;

    void Start()
    {
        currentHP = maxHP;
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
