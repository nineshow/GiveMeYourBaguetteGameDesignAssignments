using UnityEngine;

public class HunterAI : MonoBehaviour
{
    [Header("移动与巡逻设置")]
    public float moveSpeed = 2f;         // 追逐玩家时的速度
    public float patrolSpeed = 1.5f;     // 巡逻时的散步速度
    public float patrolDistance = 4f;    // 巡逻范围

    [Header("范围判定 (从大到小)")]
    public float detectionRange = 6f;    // 索敌范围 (黄圈)
    public float attackRange = 3f;       // 开火射程 (蓝圈)

    [Header("霰弹枪设置")]
    public int attackDamage = 15;        // 霰弹枪伤害
    public float attackCooldown = 2f;    // 开枪后的冷却时间（秒）
    public float recoilForce = 5f;       // 后坐力击退
    [Range(0f, 180f)]
    public float shotgunAngle = 60f;     // 霰弹枪的扇形总夹角

    [Header("攻击前摇(预警)设置")]
    public float windUpTime = 0.5f;      // 【核心新增】：进入射程后开枪的前摇时间（0.5秒）
    private float windUpTimer;           // 前摇倒计时器
    private bool isPreparingAttack = false; // 是否正在举枪瞄准蓄力中

    private Transform player;
    private Rigidbody2D rb;
    private float attackTimer;
    private Vector2 startPosition;    
    private Vector2 patrolTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        patrolTarget = new Vector2(startPosition.x + patrolDistance, startPosition.y);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        attackTimer = 0f; // 游戏刚开始时，武器是装填好的，可以随时进入前摇
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy) 
        {
            CancelAttackPreparation();
            Patrol();
            return;
        }

        // 攻击后的开枪CD计时器递减
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);

        // 核心状态机判断
        if (distance <= attackRange)
        {
            // --- 状态 1：玩家进入射程 ---
            rb.velocity = new Vector2(0, rb.velocity.y); // 无论是在瞄准还是在等CD，都必须停下脚步

            // 瞄准时，脸始终死死朝向玩家
            float targetDir = player.position.x > transform.position.x ? 1f : -1f;
            FlipTowards(targetDir);

            // 如果开枪CD已经好了，开始进行 0.5秒 的前摇蓄力
            if (attackTimer <= 0)
            {
                if (!isPreparingAttack)
                {
                    // 刚刚进入前摇瞬间：触发瞄准
                    isPreparingAttack = true;
                    windUpTimer = windUpTime; // 重置 0.5 秒倒计时
                    
                    // 💡 提示：以后如果你加了动画，可以在这里播放“举枪/瞄准”的动画
                }

                // 前摇倒计时
                windUpTimer -= Time.deltaTime;
                
                if (windUpTimer <= 0f)
                {
                    // 0.5秒时间到，正式开火！
                    ShotgunAttack();
                    isPreparingAttack = false; // 结束前摇状态
                }
            }
        }
        else
        {
            // --- 状态 2：玩家在射程之外 ---
            
            // 【神级细节】：如果玩家在猎人举枪的 0.5 秒内逃出了蓝圈，立刻取消蓄力（俗称“骗招/拉扯”）
            CancelAttackPreparation();

            if (distance <= detectionRange)
            {
                // 玩家在视野内：追逐玩家
                ChasePlayer();
            }
            else
            {
                // 丢失目标：恢复巡逻
                Patrol();
            }
        }
    }

    // 取消攻击前摇的方法
    void CancelAttackPreparation()
    {
        if (isPreparingAttack)
        {
            isPreparingAttack = false;
        }
    }

    void Patrol()
    {
        float direction = (patrolTarget.x > transform.position.x) ? 1f : -1f;
        FlipTowards(direction);
        rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);

        if (Mathf.Abs(transform.position.x - patrolTarget.x) < 0.2f)
        {
            if (patrolTarget.x > startPosition.x)
                patrolTarget = new Vector2(startPosition.x - patrolDistance, startPosition.y); 
            else
                patrolTarget = new Vector2(startPosition.x + patrolDistance, startPosition.y); 
        }
    }

    void ChasePlayer()
    {
        float direction = (player.position.x > transform.position.x) ? 1f : -1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        FlipTowards(direction);
    }

    // 霰弹枪喷射
    void ShotgunAttack()
    {
        float faceDirX = transform.localScale.x > 0 ? 1f : -1f;
        Vector2 facingDirection = new Vector2(faceDirX, 0).normalized;
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        float angleToPlayer = Vector2.Angle(facingDirection, directionToPlayer);

        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, attackRange);
        
        foreach (Collider2D target in hitTargets)
        {
            if (target.CompareTag("Player"))
            {
                if (angleToPlayer <= (shotgunAngle / 2f))
                {
                    HealthPoint hp = target.GetComponent<HealthPoint>();
                    if (hp != null) hp.TakeDamage(attackDamage);
                    
                    BossHealth bh = target.GetComponent<BossHealth>();
                    if (bh != null) bh.TakeDamage(attackDamage);

                }
            }
        }

        // 后坐力倒退
        rb.AddForce(-facingDirection * recoilForce, ForceMode2D.Impulse);

        attackTimer = attackCooldown; // 重新进入开枪CD
    }

    void FlipTowards(float direction)
    {
        if (direction > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (direction < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); 
        
        Gizmos.color = Color.blue;
        float faceDirX = transform.localScale.x > 0 ? 1f : -1f;
        Vector3 facing = new Vector3(faceDirX, 0, 0);
        
        Vector3 leftBoundary = Quaternion.Euler(0, 0, shotgunAngle / 2f) * facing;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -shotgunAngle / 2f) * facing;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * attackRange);
        Gizmos.DrawWireSphere(transform.position, attackRange); 
    }
}