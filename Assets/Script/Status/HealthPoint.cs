using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthPoint : MonoBehaviour
{
    public GameObject coinPrefab; 
    public GameObject loadTriggerPrefab; // Reference to the load trigger prefab
    

     [Header("Health Settings")]
    public int maxHP = 100;
    public int currentHP;

    [Header("UI Settings")]
    public Image healthFill;

    [Header("Optional")]
    //if true=destroy gameobject, false=manually do something else
    public bool destroyOnDeath;
    public bool hasRageMode;
    public bool isRageModeActive;   // 改成 public
    public bool rageModeDisabled;   // 改成 public

    

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
        PlayerMovement movement=GetComponent<PlayerMovement>();

        float multiplier=1f;

        //get the multiplier value
        if(combat!=null)
        {
            multiplier=combat.GetDamageMultiplier();
        }

        //if is in rage mode, reduce damage by 99%
        if(isRageModeActive&&!isChargeAttack)
        {
            multiplier*=0.01f;
        }

        //disable rage mode if it is charge attack
        if(gameObject.CompareTag("Monster") && isChargeAttack && hasRageMode)
        {
            rageModeDisabled=true;
            isRageModeActive=false;
            multiplier=1f;
            Debug.Log("Monster Rage Mode Disabled by Charge Attack!");
        }

        //calculate with multiplier
        int finalDamage=Mathf.RoundToInt(damage*multiplier);

        currentHP -= finalDamage;

       
        // 【新增】：当怪物血量低于30%时，触发 Rage Mode
        if(gameObject.CompareTag("Monster") && currentHP<=maxHP*0.3f 
        && hasRageMode && !isRageModeActive
        && !rageModeDisabled)
        {
            isRageModeActive=true;
            Debug.Log("Monster Rage Mode Activated!");
        }

        if (currentHP < 0) currentHP = 0;

        UpdateHealthUI();

        

        Debug.Log(gameObject.name + " HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
        if(gameObject.CompareTag("Player") )
        {
            // 【新增安全检查】：确保拿到了玩家移动脚本再调用，防止怪物挨打时报错
            if (movement != null) 
            {
                movement.isDamage();
            }
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
        else
        {
            LevelManager.Instance.gameOverPanel.SetActive(true);
            LevelManager.Instance.PauseGame();
        }
        if (loadTriggerPrefab != null)
        {
            loadTriggerPrefab.SetActive(true); // Activate the load trigger when the player dies
        }
    }
}
