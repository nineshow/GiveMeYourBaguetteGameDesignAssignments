using UnityEngine;
using System.Collections;

public class WeaponDamage : MonoBehaviour
{
    public int damage = 10;

    [Header("Charge Attack Settings")]
    public int chargeGainPerHit = 20;

    [Header("碰撞检测持续时间")]
    public float attackDuration = 0.2f;

    [Header("Audio Settings")]
    public AudioSource audioSource; 
    public AudioClip attackSound;   
    public AudioClip hitSound;
    public AudioClip chargeAttackSound;      

    private Collider2D myCollider;
    private PlayerCombat playerCombat; 

    private bool isChargeAttack = false; 

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
        if (myCollider != null) myCollider.enabled = false;

        playerCombat = transform.root.GetComponent<PlayerCombat>();
    }


    // 【被 PlayerCombat 呼叫】：觸發普通攻擊的碰撞體與音效
    public void TriggerNormalAttackCollider()
    {
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        StopAllCoroutines(); // 防止連續呼叫時協程衝突
        StartCoroutine(TriggerColliderRoutine());
    }

    public void PerformChargeAttack(float delayTime)
    {
        if (myCollider == null) return;

        // 停止之前的协程，防止动作冲突
        StopAllCoroutines();
        
        // 啟動大招的全面延遲協程（同時管轄音效與傷害）
        StartCoroutine(ChargeAttackRoutine(delayTime));
    }

    // 【核心新增】：大招完整同步延遲協程
    private IEnumerator ChargeAttackRoutine(float delay)
    {
        //  1. 默默等待，直到大招砍下去、特效噴發的那一幀
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        //  2. 時間到了！武器第一時間在正確的幀播放大招音效
        if (audioSource != null && chargeAttackSound != null)
        {
            audioSource.PlayOneShot(chargeAttackSound);
        }

        //  3. 同時打開傷害判定碰撞體
        isChargeAttack = true;
        myCollider.enabled = true;

        //  4. 碰撞體持續時間結束後關閉
        yield return new WaitForSeconds(attackDuration);
        
        myCollider.enabled = false;
        isChargeAttack = false;
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
        if(gameObject.CompareTag("PlayerWeapon") && other.CompareTag("Player")) return;
        if(gameObject.CompareTag("MonsterWeapon") && other.CompareTag("Monster")) return;

        HealthPoint health = other.GetComponent<HealthPoint>();
        if(health != null)
        {
            health.TakeDamage(damage, isChargeAttack);
            if(gameObject.CompareTag("PlayerWeapon") && other.CompareTag("Monster") && playerCombat != null)
            {
                if(!isChargeAttack) playerCombat.AddCharge(chargeGainPerHit);
            }
            PlayHitSound();
            return; 
        }

        BossHealth bossHealth = other.GetComponent<BossHealth>();
        if(bossHealth != null)
        {
            bossHealth.TakeDamage(damage, isChargeAttack);
            if(gameObject.CompareTag("PlayerWeapon") && playerCombat != null)
            {
                if(!isChargeAttack) playerCombat.AddCharge(chargeGainPerHit);
            }
            PlayHitSound();
            return; 
        }
    }

    private void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
}