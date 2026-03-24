using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 17.3f; // ปรับตามความสูง 5 หรือ 6 บล็อกได้เลยครับ

    [Header("Jump UX (ความสมูทระดับโปร)")]
    public float fallMultiplier = 3.5f;
    public float lowJumpMultiplier = 4f;
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // 🌟 เอากลับมาแล้ว! ระบบ Knockback
    [Header("Knockback System")]
    public float knockbackForceX = 10f;
    public float knockbackForceY = 5f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    // 🌟 เอากลับมาแล้ว! ระบบ Teleport
    [Header("Magic Teleport (Blink)")]
    public float teleportDistance = 4f;
    public float teleportCooldown = 1f;
    private float nextTeleportTime = 0f;

    // 🌟 เอากลับมาแล้ว! ตัวแปรอมตะ
    [Header("I-Frames Status")]
    public bool isInvincible = false;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    public Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // ถ้าโดนตีปลิวกระเด็นอยู่ ห้ามขยับตัว
        if (isKnockedBack) return;

        // 1. รับค่าการเดินแนวนอน
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        // หันหน้าซ้ายขวา
        if (horizontalInput > 0) transform.eulerAngles = new Vector3(0, 0, 0);
        else if (horizontalInput < 0) transform.eulerAngles = new Vector3(0, 180f, 0);

        // 2. เช็กพื้น
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 3. ระบบ Coyote Time (เดินพ้นขอบเหวไปนิดนึงก็ยังโดดได้)
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 4. ระบบ Jump Buffer (กดโดดล่วงหน้าได้)
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // 5. คำสั่งกระโดด
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f; // ตัด Buffer ทิ้งเพื่อไม่ให้โดดเบิ้ล
        }

        // 6. ตัดจังหวะลอยค้าง (ปล่อยปุ่มแล้วร่วงทันที)
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            coyoteTimeCounter = 0f; // กันบั๊กโดดเบิ้ล
        }

        // 7. ระบบร่วงตกพื้นแบบมีน้ำหนัก
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        // 8. ระบบวาร์ป (ของเดิม)
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextTeleportTime)
        {
            StartCoroutine(TeleportSequence());
        }
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    // ==========================================
    // 🌟 ระบบวาร์ป และ Knockback ของเดิมของคุณอาร์ม (ห้ามลบ!)
    // ==========================================

    System.Collections.IEnumerator TeleportSequence()
    {
        nextTeleportTime = Time.time + teleportCooldown;
        isInvincible = true;

        if (anim != null) anim.SetTrigger("Dash");

        float originalSpeed = moveSpeed;
        moveSpeed = 0f;

        yield return new WaitForSeconds(0.2f);

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

        transform.position = targetPosition;
        rb.velocity = new Vector2(0, rb.velocity.y);

        moveSpeed = originalSpeed;

        yield return new WaitForSeconds(0.2f);
        isInvincible = false;
    }

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