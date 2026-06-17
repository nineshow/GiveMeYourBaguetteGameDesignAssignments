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
    public bool isTakingDamage;

    // IEnumerator for Coroutine function
    // pause other, run this, then resume
    System.Collections.IEnumerator Dash()
    {
        isDashing=true;

        //save original gravity so can resume later
        float originalGravity=rb.gravityScale;
        //disable gravity so no falling while dashing
        rb.gravityScale=0f;

        //dash for the speed set towards the direction
        //0f so no falling
        rb.velocity=new Vector2(
            facingDirection*dashSpeed,
            0f
        );

        //pause and wait for sometime for dash to finish
        yield return new WaitForSeconds(dashDuration);
        //restore gravity
        rb.gravityScale=originalGravity;
        //dashing end
        isDashing=false;
    }

    System.Collections.IEnumerator Hurt()
    {
        isTakingDamage=true;

        yield return new WaitForSeconds(0.5);
        
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
    }

   

    void Update()
    {
        // if dashing disable movement
        if(isDashing)
        {
            return;
        }


        // Ground Detection
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            0.2f,
            groundLayer
        );

        // Reset jump when touch ground
        // Do not repeat if is just standing
        if(isGrounded && !standing)
        {
            jumpsRemaining=maxJumps;
        }
        // if is Grounded, must be standing too
        standing=isGrounded;

        // Horizontal Movement
        float moveInput = Input.GetAxisRaw("Horizontal");

        // facing right
        if(moveInput>0)
        {
            facingDirection=1;
        }
        // facing left
        else if(moveInput<0)
        {
            facingDirection=-1;
        }

        rb.velocity = new Vector2(
            moveInput * moveSpeed,
            rb.velocity.y
        );

        // Jump when space key + still have remaining jump
        if(Input.GetKeyDown(KeyCode.Space) && jumpsRemaining>0)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                jumpForce
            );

            jumpsRemaining--;
        }


        // Glide when holding L
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
        
        // reset dash and glide when touching ground
        if(isGrounded)
        {
            canDash=true;
            isGliding=false;
        }

        // dash if left shift pressed and is not currently dashing
        // canDash check for limiting only one dash while jumping
        if(Input.GetKeyDown(KeyCode.LeftShift) 
        && !isDashing
        && canDash)
        {
            canDash=false;
            // start Dash() as a coroutine
            StartCoroutine(Dash());
        }

    }
}