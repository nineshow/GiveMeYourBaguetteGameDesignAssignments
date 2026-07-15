using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    public GameObject coinPrefab; 

    public GameObject loadTriggerPrefab; // Reference to the load trigger prefab

     [Header("Health Settings")]
    public int maxHP = 250;
    public int currentHP;

    [Header("UI Settings")]
    public Image healthFill;

    [Header("Optional")]
    //if true=destroy gameobject, false=manually do something else
    public bool destroyOnDeath;

    [Header("Rage Mode Settings")]
    public bool hasRageMode;
    private bool isRageModeActive;
    private bool rageModeDisabled;

    void Start()
    {
        currentHP = maxHP;
        if(loadTriggerPrefab != null)
        {
            loadTriggerPrefab.SetActive(false); // Deactivate the load trigger at the start
        }
        UpdateHealthUI();
    }

    public void TakeDamage(int damage, bool isChargeAttack=false)
    {
        PlayerCombat combat=GetComponent<PlayerCombat>();

        float multiplier=1f;

        //get the multiplier value
        if(combat!=null)
        {
            multiplier=combat.GetDamageMultiplier();
        }

        if(isRageModeActive&&!isChargeAttack)
        {
            multiplier*=0.01f;
        }

        if(isChargeAttack && hasRageMode)
        {
            rageModeDisabled=true;
            isRageModeActive=false;
            multiplier=1f;
            Debug.Log("Boss Rage Mode Disabled by Charge Attack!");
        }

        //calculate with multiplier
        int finalDamage=Mathf.RoundToInt(damage*multiplier);

        currentHP -= finalDamage;

        if(currentHP<=maxHP*0.3f && hasRageMode 
        && !rageModeDisabled && !isRageModeActive)
        {
            isRageModeActive=true;
            Debug.Log("Boss Rage Mode Activated!");
        }
        

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
        if (loadTriggerPrefab != null)
        {
            loadTriggerPrefab.SetActive(true); // Activate the load trigger when the boss dies
        }
    }
}
