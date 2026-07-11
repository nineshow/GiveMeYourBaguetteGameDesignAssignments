using System.Collections;
using UnityEngine;

// 确保挂载此脚本的物体会自动添加刚体和音频源组件
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))] 
public class DogAI : MonoBehaviour
{
    public enum DogState { Patrolling, Chasing, WaitingToAttack, Attacking }
    public DogState currentState;

    [Header("目标引用")]
    public Transform player;

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

    [Header("扑击设置")]
    public float pounceForceX = 5f;    
    public float pounceForceY = 6f;    
    public float pouncePreparationTime = 0.3f; 

    [Header("挠击设置")]
    public float scratchPreparationTime = 0.2f; 
    public float scratchHitboxOffset = 0.5f;    
    public float scratchHitboxRadius = 0.4f;    

    [Header("公共攻击设置")]
    public float attackCooldown = 2f;  
    private float lastAttackTime;

    [Header("音效设置")]
    public AudioClip pounceSound;   // 扑击音效
    public AudioClip scratchSound;  // 挠击音效
    private AudioSource audioSource;
    
    private bool isLeaping = false;       
    private bool hasDealtDamage = false;  

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 获取音频播放器，并设置为不自动播放
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

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

        if (currentState != DogState.Attacking)
        {
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

    private IEnumerator JumpAttackRoutine()
    {
        currentState = DogState.Attacking; 
        
        rb.velocity = new Vector2(0, rb.velocity.y); 
        int dirX = player.position.x > transform.position.x ? 1 : -1;
        FlipSprite(dirX); 
        
        yield return new WaitForSeconds(pouncePreparationTime);

        // 【新增】：播放扑击音效
        if (pounceSound != null)
        {
            audioSource.PlayOneShot(pounceSound);
        }

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

        yield return new WaitForSeconds(scratchPreparationTime);

        // 【新增】：播放挠击音效
        if (scratchSound != null)
        {
            audioSource.PlayOneShot(scratchSound);
        }

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

        Gizmos.color = Color.green;
        Vector3 leftPos = new Vector3(transform.position.x - patrolDistance, transform.position.y, transform.position.z);
        Vector3 rightPos = new Vector3(transform.position.x + patrolDistance, transform.position.y, transform.position.z);
        Gizmos.DrawLine(leftPos, rightPos);
    }
}