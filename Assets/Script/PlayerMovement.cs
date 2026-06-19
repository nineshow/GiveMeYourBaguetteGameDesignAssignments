using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed=5f;

    [Header("Jump")]
    public float jumpForce=10f;

    [Header("Glide")]
    public float normalGravity=3f;
    public float glideGravity=0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;

    public int maxJumps=2;
    private int jumpsRemaining;

    private bool standing;

    public float dashSpeed=15f;
    public float dashDuration=0.2f;

    private bool isDashing;
    private int facingDirection=1;
    private bool canDash=true;

    private bool isGliding;
    private bool isTakingDamage;

    private Animator anim; 
    private PlayerCombat combatScript; 

    System.Collections.IEnumerator Dash()
    {
        isDashing=true;

        if (anim != null) anim.SetBool("isDashing", true);

        float originalGravity=rb.gravityScale;
        rb.gravityScale=0f;

        rb.velocity=new Vector2(
            facingDirection*dashSpeed,
            0f
        );

        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale=originalGravity;
        isDashing=false;

        if (anim != null) anim.SetBool("isDashing", false);
    }

    System.Collections.IEnumerator Hurt()
    {
        isTakingDamage=true;
        yield return new WaitForSeconds(0.5f);
        isTakingDamage=false; 
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
        jumpsRemaining=maxJumps;
        anim = GetComponentInChildren<Animator>();
        combatScript = GetComponent<PlayerCombat>();
    }

    void Update()
    {
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
            jumpsRemaining=maxJumps;
            canDash=true; // 落地恢复冲刺
        }
        standing=isGrounded;

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
            return; // 冲刺状态全权由协程接管，无需干涉
        }

        if(isTakingDamage || (combatScript != null && combatScript.isDefending))
        {
            // 【核心修复】：紧急刹车！把水平速度清零，垂直速度保持（该掉下来还是得掉下来）
            rb.velocity = new Vector2(0f, rb.velocity.y);
            
            // 强行恢复正常重力（防止滑翔时防御导致一直飘在空中）
            rb.gravityScale = normalGravity;
            isGliding = false;

            // 强制关闭奔跑和滑翔的动画开关
            if (anim != null)
            {
                anim.SetBool("isRunning", false);
                anim.SetBool("isGliding", false);
            }

            return; // 刹停之后直接结束这一帧，不再读取按键移动
        }

        // ==========================================
        // 第三步：正常的移动、跳跃、滑翔逻辑
        // ==========================================
        float moveInput = Input.GetAxisRaw("Horizontal");

        if(moveInput>0)
        {
            facingDirection=1;
            transform.localScale = new Vector3(1, 1, 1); 
        }
        else if(moveInput<0)
        {
            facingDirection=-1;
            transform.localScale = new Vector3(-1, 1, 1); 
        }

        // 赋予正常的移动速度
        rb.velocity = new Vector2(
            moveInput * moveSpeed,
            rb.velocity.y
        );

        // 同步跑步动画
        if (anim != null)
        {
            bool isMoving = Mathf.Abs(moveInput) > 0f;
            anim.SetBool("isRunning", isMoving);
        }

        // 跳跃检测
        if(Input.GetKeyDown(KeyCode.Space) && jumpsRemaining>0)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                jumpForce
            );
            jumpsRemaining--;

            if (anim != null) anim.SetTrigger("Jump");
        }

        // 滑翔检测
        if(Input.GetKey(KeyCode.L) && rb.velocity.y < 0 && !isGrounded)
        {
            rb.gravityScale = glideGravity;
            isGliding=true;
        }
        else
        {
            rb.gravityScale = normalGravity;
            isGliding=false;
        }
        
        if (anim != null) anim.SetBool("isGliding", isGliding);

        // 冲刺检测
        if(Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && canDash)
        {
            canDash=false;
            StartCoroutine(Dash());
        }
    }
}