using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 必须引入UI命名空间才能控制进度条图片

public class PlayerCombat : MonoBehaviour
{
    [Range(0f, 1f)]
    public float damageReduction = 0.2f;

    public bool isDefending;

    [Header("Normal Attack")]
    public KeyCode normalAttackKey = KeyCode.J;
    public GameObject normalAttackEffectPrefab; // 普通攻击的特效预制体
    public float effectDelay = 0.15f;           // 特效延迟生成的秒数（可根据动画自行调整）

    [Header("Combo & Cooldown Setting ")] 
    public int comboStep = 0;          
    public float comboResetTime = 0.8f;  
    public float attackCooldown = 0.5f;  
    private float nextAttackTime = 0f;   
    private float lastAttackTime;

    [Header("Charge Attack")]
    public int currentCharge = 0;
    public int maxCharge = 100;
    public bool chargeAttackReady = false;
    public KeyCode chargeAttackKey = KeyCode.R;
    public bool chargeAttackActivated = false;
    public GameObject chargeAttackEffectPrefab; // 蓄力攻击的特效预制体
    public float chargeEffectDelay = 0.25f;

    [Header("Attack Settings (Common)")]
    public Transform attackPoint;               // 攻击特效的生成点
    public float effectDuration = 0.5f;         // 特效自动销毁时间（秒）

    [Header("UI Settings")]
    public Image chargeBarFill;                 // UI 充能条（必须是Image Type为Filled的图片）
    public GameObject glowLayer;                // 满能量时的发光特效图层/图片

    private Animator anim;

    [Header("Weapon")]
    public WeaponDamage weaponDamage;


    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        weaponDamage = GetComponentInChildren<WeaponDamage>();
        
        // 游戏开始时刷新一次 UI，确保进度条初始状态是空的
        UpdateChargeUI();
    }

    void Update()
    {
        // 1. 防御逻辑 (按住K键)
        isDefending = Input.GetKey(KeyCode.K);
        if (anim != null)
        {
            anim.SetBool("isDefending", isDefending);
        }

        // 2. 普通攻击逻辑 (按下J键)
        if (Input.GetKeyDown(normalAttackKey))
        {
            if (isDefending) return;

            // 🔒【嚴格冷卻攔截】：如果冷卻時間沒到，直接折返，絕對不播動畫或生成特效！
            if (Time.time < nextAttackTime) return; 

            NormalAttack();
        }

        // 3. 蓄力/特殊攻击逻辑 (条件：按下R键 + 能量已满 + 当前没有正在释放大招)
        if (Input.GetKeyDown(chargeAttackKey) && chargeAttackReady && !chargeAttackActivated)
        {
            // 立刻锁死状态，防止玩家狂按R键爆出一堆特效
            chargeAttackActivated = true;

            // 播放大招动画
            if (anim != null)
            {
                anim.SetTrigger("ChargeAttack");
            }

            if(weaponDamage != null)
            {
                weaponDamage.PerformChargeAttack(chargeEffectDelay);
            }
            StartCoroutine(SpawnEffectWithDelay(chargeAttackEffectPrefab, chargeEffectDelay));
            // 释放大招后立刻调用消耗逻辑，进度条会瞬间清零
            ConsumeChargeAttack();

            Debug.Log("Charge Attack Activated & Bar Reset!");
        }
    }

    void NormalAttack()
    {
        // 檢查連擊重置
        if (Time.time - lastAttackTime > comboResetTime && comboStep > 0)
        {
            ResetCombo();
        }

        // 更新計時器
        lastAttackTime = Time.time;
        nextAttackTime = Time.time + attackCooldown; // 鎖死冷卻時間

        comboStep++;
        if (comboStep > 3) comboStep = 1;

        // 瞬間觸發動畫（讓角色先動起來）
        if (anim != null)
        {
            anim.SetInteger("comboStep", comboStep);
            anim.SetTrigger("Attack"); 
        }

        // 瞬間叫武器打開碰撞體與播放音效（打擊判定不延遲）
        if (weaponDamage != null)
        {
            weaponDamage.TriggerNormalAttackCollider();
        }

        // 🎯【核心改動】：啟動協程，讓特效在後面慢慢排隊生成
        StartCoroutine(SpawnEffectWithDelay(normalAttackEffectPrefab, effectDelay));

        Debug.Log($"Normal Attack Step {comboStep} Triggered!");
    }

    // 🎯【新增】：負責處理特效延遲生成的協程
    private IEnumerator SpawnEffectWithDelay(GameObject effectPrefab, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        SpawnEffect(effectPrefab);
    }

    public void ResetCombo()
    {
        comboStep = 0;
        if (anim != null) anim.SetInteger("comboStep", 0);
    }

    // 防御减伤相关
    public float GetDamageMultiplier()
    {
        if (isDefending)
        {
            return 1f - damageReduction;
        }
        return 1f;
    }

    // 增加蓄力值（充能）
    public void AddCharge(int amount)
    {
        currentCharge += amount;
        if (currentCharge >= maxCharge)
        {
            currentCharge = maxCharge;
            chargeAttackReady = true;
        }
        
        // 每次能量改变时，同步更新 UI 进度条
        UpdateChargeUI();
        
        Debug.Log("Current Charge: " + currentCharge + "/" + maxCharge);
    }

    public bool IsChargeAttackReady()
    {
        return chargeAttackReady;
    }

    // 消耗蓄力值并清空UI
    public bool ConsumeChargeAttack()
    {
        if (!chargeAttackActivated || !chargeAttackReady)
        {
            return false;
        }

        // 数据清零
        currentCharge = 0;
        chargeAttackReady = false;
        chargeAttackActivated = false;

        // 清空 UI 进度条显示
        UpdateChargeUI();

        Debug.Log("Charge Attack Consumed & Reset!");

        return true;
    }

    // 通用的生成特效方法
    public void SpawnEffect(GameObject effectPrefab)
    {
        // 确保你已经在面板里挂载了预制体和攻击点
        if (effectPrefab != null && attackPoint != null)
        {
            // 实例化特效
            GameObject effect = Instantiate(effectPrefab, attackPoint.position, attackPoint.rotation);

            // 处理角色左右翻转导致的特效朝向问题（假设你是通过 scale.x 翻转主角的）
            if (transform.localScale.x < 0)
            {
                Vector3 effectScale = effect.transform.localScale;
                //effectScale.x *= -1;
                effect.transform.localScale = effectScale;
            }

            // 延时销毁特效，防止卡顿
            Destroy(effect, effectDuration);
        }
    }

    // 专门用来刷新 UI 充能条进度的方法
    private void UpdateChargeUI()
    {
        if (chargeBarFill != null)
        {
            // Fill Amount 需要 0 到 1 之间的小数。
            // 强转 float 是为了避免整数除法直接变成 0 (比如 50/100 默认等于 0)。
            chargeBarFill.fillAmount = (float)currentCharge / maxCharge;
        }
            if (glowLayer != null)
        {
            // 当当前能量达到最大值时，打开全局发光图层，否则关闭
            if (currentCharge >= maxCharge)
            {
                glowLayer.SetActive(true);
            }
            else
            {
                glowLayer.SetActive(false);
            }
        }
    }
}