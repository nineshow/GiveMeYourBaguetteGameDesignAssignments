using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))] 
public class DogAI : MonoBehaviour
{
    // 【新增】：Transforming (变身状态)，变身期间小狗不会移动和攻击
    public enum DogState { Patrolling, Chasing, WaitingToAttack, Attacking, Transforming }
    public DogState currentState;

    [Header("目标与组件引用")]
    public Transform player;
    public Animator anim; 
    private HealthPoint hpTracker; // 用于读取血量脚本里的狂暴状态

    [Header("狂暴形态设置 (二阶段)")]
    public RuntimeAnimatorController normalAnimController; // 第一阶段的普通动画控制器
    public RuntimeAnimatorController rageAnimController;   // 第二阶段的狂暴动画控制器
    public float transformToRageTime = 3f;                 // 进狂暴的动画时间
    public float revertToNormalTime = 2f;                  // 退回普通的动画时间
    private bool currentlyInRageMode = false;              // 本地记录当前的狂暴状态

    [Header("属性设置")]
    public int attackDamage = 10;      

    [Header("范围设置")]
    public float detectionRadius = 5f; 
    public float attackRadius = 2.5f;  
    public float scratchRadius = 0.8f; 

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

    [Header("扑击设置 (抛物线跳脸)")]
    public float pounceForceX = 5f;    
    public float pounceForceY = 6f;    
    public float pouncePreparationTime = 0.3f; 

    [Header("挠击设置 (近战攻击)")]
    public float scratchPreparationTime = 0.2f; 
    public float scratchHitboxOffset = 0.5f;    
    public float scratchHitboxRadius = 0.4f;    

    [Header("公共攻击设置")]
    public float attackCooldown = 2f;  
    private float lastAttackTime;

    [Header("音效设置")]
    public AudioClip pounceSound;   
    public AudioClip scratchSound;  
    
    private AudioSource audioSource;
    private Rigidbody2D rb;

