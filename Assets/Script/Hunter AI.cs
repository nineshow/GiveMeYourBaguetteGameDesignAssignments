using UnityEngine;

public class HunterAI : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 2f;

    [Header("范围判定 (从大到小)")]
    public float detectionRange = 6f;   // 索敌范围 (进入后开始追击)
    public float rangedAttackRange = 4f; // 大攻击范围 (开枪)
    public float meleeAttackRange = 1.5f; // 小攻击范围 (近战抡枪)

    [Header("远程攻击 (开枪)")]
    public int rangedDamage = 15;
    public float rangedCooldown = 2f;    // 开枪冷却
    public float recoilForce = 5f;       // 开枪时的后坐力击退
    public GameObject bulletPrefab;      // 子弹预制体
    public Transform firePoint;          // 枪口位置（子弹生成点）

    [Header("近战攻击 (抡枪)")]
    public int meleeDamage = 20;
    public float meleeCooldown = 1.5f;   // 近战冷却

    private Transform player;
    private Rigidbody2D rb;
    private float rangedTimer;
    private float meleeTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 自动寻找玩家
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rangedTimer = rangedCooldown;
        meleeTimer = meleeCooldown;
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy) return;

        // 冷却计时器递减
        if (rangedTimer > 0) rangedTimer -= Time.deltaTime;
        if (meleeTimer > 0) meleeTimer -= Time.deltaTime;

        // 计算与玩家的直线距离
        float distance = Vector2.Distance(transform.position, player.position);

        // 状态机判断（优先判断最近的距离）
        if (distance <= meleeAttackRange)
        {
            // 状态 3：玩家贴脸，触发 360° 抡枪
            if (meleeTimer <= 0) MeleeAttack();
        }
        else if (distance <= rangedAttackRange)
        {
            // 状态 2：玩家在中距离，触发开枪
            if (rangedTimer <= 0) RangedAttack();
        }
        else if (distance <= detectionRange)
        {
            // 状态 1：玩家在远距离视野内，追逐玩家
            ChasePlayer();
        }
        else
        {
            // 状态 0：丢失目标，原地待命 (或者你也可以改成巡逻)
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    // 追逐玩家
    void ChasePlayer()
    {
        float direction = (player.position.x > transform.position.x) ? 1f : -1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        FlipTowards(direction);
    }

    // 远程攻击（开枪）
    void RangedAttack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y); // 开枪时站定
        
        // 计算指向玩家的方向向量
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        
        // 翻转身体朝向玩家
        float faceDir = directionToPlayer.x > 0 ? 1f : -1f;
        FlipTowards(faceDir);

        // 1. 生成子弹，并根据玩家位置调整发射角度（完美还原你的图纸要求）
        if (bulletPrefab != null && firePoint != null)
        {
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
        }

        // 2. 产生后坐力（给猎人施加一个反方向的冲击力）
        rb.AddForce(-directionToPlayer * recoilForce, ForceMode2D.Impulse);

        rangedTimer = rangedCooldown; // 重置冷却
    }

    // 近战攻击（360度抡枪）
    void MeleeAttack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y); // 抡枪时站定

        // 在猎人中心生成一个圆形的物理检测范围（完美还原你图纸里的粉色圆圈）
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, meleeAttackRange);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Player"))
            {
                // 获取玩家血量组件并造成伤害，同时兼容你之前的防御判定
                HealthPoint hp = enemy.GetComponent<HealthPoint>();
                PlayerCombat combat = enemy.GetComponent<PlayerCombat>();
                
                if (hp != null)
                {
                    hp.TakeDamage(meleeDamage);
                    if (combat != null && combat.isDefending)
                        Debug.Log("玩家防住了猎人的近战攻击！");
                    else
                        Debug.Log("猎人造成了近战伤害！");
                }
            }
        }

        meleeTimer = meleeCooldown; // 重置冷却
    }

    void FlipTowards(float direction)
    {
        if (direction > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (direction < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    // 绘制判定圈（方便你在 Unity 里调参）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // 黄圈：索敌
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedAttackRange); // 蓝圈：开枪
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange); // 红圈：抡枪
    }
}