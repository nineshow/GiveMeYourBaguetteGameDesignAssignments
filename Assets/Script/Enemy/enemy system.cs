using UnityEngine;
using System.Collections; // 【必须引入】：为了使用协程

public class EnemyAI : MonoBehaviour
{
    [Header("移动速度")]
    public float patrolSpeed = 1.5f;    // 巡逻时的走路速度
    public float chaseSpeed = 2f;     // 发现玩家后的奔跑速度
    public float patrolDistance = 3f; // 巡逻范围（以出生点为中心，左右走多远）

    [Header("视野与战斗")]
    public float detectionRange = 4f; // 检测范围（玩家进入这个半径开始追逐）
    public float attackRange = 1.2f;  // 攻击范围（玩家进入这个半径准备攻击）
    public int attackDamage = 10;     // 怪物每次攻击的伤害值
    public float attackCooldown = 1f; // 攻击间隔（也是进入范围后的延迟时间）

    [Header("动画节奏(新)")]
    public float attackWindUp = 0.2f; // 【核心】：攻击前摇（从播放动画到扣血之间的延迟时间）
    private bool isAttacking = false; // 状态锁：判断小怪是否正在砍人

    private Vector2 startPosition;    // 记录怪物出生的初始位置
    private Vector2 patrolTarget;     // 当前正在朝哪个点巡逻
    private Transform player;         // 玩家的坐标引用
    private Rigidbody2D rb;
    
    private float attackTimer;        // 攻击计时器

    // 【新增 1】：声明动画控制器
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        
        // 游戏开始时，先定一个小目标：向右侧巡逻
        patrolTarget = new Vector2(startPosition.x + patrolDistance, startPosition.y);

        // 自动通过 Tag 找到玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // 初始化计时器
        attackTimer = attackCooldown; 

        // 【新增 2】：获取小怪身上的 Animator
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
       // 【核心拦截】：如果小怪正在砍人，锁死它的大脑，不允许它移动或转身！
        if (isAttacking)
        {
            return; 
        }
       
        // 【新增 3】：实时同步小怪的“行走”动画
        if (anim != null)
        {
            // 只要 X 轴速度的绝对值大于 0.1，就认为它在走路
            bool isMoving = Mathf.Abs(rb.velocity.x) > 0.1f;
            anim.SetBool("isWalking", isMoving);
        }

        // 如果玩家不存在或者玩家已经死亡（隐藏了），怪物就乖乖回去巡逻
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            Patrol();
            return;
        }

        // 计算怪物和玩家之间的直线距离
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 核心 AI 逻辑（状态机）
        if (distanceToPlayer <= attackRange)
        {
            // 状态 3：进入攻击范围，准备攻击
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            // 状态 2：进入视野，但还没到攻击距离，追逐玩家
            ChasePlayer();
        }
        else
        {
            // 状态 1：玩家不在视野内，按预定路线巡逻
            Patrol();
        }
    }

    // --- 状态 1：巡逻 ---
    void Patrol()
    {
        // 只要不在攻击范围内，计时器永远保持在 1 秒（确保下次玩家进入时，依然有 1 秒的反应时间）
        attackTimer = attackCooldown;

        // 判断应该往左走还是往右走
        float direction = (patrolTarget.x > transform.position.x) ? 1f : -1f;
        rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);

        // 转向动画
        FlipTowards(direction);

        // 如果走到了巡逻目标点附近，就掉头
        if (Mathf.Abs(transform.position.x - patrolTarget.x) < 0.2f)
        {
            if (patrolTarget.x > startPosition.x)
                patrolTarget = new Vector2(startPosition.x - patrolDistance, startPosition.y); // 设为左边目标
            else
                patrolTarget = new Vector2(startPosition.x + patrolDistance, startPosition.y); // 设为右边目标
        }
    }

    // --- 状态 2：追逐 ---
    void ChasePlayer()
    {
        attackTimer = attackCooldown; // 追逐时也要重置计时器

        // 判断玩家在左边还是右边
        float direction = (player.position.x > transform.position.x) ? 1f : -1f;
        
        // 朝着玩家加速跑过去
        rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
        FlipTowards(direction);
    }

    // --- 状态 3：攻击 ---
    void AttackPlayer()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        float direction = (player.position.x > transform.position.x) ? 1f : -1f;
        FlipTowards(direction);

        attackTimer -= Time.deltaTime;
        
        if (attackTimer <= 0f)
        {
            // 时间到了，启动攻击协程！
            StartCoroutine(PerformAttack());
        }
    }

    // 【全新重写的攻击协程】
    IEnumerator PerformAttack()
    {
        // 1. 上锁！彻底刹车，强行关闭行走动画
        isAttacking = true;
        rb.velocity = new Vector2(0, rb.velocity.y); 
        
        if (anim != null)
        {
            anim.SetBool("isWalking", false); 
            anim.SetTrigger("Attack"); // 触发攻击动作
        }

        // 2. 等待“前摇”时间（也就是让动画飞刀或砍下的时间）
        yield return new WaitForSeconds(attackWindUp);

        // 3. 伤害判定：加了一个防逃课机制，如果在这 0.3 秒内玩家用位移闪出了范围，就不扣血（空刀）
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            HealthPoint hp = player.GetComponent<HealthPoint>();
            PlayerCombat combat = player.GetComponent<PlayerCombat>();

            if (hp != null)
            {
                hp.TakeDamage(attackDamage);

                if (combat != null && combat.isDefending)
                {
                    int realDamage = Mathf.RoundToInt(attackDamage * combat.GetDamageMultiplier());
                    Debug.Log("玩家格挡成功，受到了 " + realDamage + " 点伤害");
                }
                else
                {
                    Debug.Log("怪物造成了 " + attackDamage + " 点伤害");
                }
            }
        }

        // 4. 攻击完毕，重新进入发呆冷却期
        attackTimer = attackCooldown; 
        isAttacking = false; // 解锁，允许再次行动
    }

    // --- 辅助方法：翻转怪物贴图 ---
    void FlipTowards(float direction)
    {
        if (direction > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); 
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);    
    }
}