    private bool isLeaping = false;       
    private bool hasDealtDamage = false;  

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        
        hpTracker = GetComponent<HealthPoint>(); // 获取身上的通用血量脚本

        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }

        // 确保一开始使用的是普通动画控制器
        if (anim != null && normalAnimController != null)
        {
            anim.runtimeAnimatorController = normalAnimController;
        }

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
        // 传递速度给 Animator 控制跑步/待机动画
        if (currentState != DogState.Attacking && currentState != DogState.Transforming && anim != null) 
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        }

        if (player == null || hpTracker == null) return;

        // --- 核心：狂暴状态监听与切换 ---
        CheckRageStateChange();

        // 如果正在变身，或者正在攻击，就不执行其他的 AI 寻路逻辑
        if (currentState == DogState.Attacking || currentState == DogState.Transforming) return;

        // --- 正常的 AI 逻辑 ---
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= scratchRadius)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
                StartCoroutine(ScratchAttackRoutine());
            else
                currentState = DogState.WaitingToAttack;
        }
        else if (distanceToPlayer <= attackRadius)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
                StartCoroutine(JumpAttackRoutine());
            else
                currentState = DogState.WaitingToAttack;
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            currentState = DogState.Chasing;
        }
        else
        {
            currentState = DogState.Patrolling;
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
        else if (currentState == DogState.WaitingToAttack || currentState == DogState.Transforming)
        {
            // 变身或者等待攻击时，都会强制停车
            rb.velocity = new Vector2(0, rb.velocity.y);
            
            // 变身时最好也面朝玩家
            int dirX = player.position.x > transform.position.x ? 1 : -1;
            FlipSprite(dirX); 
        }
    }

    // --- 【新增】：检测并执行狂暴变身的逻辑 ---
    private void CheckRageStateChange()
    {
        // 如果 HealthPoint 里进入了狂暴，但本地还没变身
        if (hpTracker.isRageModeActive && !currentlyInRageMode && currentState != DogState.Transforming)
        {
            StartCoroutine(TransformToRageRoutine());
        }
        // 如果 HealthPoint 里的狂暴被打断退出了，但本地还在狂暴状态
        else if (!hpTracker.isRageModeActive && currentlyInRageMode && currentState != DogState.Transforming)
        {
            StartCoroutine(RevertToNormalRoutine());
        }
    }

    // 变身为狂暴形态的协程
    private IEnumerator TransformToRageRoutine()
    {
        currentState = DogState.Transforming;
        rb.velocity = new Vector2(0, rb.velocity.y); // 刹车

        // 1. 播放一阶段的【进入狂暴】动画
        if (anim != null) anim.SetTrigger("EnterRage");

        // 2. 等待 3 秒（变身动画播放的时间）
        yield return new WaitForSeconds(transformToRageTime);

        // 3. 瞬间替换整个动画控制器为第二阶段（贴图和动画逻辑全换）
        if (anim != null && rageAnimController != null)
        {
            anim.runtimeAnimatorController = rageAnimController;
        }

        // 4. 更新状态，满血复活（或者直接追击）
        currentlyInRageMode = true;
        currentState = DogState.Chasing;
    }

    // 退出狂暴形态的协程
    private IEnumerator RevertToNormalRoutine()
    {
        currentState = DogState.Transforming;
        rb.velocity = new Vector2(0, rb.velocity.y); // 刹车

        // 1. 播放二阶段的【退出狂暴】动画被打回原型
        if (anim != null) anim.SetTrigger("ExitRage");

        // 2. 等待退化动画播放完毕
        yield return new WaitForSeconds(revertToNormalTime);

        // 3. 瞬间替换回第一阶段的动画控制器
        if (anim != null && normalAnimController != null)
        {
            anim.runtimeAnimatorController = normalAnimController;
        }

        // 4. 更新状态，恢复普通 AI
        currentlyInRageMode = false;
        currentState = DogState.WaitingToAttack;
    }

    // 以下原有 AI 逻辑保持不变...
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

    private IEnumerator JumpAttackRoutine()
    {
        currentState = DogState.Attacking; 
        rb.velocity = new Vector2(0, rb.velocity.y); 
        int dirX = player.position.x > transform.position.x ? 1 : -1;
        FlipSprite(dirX); 
        
        if (anim != null) anim.SetTrigger("Pounce");

        yield return new WaitForSeconds(pouncePreparationTime);

        if (pounceSound != null) audioSource.PlayOneShot(pounceSound);

        isLeaping = true; 
        hasDealtDamage = false; 
        
        rb.velocity = new Vector2(dirX * pounceForceX, pounceForceY);

        yield return new WaitForSeconds(0.6f); 

        isLeaping = false;
        lastAttackTime = Time.time;
        currentState = DogState.WaitingToAttack; 
    }

    private IEnumerator ScratchAttackRoutine()
    {
        currentState = DogState.Attacking;
        rb.velocity = new Vector2(0, rb.velocity.y);
        int dirX = player.position.x > transform.position.x ? 1 : -1;
        FlipSprite(dirX);

        if (anim != null) anim.SetTrigger("Scratch");

        yield return new WaitForSeconds(scratchPreparationTime);

        if (scratchSound != null) audioSource.PlayOneShot(scratchSound);

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
                break; 
            }
        }

        yield return new WaitForSeconds(0.3f);
        lastAttackTime = Time.time;
        currentState = DogState.WaitingToAttack;
    }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f); 
        Gizmos.DrawWireSphere(transform.position, scratchRadius);
        Gizmos.color = Color.cyan;
        int dirX = transform.localScale.x > 0 ? 1 : -1; 
        Vector2 attackPoint = new Vector2(transform.position.x + (dirX * scratchHitboxOffset), transform.position.y);
        Gizmos.DrawWireSphere(attackPoint, scratchHitboxRadius);
    }
}