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

            // 让代码在这里等待一小段时间（比如 0.2 秒，也就是挥棍子砸过去的那一瞬间）
            yield return new WaitForSeconds(attackDuration);

            myCollider.enabled = false; // 时间到了，自动关闭 Collider
        }
    }

    // if is triggered (只有在 Collider 被开启、且碰到物体时才会触发这一段)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it has HealthPoint component
        HealthPoint health = other.GetComponent<HealthPoint>();

        // if no health component, just return and end
        if(health == null)
        {
            return;
        }

        // player will not be hurt by player weapon
        if(gameObject.CompareTag("PlayerWeapon") && other.CompareTag("Player"))
        {
            return;
        }

        // monster will not be hurt by monster weapon
        if(gameObject.CompareTag("MonsterWeapon") && other.CompareTag("Monster"))
        {
            return;
        }

        // if reach here, can just do damage as usual
        // 只要通过了上面的友军免伤检查，直接造成伤害！
        health.TakeDamage(damage);
    }
}