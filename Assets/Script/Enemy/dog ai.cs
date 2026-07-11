using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DogAI : MonoBehaviour
{
    public enum DogState { Patrolling, Chasing, WaitingToAttack, Attacking }
    public DogState currentState;

    [Header("目标引用")]
    public Transform player;

    [Header("属性设置")]
    public int attackDamage = 10;      // 扑击和挠击造成的伤害相同

    [Header("范围设置")]
    public float detectionRadius = 5f; // 索敌范围 (黄圈)
    public float attackRadius = 2.5f;  // 扑击范围 (红圈)
    public float scratchRadius = 0.8f; // 极近距离的挠击范围 (橙圈)

    [Header("移动速度")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("横向巡逻设置")]
    public float patrolDistance = 3f;  
    public float idleTime = 1.5f;      
    private float leftLimit;
    private float rightLimit;
    private int patrolDirection = 1;   
    private float idleTimer;
    private bool isIdling = false;

    [Header("远程攻击设置")]
    public float pounceForceX = 5f;    
    public float pounceForceY = 6f;    
    public float pouncePreparationTime = 0.3f; 

    [Header("近战攻击")]
    public float scratchPreparationTime = 0.2f; // 挠击的前摇比扑击快一点
    public float scratchHitboxOffset = 0.5f;    // 爪子攻击判定的前方偏移距离
    public float scratchHitboxRadius = 0.4f;    // 爪子攻击判定的范围大小

    [Header("公共攻击设置")]
    public float attackCooldown = 1f;  
    private float lastAttackTime;
    
    // 用于控制物理碰撞伤害判定的开关 (仅用于扑击)
    private bool isLeaping = false;       
    private bool hasDealtDamage = false;  

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        leftLimit = transform.position.x - patrolDistance;
        rightLimit = transform.position.x + patrolDistance;
        
        currentState = DogState.Patrolling;
    }

    private void Update()
    {
        if (player == null) return;

        // --- 状态切换逻辑 ---
        if (currentState != DogState.Attacking)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // 1. 判断是否在极近距离 (触发挠击)
            if (distanceToPlayer <= scratchRadius)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                    StartCoroutine(ScratchAttackRoutine());
                else
                    currentState = DogState.WaitingToAttack;
            }
            // 2. 判断是否在中等距离 (触发跳脸扑击)
            else if (distanceToPlayer <= attackRadius)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                    StartCoroutine(JumpAttackRoutine());
                else
                    currentState = DogState.WaitingToAttack;
            }
            // 3. 判断是否在索敌范围内 (触发追击)
            else if (distanceToPlayer <= detectionRadius)
            {
                currentState = DogState.Chasing;
            }
            // 4. 丢失目标 (恢复巡逻)
            else
            {
                currentState = DogState.Patrolling;
            }
        }
    }

    private void FixedUpdate()
    {
        if (currentState == DogState.Patrolling)
        {
            Patrol();
        }
        else if (currentState == DogState.Chasing)
        {
            Chase();
        }
        else if (currentState == DogState.WaitingToAttack)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            int dirX = player.position.x > transform.position.x ? 1 : -1;
            FlipSprite(dirX); 
        }
    }

    private void Patrol()
    {
        if (isIdling)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            idleTimer += Time.fixedDeltaTime;
            if (idleTimer >= idleTime)
            {
                isIdling = false;
                idleTimer = 0f;
                patrolDirection *= -1; 
            }
            return;
        }

        rb.velocity = new Vector2(patrolDirection * patrolSpeed, rb.velocity.y);
        FlipSprite(patrolDirection); 

        if ((transform.position.x >= rightLimit && patrolDirection == 1) || 
            (transform.position.x <= leftLimit && patrolDirection == -1))
        {
            isIdling = true;
        }
    }

    private void Chase()
    {
        int dirX = player.position.x > transform.position.x ? 1 : -1;
        rb.velocity = new Vector2(dirX * chaseSpeed, rb.velocity.y);
        FlipSprite(dirX); 
    }

    // ================= 攻击行为 A：抛物线扑击 =================
    private IEnumerator JumpAttackRoutine()
    {
        currentState = DogState.Attacking; 
        
        rb.velocity = new Vector2(0, rb.velocity.y); 
        int dirX = player.position.x > transform.position.x ? 1 : -1;
        FlipSprite(dirX); 
        
        yield return new WaitForSeconds(pouncePreparationTime);

        isLeaping = true; 
        hasDealtDamage = false; 
        
        rb.velocity = new Vector2(dirX * pounceForceX, pounceForceY);

        yield return new WaitForSeconds(0.6f); 

        isLeaping = false;
        
        lastAttackTime = Time.time;
        currentState = DogState.WaitingToAttack; 
    }

    // ================= 攻击行为 B：近战爪子挠击 =================
    private IEnumerator ScratchAttackRoutine()
    {
        currentState = DogState.Attacking;

        // 1. 原地停下，面朝玩家
        rb.velocity = new Vector2(0, rb.velocity.y);
        int dirX = player.position.x > transform.position.x ? 1 : -1;
        FlipSprite(dirX);

        // 2. 挠击前摇 (通常比扑击快)
        yield return new WaitForSeconds(scratchPreparationTime);

        // 3. 瞬间伤害判定：在小狗正前方生成一个虚拟的圆形碰撞范围进行检测
        Vector2 attackPoint = new Vector2(transform.position.x + (dirX * scratchHitboxOffset), transform.position.y);
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint, scratchHitboxRadius);

        foreach (Collider2D hit in hitObjects)
        {
            if (hit.CompareTag("Player"))
            {
                HealthPoint playerHP = hit.GetComponent<HealthPoint>();
                if (playerHP != null)
                {
                    playerHP.TakeDamage(attackDamage);
                }
                break; // 打到一次就跳出循环
            }
        }

        // 4. 攻击动作的后摇停留时间
        yield return new WaitForSeconds(0.3f);

        // 5. 结束攻击
        lastAttackTime = Time.time;
        currentState = DogState.WaitingToAttack;
    }


    // --- 物理碰撞检测 (仅用于扑击时的动态碰撞) ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamageOnLeap(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDealDamageOnLeap(collision.gameObject);
    }

    private void TryDealDamageOnLeap(GameObject targetObj)
    {
        if (targetObj.CompareTag("Player") && isLeaping && !hasDealtDamage)
        {
            HealthPoint playerHP = targetObj.GetComponent<HealthPoint>();
            if (playerHP != null)
            {
                playerHP.TakeDamage(attackDamage);
                hasDealtDamage = true; 
            }
        }
    }

    // --- 辅助方法 ---
    private void FlipSprite(int directionX)
    {
        if (directionX > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (directionX < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    // --- 编辑器可视化 ---
    private void OnDrawGizmosSelected()
    {
        // 索敌范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 扑击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // 挠击范围 (极近距离)
        Gizmos.color = new Color(1f, 0.5f, 0f); // 橙色
        Gizmos.DrawWireSphere(transform.position, scratchRadius);

        // 画出挠击的伤害判定区 (蓝色小圈)
        Gizmos.color = Color.cyan;
        int dirX = transform.localScale.x > 0 ? 1 : -1; // 根据当前朝向计算
        Vector2 attackPoint = new Vector2(transform.position.x + (dirX * scratchHitboxOffset), transform.position.y);
        Gizmos.DrawWireSphere(attackPoint, scratchHitboxRadius);

        // 巡逻边界
        Gizmos.color = Color.green;
        Vector3 leftPos = new Vector3(transform.position.x - patrolDistance, transform.position.y, transform.position.z);
        Vector3 rightPos = new Vector3(transform.position.x + patrolDistance, transform.position.y, transform.position.z);
        Gizmos.DrawLine(leftPos, rightPos);
    }
}