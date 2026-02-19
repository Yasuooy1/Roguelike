using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Knockback System")]
    public float knockbackForceX = 10f;
    public float knockbackForceY = 5f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    [Header("Magic Teleport (Blink)")]
    public float teleportDistance = 4f;     // ระยะทางที่จะวาร์ปไป
    public float teleportCooldown = 1f;     // ดีเลย์ก่อนจะวาร์ปครั้งต่อไปได้
    private float nextTeleportTime = 0f;    // ตัวจับเวลา Cooldown

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;

    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isKnockedBack) return;

        // 1. เดินซ้ายขวา
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0) transform.eulerAngles = new Vector3(0, 0, 0);
        else if (horizontalInput < 0) transform.eulerAngles = new Vector3(0, 180f, 0);

        // 2. กระโดด
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // 3. ระบบวาร์ป (กดปุ่ม Left Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextTeleportTime)
        {
            Teleport();
        }
        
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        
    }

    // ฟังก์ชันวาร์ปของพ่อมด!
    void Teleport()
    {
        // เริ่มนับ Cooldown
        nextTeleportTime = Time.time + teleportCooldown;

        // หาทิศทางที่เราหันหน้าอยู่ (หันขวา = 1, หันซ้าย = -1)
        Vector2 facingDirection = (transform.eulerAngles.y == 0) ? Vector2.right : Vector2.left;

        // ยิงเลเซอร์ (Raycast) ออกไปเช็กว่าข้างหน้ามีกำแพง/พื้น ขวางระยะวาร์ปไหม?
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, teleportDistance, groundLayer);

        Vector2 targetPosition;

        if (hit.collider != null)
        {
            // ถ้ามีกำแพงขวาง ให้วาร์ปไปชิดกำแพงแทน (ถอยออกมา 0.5f เพื่อไม่ให้ตัวฝังกำแพง)
            targetPosition = hit.point - (facingDirection * 0.5f);
            Debug.Log("วาร์ปติดกำแพง!");
        }
        else
        {
            // ถ้าทางสะดวก วาร์ปไปเต็มระยะเลย
            targetPosition = (Vector2)transform.position + (facingDirection * teleportDistance);
        }

        // สั่งย้ายตำแหน่งตัวละครทันที (นี่แหละคือการวาร์ป!)
        transform.position = targetPosition;

        // ดรอปความเร็วตกค้าง เพื่อไม่ให้วาร์ปแล้วตัวลื่นไถล
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    // ระบบกระเด็น (ของเดิม)
    public void Knockback(Transform enemyTransform)
    {
        isKnockedBack = true;
        float pushDirection = (transform.position.x < enemyTransform.position.x) ? -1f : 1f;
        rb.velocity = new Vector2(pushDirection * knockbackForceX, knockbackForceY);
        Invoke("ResetKnockback", knockbackDuration);
    }

    void ResetKnockback()
    {
        isKnockedBack = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}