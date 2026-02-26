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
    public Animator anim; // สร้างตัวแปรไว้เรียกใช้แอนิเมชัน




    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isKnockedBack) return;

        // 1. เดินซ้ายขวา
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 🌟 เพิ่มโค้ดบรรทัดนี้ลงไป เพื่อส่งค่าความเร็วไปให้ Animator 🌟
        // (ใช้ Mathf.Abs เพื่อแปลงค่าติดลบตอนเดินซ้าย ให้กลายเป็นบวกเสมอ อนิเมชันจะได้ทำงานทั้ง 2 ฝั่งครับ)
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        }

        // โค้ดหันหน้าซ้ายขวา (ของเดิม)
        if (horizontalInput > 0) transform.eulerAngles = new Vector3(0, 0, 0);
        else if (horizontalInput < 0) transform.eulerAngles = new Vector3(0, 180f, 0);

        // 2. กระโดด
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // 3. ระบบวาร์ป (กดปุ่ม Left Shift)
        // 3. ระบบวาร์ป (กดปุ่ม Left Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextTeleportTime)
        {
            // เรียกใช้ระบบวาร์ปแบบหน่วงเวลา
            StartCoroutine(TeleportSequence());
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                // สั่งให้เล่นท่า Dash!
                if (anim != null)
                {
                    anim.SetTrigger("Dash");
                }

                // ... โค้ดพุ่งตัว (เพิ่มความเร็ว) เดิมของคุณอาร์ม ...
            }
        }
        
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        
    }

    // ฟังก์ชันวาร์ปของพ่อมด!
    // ฟังก์ชันวาร์ปแบบสมูท (รอจังหวะอนิเมชัน)
    System.Collections.IEnumerator TeleportSequence()
    {
        // 1. เริ่มนับ Cooldown
        nextTeleportTime = Time.time + teleportCooldown;

        // 2. สั่งเล่นท่า Dash (ให้ตัวจางหายไป)
        if (anim != null) anim.SetTrigger("Dash");

        // 3. หยุดตัวละครไม่ให้เดินได้ชั่วคราวตอนกำลังร่ายวาร์ป
        float originalSpeed = moveSpeed;
        moveSpeed = 0f;

        // 4. ⏳ รอเวลาให้ภาพจางจนสุด (ปรับเลข 0.2f ให้ตรงกับความเร็วอนิเมชันคุณอาร์มได้เลย)
        yield return new WaitForSeconds(0.2f);

        // --- 5. เริ่มทำการวาร์ป ---
        Vector2 facingDirection = (transform.eulerAngles.y == 0) ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, teleportDistance, groundLayer);
        Vector2 targetPosition;

        if (hit.collider != null)
        {
            targetPosition = hit.point - (facingDirection * 0.5f);
        }
        else
        {
            targetPosition = (Vector2)transform.position + (facingDirection * teleportDistance);
        }

        // ย้ายตำแหน่งทันที (คนเล่นจะไม่เห็นการวาร์ปขัดตา เพราะภาพมันล่องหนอยู่พอดี!)
        transform.position = targetPosition;
        rb.velocity = new Vector2(0, rb.velocity.y);

        // 6. ⏳ รอให้อนิเมชันกลับมาปรากฏตัวจนจบ ค่อยคืนค่าให้เดินได้ปกติ
        yield return new WaitForSeconds(0.7f);
        moveSpeed = originalSpeed;
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