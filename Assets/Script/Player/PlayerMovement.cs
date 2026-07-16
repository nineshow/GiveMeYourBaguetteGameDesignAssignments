using UnityEngine;
using UnityEngine.UI; // 🎯 必須引入UI命名空間以控制進度條

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 10f;

    [Header("Glide")]
    public float normalGravity = 3f;
    public float glideGravity = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("Audio Settings")]
    public AudioSource audioSource; // 播放声音的“扬声器”
    public AudioClip jumpSound;     // 跳跃音效
    public AudioClip dashSound;     // 冲刺音效

    // 🎯【新增】：衝刺 UI 設置面板
    [Header("Dash UI Settings")]
    public Image dashBarFill;       // 衝刺冷卻進度條 (Image Type 必須是 Filled)
    public GameObject dashGlowLayer; // 衝刺就緒時的發光圖層
    public float dashCooldown = 1.0f; // 衝刺冷卻時間（對齊你原本協程底部的 1.0f）
    private float dashTimer = 1.0f;   // 衝刺計時器，預設為滿值代表開局可用

    private Rigidbody2D rb;
    private bool isGrounded;

    public int maxJumps = 2;
    private int jumpsRemaining;

    private bool standing;

    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;

    private bool isDashing;
    private int facingDirection = 1;
    private bool canDash = true;

    private bool isGliding;
    private bool isTakingDamage;

    private Animator anim; 
    private PlayerCombat combatScript; 

    [Header("Player Map Bound")]
    [SerializeField] private SpriteRenderer mapSprite;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private bool clampY = false;

    private float minX;
    private float maxX;
    private float minY;
    private float maxY;
    private bool boundsCalculated;

    System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        dashTimer = 0f; // 🎯【新增】：衝刺觸發，冷卻計時器歸零，UI條會瞬間變空

        if (anim != null) anim.SetBool("isDashing", true);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.velocity = new Vector2(
            facingDirection * dashSpeed,
            0f
        );

        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = originalGravity;
        isDashing = false;

        if (anim != null) anim.SetBool("isDashing", false);
        
        // 🎯 注意：原本這裡有 yield return new WaitForSeconds(1.0f); 
        // 為了讓冷卻計時更精準流暢，冷卻時間交由 Update 中的 dashTimer 接管，此處移除死等
    }

    System.Collections.IEnumerator Hurt()
    {
        isTakingDamage = true;
        yield return new WaitForSeconds(0.5f);
        isTakingDamage = false; 
    }

    public void isDamage()
    {
        if (anim != null) 
        {
            anim.SetTrigger("Hurt");
        }
        StartCoroutine(Hurt());
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining = maxJumps;
        anim = GetComponentInChildren<Animator>();
        combatScript = GetComponent<PlayerCombat>();
        
        // 🎯 遊戲開始時初始化一次 UI
        dashTimer = dashCooldown;
        UpdateDashUI();
    }

    void Update()
    {
        // 🎯【新增】：即時計算衝刺冷卻時間
        if (!canDash)
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashCooldown)
            {
                dashTimer = dashCooldown;
                canDash = true; // 冷卻完畢，解鎖衝刺
            }
        }
        
        // 🎯【新增】：每幀同步刷新 Dash UI 的進度與發光狀態
        UpdateDashUI();

        // ==========================================
        // 第一步：不管什么状态，永远优先检测是否落地！
        // ==========================================
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            0.2f,
            groundLayer
        );

        if(isGrounded && !standing)
        {
            jumpsRemaining = maxJumps;
            // 🎯 原本這裡有 canDash = true; 
            // 為了不破壞你「落地刷新衝刺」的原始機制，這裡加上計時器回滿，讓 UI 瞬間亮起
            if (!canDash)
            {
                canDash = true;
                dashTimer = dashCooldown;
            }
        }
        standing = isGrounded;

        // 实时同步基础动画状态
        if (anim != null) 
        {
            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("yVelocity", rb.velocity.y);
        }

        // ==========================================
        // 第二步：特殊状态拦截（冲刺、挨打、防御）
        // ==========================================
        if(isDashing) 
        {
            return; 
        }

        if(isTakingDamage || (combatScript != null && combatScript.isDefending))
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            rb.gravityScale = normalGravity;
            isGliding = false;

            if (anim != null)
            {
                anim.SetBool("isRunning", false);
                anim.SetBool("isGliding", false);
            }
            return; 
        }

        // ==========================================
        // 第三步：正常的移动、跳跃、滑翔逻辑
        // ==========================================
        float moveInput = Input.GetAxisRaw("Horizontal");

        if(moveInput > 0)
        {
            facingDirection = 1;
            transform.localScale = new Vector3(1, 1, 1); 
        }
        else if(moveInput < 0)
        {
            facingDirection = -1;
            transform.localScale = new Vector3(-1, 1, 1); 
        }

        rb.velocity = new Vector2(
            moveInput * moveSpeed,
            rb.velocity.y
        );

        if (anim != null)
        {
            bool isMoving = Mathf.Abs(moveInput) > 0f;
            anim.SetBool("isRunning", isMoving);
        }

        // 跳跃检测
        if(Input.GetKeyDown(KeyCode.Space) && jumpsRemaining > 0)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                jumpForce
            );
            jumpsRemaining--;

            if (anim != null) anim.SetTrigger("Jump");

            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }

        // 滑翔检测
        if(Input.GetKey(KeyCode.L) && rb.velocity.y < 0 && !isGrounded)
        {
            rb.gravityScale = glideGravity;
            isGliding = true;
        }
        else
        {
            rb.gravityScale = normalGravity;
            isGliding = false;
        }
        
        if (anim != null) anim.SetBool("isGliding", isGliding);

        // 冲刺检测
        if(Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && canDash)
        {
            canDash = false;

            if (audioSource != null && dashSound != null)
            {
                audioSource.PlayOneShot(dashSound);
            }

            StartCoroutine(Dash());
        }
    }

    // 🎯【新增】：更新衝刺 UI 進度與發光圖層的方法
    private void UpdateDashUI()
    {
        if (dashBarFill != null)
        {
            dashBarFill.fillAmount = dashTimer / dashCooldown;
        }

        if (dashGlowLayer != null)
        {
            // 當計時器大於等於冷卻時間（即衝刺就緒）時，開啟發光圖層，否則關閉
            if (dashTimer >= dashCooldown)
            {
                dashGlowLayer.SetActive(true);
            }
            else
            {
                dashGlowLayer.SetActive(false);
            }
        }
    }

    private void LateUpdate()
    {
        if (!boundsCalculated)
        {
            CalculatePlayerBounds();
        }
        KeepPlayerInsideMap();
    }

    private void CalculatePlayerBounds()
    {
        if (mapSprite == null) return;

        if (playerSprite == null)
        {
            playerSprite = GetComponentInChildren<SpriteRenderer>();
        }

        Bounds mapBounds = mapSprite.bounds;
        float playerHalfWidth = 0f;
        float playerHalfHeight = 0f;

        if (playerSprite != null)
        {
            playerHalfWidth = playerSprite.bounds.extents.x;
            playerHalfHeight = playerSprite.bounds.extents.y;
        }

        minX = mapBounds.min.x + playerHalfWidth;
        maxX = mapBounds.max.x - playerHalfWidth;
        minY = mapBounds.min.y + playerHalfHeight;
        maxY = mapBounds.max.y - playerHalfHeight;

        boundsCalculated = true;
    }

    private void KeepPlayerInsideMap()
    {
        if (mapSprite == null) return;

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = transform.position.y;

        if (clampY)
        {
            clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        }

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}