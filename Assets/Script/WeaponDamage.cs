using UnityEngine;
using System.Collections;

public class WeaponDamage : MonoBehaviour
{
    // set damage number
    public int damage = 10;

    [Header("碰撞检测持续时间")]
    public float attackDuration = 0.2f; // 按下J后，Collider开启多长时间（秒）

    private Collider2D myCollider;
    private Animator anim; // 声明动画控制器

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
        if (myCollider != null) myCollider.enabled = false;

        anim = transform.root.GetComponentInChildren<Animator>();
        
    }

    void Update()
    {
        // 按下 J 键攻击
        if (Input.GetKeyDown(KeyCode.J))
        {
            // --- 【终极测试的输出加在这里】 ---
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            // ---------------------------------

            StartCoroutine(TriggerColliderRoutine());
            }
        }
    }

    // 控制 Collider 开关的协程
    IEnumerator TriggerColliderRoutine()
    {
        if (myCollider != null)
        {
            myCollider.enabled = true; // 开启 Collider 判定
            yield return new WaitForSeconds(attackDuration);
            myCollider.enabled = false; // 时间到了，自动关闭 Collider
        }
    }

    // if is triggered
    private void OnTriggerEnter2D(Collider2D other)
    {
        // --- 第一步：先进行友军免伤检查 ---
        // player will not be hurt by player weapon
        if(gameObject.CompareTag("PlayerWeapon") && other.CompareTag("Player"))
        {
            return;
        }

        // monster will not be hurt by monster weapon
        // (如果你的猎人 Tag 是 Enemy，这里也不会被拦截，完美兼容)
        if(gameObject.CompareTag("MonsterWeapon") && (other.CompareTag("Monster")))
        {
            return;
        }

        // --- 第二步：尝试给小怪或玩家（带有 HealthPoint）造成伤害 ---
        HealthPoint health = other.GetComponent<HealthPoint>();
        if(health != null)
        {
            health.TakeDamage(damage);
            return; // 造成伤害后直接结束判定
        }

        // --- 第三步：尝试给猎人 Boss（带有 BossHealth）造成伤害 ---
        BossHealth bossHealth = other.GetComponent<BossHealth>();
        if(bossHealth != null)
        {
            bossHealth.TakeDamage(damage);
            return; // 造成伤害后直接结束判定
        }
    }
}