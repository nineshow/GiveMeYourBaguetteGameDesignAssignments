using UnityEngine;

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

    private Vector2 startPosition;    // 记录怪物出生的初始位置
    private Vector2 patrolTarget;     // 当前正在朝哪个点巡逻
    private Transform player;         // 玩家的坐标引用
    private Rigidbody2D rb;
    
    private float attackTimer;        // 攻击计时器

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
    }

    void Update()
    {
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
    // --- 状态 3：攻击 ---
    void AttackPlayer()
    {
        // 站在原地打人，停止移动
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        // 脸一定要朝向玩家
        float direction = (player.position.x > transform.position.x) ? 1f : -1f;
        FlipTowards(direction);

        // 开始倒计时
        attackTimer -= Time.deltaTime;
        
        if (attackTimer <= 0f)
        {
            // 获取玩家身上的生命组件和战斗组件
            HealthPoint hp = player.GetComponent<HealthPoint>();
            PlayerCombat combat = player.GetComponent<PlayerCombat>(); // 【新增】：侦测玩家的防御状态

            if (hp != null)
            {
                // 怪物发动攻击，传入基础伤害
                // (最终扣多少血依然由 HealthPoint 里你写的 Mathf.RoundToInt 完美接管)
                hp.TakeDamage(attackDamage);

                // 根据玩家的防御状态，输出不同的反馈
                if (combat != null && combat.isDefending)
                {
                    // 计算出实际伤害用于控制台显示（和你在 HealthPoint 里的算法一致）
                    int realDamage = Mathf.RoundToInt(attackDamage * combat.GetDamageMultiplier());
                    Debug.Log("玩家格挡成功，受到了 " + realDamage + " 点伤害");
                    
                    // 【进阶玩法】：如果玩家防住了，你可以让怪物往后退一点（弹刀效果）
                    // rb.AddForce(new Vector2(-direction * 5f, 0), ForceMode2D.Impulse); 
                }
                else
                {
                    Debug.Log("怪物造成了 " + attackDamage + " 点伤害");
                }
            }
            
            // 重新把计时器设为攻击间隔，准备下一次攻击
            attackTimer = attackCooldown; 
        }
    }

    // --- 辅助方法：翻转怪物贴图 ---
    void FlipTowards(float direction)
    {
        if (direction > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    // --- 辅助方法：在 Unity 编辑器中画出检测圈，方便你调整数值 ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // 黄圈：视野范围
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);    // 红圈：攻击范围
    }
}