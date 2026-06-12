using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动属性")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    
    [Header("战斗属性")]
    public int attackDamage = 40;        // 攻击力
    public float attackRange = 0.8f;     // 攻击范围半径
    public float attackOffset = 0.6f;    // 攻击判定点在人物前方的距离
    public string enemyTag = "Enemy";         // 告诉代码谁是敌人

    private Rigidbody2D rb;
    private float horizontalInput;
    private int groundContactCount = 0; 
    private bool isGrounded => groundContactCount > 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 获取输入与转向
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (horizontalInput > 0) 
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0) 
            transform.localScale = new Vector3(-1, 1, 1);

        // 2. 跳跃
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // 3. 攻击指令 (按下 J 键)
        if (Input.GetKeyDown(KeyCode.J))
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision) { groundContactCount++; }
    void OnCollisionExit2D(Collision2D collision)  { groundContactCount--; }

    // --- 战斗核心逻辑 ---
    void Attack()
    {
        // 1. 计算攻击判定的中心点 (玩家当前位置 + 面向方向的偏移)
        // transform.localScale.x 可以判断玩家是朝左(-1)还是朝右(1)
        Vector2 attackCenter = new Vector2(transform.position.x + (transform.localScale.x * attackOffset), transform.position.y);

        // 2. 画一个无形的圆，找出圆内所有位于 enemyLayer 的碰撞体
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCenter, attackRange);

        // 3. 挨个造成伤害
        foreach (Collider2D enemy in hitEnemies)
        {
            // 找到敌人身上的 EnemySystem 脚本，并调用受伤方法
            enemy.GetComponent<EnemySystem>().TakeDamage2(attackDamage);
        }
    }

    // 在Unity编辑器里画出一个红圈，方便你调攻击范围
    private void OnDrawGizmosSelected()
    {
        Vector2 attackCenter = new Vector2(transform.position.x + (transform.localScale.x * attackOffset), transform.position.y);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCenter, attackRange);
    }
}