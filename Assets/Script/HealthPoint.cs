using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPoint : MonoBehaviour
{
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

    public void Heal(int amount)
    {
        currentHP += amount;
        //Clamp to make sure 0 < currentHP < maxHP
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }

    //if died use this
    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " died");

        //if destroyOnDeath=True, destroy
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}
