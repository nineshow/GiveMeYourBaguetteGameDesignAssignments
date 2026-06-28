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
    private Animator anim;            // 声明动画控制器

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

        // 获取小怪身上的 Animator
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
       // 【核心拦截】：如果小怪正在砍人，锁死它的大脑，不允许它移动或转身！
        if (isAttacking)
        {
            return; 
        }
       
        // 实时同步小怪的“行走”动画
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
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // --- 状态 1：巡逻 ---
    void Patrol()
    {
        attackTimer = attackCooldown;

        float direction = (patrolTarget.x > transform.position.x) ? 1f : -1f;
        rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);

        FlipTowards(direction);

        if (Mathf.Abs(transform.position.x - patrolTarget.x) < 0.2f)
        {
            if (patrolTarget.x > startPosition.x)
                patrolTarget = new Vector2(startPosition.x - patrolDistance, startPosition.y); 
            else
                patrolTarget = new Vector2(startPosition.x + patrolDistance, startPosition.y); 
        }
    }

    // --- 状态 2：追逐 ---
    void ChasePlayer()
    {
        attackTimer = attackCooldown; 

        float direction = (player.position.x > transform.position.x) ? 1f : -1f;
        
        rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
        FlipTowards(direction);
    }

    // --- 状态 3：攻击 ---
    void AttackPlayer()
    {
        if (isAttacking) return; 

        rb.velocity = new Vector2(0, rb.velocity.y);
        float direction = (player.position.x > transform.position.x) ? 1f : -1f;
        FlipTowards(direction);

        attackTimer -= Time.deltaTime;
        
        if (attackTimer <= 0f)
        {
            isAttacking = true; // 核心防重播机制：立刻上锁
            StartCoroutine(PerformAttack());
        }
    } // 👈 之前這裡漏掉了閉合大括號，現在補上了！

    // 【攻击协程】
    IEnumerator PerformAttack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y); 
        
        if (anim != null)
        {
            anim.SetBool("isWalking", false); 
            anim.SetTrigger("Attack"); // 触发攻击动画
        }

        yield return new WaitForSeconds(attackWindUp);

        // 伤害判定
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

        attackTimer = attackCooldown; 
        isAttacking = false; // 解锁
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