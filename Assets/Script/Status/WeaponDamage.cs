using UnityEngine;
using System.Collections;

public class WeaponDamage : MonoBehaviour
{
    // set damage number
    public int damage = 10;

    [Header("碰撞检测持续时间")]
    public float attackDuration = 0.2f; // 按下J后，Collider开启多长时间（秒）

    [Header("Combo Hit Setting ")] 
    public int comboStep = 0;           // 当前是第几段攻击 (0:没攻击, 1:第一击, 2:第二击, 3:第三击)
    public float comboResetTime = 0.6f;  // 超过 0.6 秒不按下一发，连击自动重置
    private float lastAttackTime;        // 记录上一次按下攻击的时间点

    [Header("Hit Cooldown Setting")]
    public float attackCooldown = 0.2f;  // 每次攻击之间必须间隔多少秒（比如 0.3 秒，数值越大攻击越慢）
    private float nextAttackTime = 0f;   // 下一次允许发起攻击的时间点

    private Collider2D myCollider;
    private Animator anim; // 声明动画控制器
    private PlayerCombat playerCombat; // 用来联动玩家的防御状态
    

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
        if (myCollider != null) myCollider.enabled = false;

        anim = transform.root.GetComponentInChildren<Animator>();
        playerCombat = transform.root.GetComponent<PlayerCombat>();
        
    }

    void Update()
    {

        // 按下 J 键攻击
        if (Input.GetKeyDown(KeyCode.J))
        {
            // 【核心联动】：如果父物体正在防守(isDefending == true)，直接拦截不准出招！
            if (playerCombat != null && playerCombat.isDefending)
            {
                return;
            }

            // 🔥 2. 核心加入：如果现在还没到允许攻击的时间，直接拦截！拒不出招！
            if (Time.time < nextAttackTime)
            {
                return; // 这样你就没办法通过狂按键盘来鬼畜无限连击了
            }

            // 执行攻击
            Attack();
        }
    }
    void Attack()
    {
        lastAttackTime = Time.time;
         if (Time.time - lastAttackTime > comboResetTime && comboStep > 0)
        {
            ResetCombo();
        }
        
        nextAttackTime = Time.time + attackCooldown;
        
        comboStep++;

        if (comboStep > 3)
        {
            comboStep = 1; // 三段砍完回到第一段
        }

        if (anim != null)
        {
            // 实时把连击段数（1、2、3）塞给 Animator 状态机！
            anim.SetInteger("comboStep", comboStep);
            anim.SetTrigger("Attack");

            // 开启物理碰撞判定
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