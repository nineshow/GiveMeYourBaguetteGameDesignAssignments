using UnityEngine;
using System.Collections;

public class WeaponDamage : MonoBehaviour
{
    public int damage = 10;

    [Header("碰撞检测持续时间")]
    public float attackDuration = 0.2f;

    [Header("Combo Hit Setting ")] 
    public int comboStep = 0;          
    public float comboResetTime = 0.6f;  
    private float lastAttackTime;        

    [Header("Hit Cooldown Setting")]
    public float attackCooldown = 0.2f;  
    private float nextAttackTime = 0f;   

    // 【新增】：音效设置面板
    [Header("Audio Settings")]
    public AudioSource audioSource; // 播放声音的“扬声器”
    public AudioClip attackSound;   // 挥刀的音效文件
    public AudioClip hitSound;      // （可选）砍中敌人的音效文件

    private Collider2D myCollider;
    private Animator anim; 
    private PlayerCombat playerCombat; 

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
        if (myCollider != null) myCollider.enabled = false;

        anim = transform.root.GetComponentInChildren<Animator>();
        playerCombat = transform.root.GetComponent<PlayerCombat>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (playerCombat != null && playerCombat.isDefending)
            {
                return;
            }

            if (Time.time < nextAttackTime)
            {
                return; 
            }

            Attack();
        }
    }

    void Attack()
    {
        // 【修正 Bug】：先检查距离上一次攻击过了多久，再更新 lastAttackTime
        if (Time.time - lastAttackTime > comboResetTime && comboStep > 0)
        {
            ResetCombo();
        }
        
        // 更新本次攻击的时间戳
        lastAttackTime = Time.time;
        nextAttackTime = Time.time + attackCooldown;
        
        comboStep++;

        if (comboStep > 3)
        {
            comboStep = 1; 
        }

        if (anim != null)
        {
            anim.SetInteger("comboStep", comboStep);
            anim.SetTrigger("Attack");

            // 【核心新增】：播放挥刀攻击音效
            if (audioSource != null && attackSound != null)
            {
                // 使用 PlayOneShot 可以允许音效重叠播放，适合快速连击
                audioSource.PlayOneShot(attackSound);
            }

            StartCoroutine(TriggerColliderRoutine());
        }
    }

    public void ResetCombo()
    {
        comboStep = 0;
        if (anim != null)
        {
            anim.SetInteger("comboStep", 0);
        }
    }

    IEnumerator TriggerColliderRoutine()
    {
        if (myCollider != null)
        {
            myCollider.enabled = true; 
            yield return new WaitForSeconds(attackDuration);
            myCollider.enabled = false; 
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(gameObject.CompareTag("PlayerWeapon") && other.CompareTag("Player"))
        {
            return;
        }

        if(gameObject.CompareTag("MonsterWeapon") && (other.CompareTag("Monster")))
        {
            return;
        }

        // --- 第二步：尝试给小怪造成伤害 ---
        HealthPoint health = other.GetComponent<HealthPoint>();
        if(health != null)
        {
            health.TakeDamage(damage);
            PlayHitSound(); // 【新增】：播放击中音效
            return; 
        }

        // --- 第三步：尝试给猎人 Boss 造成伤害 ---
        BossHealth bossHealth = other.GetComponent<BossHealth>();
        if(bossHealth != null)
        {
            bossHealth.TakeDamage(damage);
            PlayHitSound(); // 【新增】：播放击中音效
            return; 
        }
    }

    // 【新增】：封装一个播放击中音效的方法
    private void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
}