using UnityEngine;
using System.Collections;

public class WeaponDamage : MonoBehaviour
{
    // set damage number
    public int damage = 10;

    [Header("碰撞检测持续时间")]
    public float attackDuration = 0.2f; // 按下J后，Collider开启多长时间（秒）

    private Collider2D myCollider;

    

    void Start()
    {
        // 1. 获取武器自身的 Collider 2D 组件
        myCollider = GetComponent<Collider2D>();

        // 2. 游戏一开始，默认把自己的 Collider 关闭，防止平时走路蹭死怪
        if (myCollider != null)
        {
            myCollider.enabled = false;
        }
    }

    void Update()
    {
        // 3. 每一帧监听键盘：如果按下 J 键，启动开启 Collider 的协程
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(TriggerColliderRoutine());
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