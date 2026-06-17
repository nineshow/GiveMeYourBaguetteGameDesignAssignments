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

    // 【新增 1】：声明动画控制器
    private Animator anim; 

    // IEnumerator for Coroutine function
    System.Collections.IEnumerator Dash()
    {
        isDashing=true;

        // 【新增 2】：告诉动画机开始冲刺
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

        // 【新增 3】：告诉动画机结束冲刺
        if (anim != null) anim.SetBool("isDashing", false);
    }

    System.Collections.IEnumerator Hurt()
    {
        isTakingDamage=true;

        yield return new WaitForSeconds(0.5f);
        
        isDashing=false;
    }

    public void isDamage()
    {
        StartCoroutine(Hurt());
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsRemaining=maxJumps;

        // 【新增 4】：获取动画组件
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 动画同步：每帧将落地状态同步给动画机（方便做空中/落地判定）
        if (anim != null) anim.SetBool("isGrounded", isGrounded);

        if(isDashing)
        {
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            0.2f,
            groundLayer
        );

        if(isGrounded && !standing)
        {
            jumpsRemaining=maxJumps;
        }
        standing=isGrounded;

        float moveInput = Input.GetAxisRaw("Horizontal");

        // 【修改】：加入了 transform.localScale 翻转逻辑，解决太空步问题
        if(moveInput>0)
        {
            facingDirection=1;
            transform.localScale = new Vector3(1, 1, 1); // 面朝右
        }
        else if(moveInput<0)
        {
            facingDirection=-1;
            transform.localScale = new Vector3(-1, 1, 1); // 面朝左
        }

        rb.velocity = new Vector2(
            moveInput * moveSpeed,
            rb.velocity.y
        );

        // 【新增 5】：跑步动画控制
        if (anim != null)
        {
            bool isMoving = Mathf.Abs(moveInput) > 0f;
            anim.SetBool("isRunning", isMoving);
        }

        if(Input.GetKeyDown(KeyCode.Space) && jumpsRemaining>0)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                jumpForce
            );
            jumpsRemaining--;

            // 【新增 6】：触发跳跃动画
            if (anim != null) anim.SetTrigger("Jump");
        }

        if(Input.GetKey(KeyCode.L)
           && rb.velocity.y < 0
           && !isGrounded)
        {
            rb.gravityScale = glideGravity;
            isGliding=true;
        }
        else
        {
            rb.gravityScale = normalGravity;
            isGliding=false;
        }
        
        // 【新增 7】：滑翔动画控制
        if (anim != null) anim.SetBool("isGliding", isGliding);

        if(isGrounded)
        {
            canDash=true;
            isGliding=false;
        }

        if(Input.GetKeyDown(KeyCode.LeftShift) 
        && !isDashing
        && canDash)
        {
            canDash=false;
            StartCoroutine(Dash());
        }
    }
}