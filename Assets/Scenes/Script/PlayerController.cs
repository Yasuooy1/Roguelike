using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("ระบบเสียงเคลื่อนไหว")]
    public AudioSource audioSource;  // ลาก Player มาใส่ช่องนี้
    public AudioSource walkAudioSource;
    public AudioClip walkSound;      // เสียงเดิน
    public AudioClip jumpSound;      // เสียงกระโดดปกติ
    public AudioClip highJumpSound;  // เสียงกระโดดสูง (รอใส่ตอนทำแท่นกระโดด)
    public AudioClip blinkSound;     // เสียงพุ่ง/บลิ๊งค์
    public float walkSoundRate = 0.3f; // ปรับความรัวของก้าวเดินตรงนี้ (ยิ่งน้อยยิ่งรัว)
    private float nextWalkSoundTime = 0f;
    public AudioClip landingSound;   // 🌟 เสียงตอนเท้ากระทบพื้น (ตุ้บ!)
    private bool wasGrounded;        // 🌟 เอาไว้จำว่า "เฟรมที่แล้วลอยอยู่หรือเปล่า?"
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void OnMove(InputValue value)
    {
        // เก็บค่าทิศทางที่กด (X และ Y) ไว้ในตัวแปร
        moveInput = value.Get<Vector2>();
        Debug.Log("ขยับจอยไปที่: " + moveInput);
    }
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime; // ตั้งเวลารอกระโดด
        }
        else if (rb.velocity.y > 0f)
        {
            // ตัดจังหวะลอยค้าง (ปล่อยปุ่มแล้วร่วงทันที)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }
    }
    public void OnDash(InputValue value)
    {
        if (value.isPressed && Time.time >= nextTeleportTime)
        {
            StartCoroutine(TeleportSequence());
        }
    }

    void Update()
    {
        // ถ้าโดนตีปลิวกระเด็นอยู่ ห้ามขยับตัว
        if (isKnockedBack) return;

        // 🌟 1. ดึงค่าจากจอยหรือคีย์บอร์ดมาใช้ (ตัด Input.GetAxisRaw ทิ้ง!)
        horizontalInput = moveInput.x;

        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        // หันหน้าซ้ายขวา
        if (horizontalInput > 0) transform.eulerAngles = new Vector3(0, 0, 0);
        else if (horizontalInput < 0) transform.eulerAngles = new Vector3(0, 180f, 0);

        // 2. เช็กพื้น
        bool currentGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && currentGrounded)
        {
            if (audioSource != null && landingSound != null)
            {
                audioSource.PlayOneShot(landingSound, 0.5f);
            }
        }

        isGrounded = currentGrounded;
        wasGrounded = isGrounded;

        // เสียงเดิน
        if (Mathf.Abs(horizontalInput) > 0.1f && isGrounded)
        {
            if (walkAudioSource != null)
            {
                if (walkAudioSource.clip != walkSound)
                {
                    walkAudioSource.clip = walkSound;
                    walkAudioSource.loop = true;
                }

                if (!walkAudioSource.isPlaying)
                {
                    walkAudioSource.Play();
                }
            }
        }
        else
        {
            if (walkAudioSource != null && walkAudioSource.isPlaying)
            {
                walkAudioSource.Pause();
            }
        }

        // 3. ระบบ Coyote Time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 4. ระบบ Jump Buffer (อัปเดตเวลารอไปเรื่อยๆ)
        jumpBufferCounter -= Time.deltaTime;

        // 5. คำสั่งกระโดด (เช็กเงื่อนไข)
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f;

            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound, 0.6f);
            }
        }

        // 7. ระบบร่วงตกพื้นแบบมีน้ำหนัก
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;
        // ใช้ horizontalInput ที่อัปเดตจากจอย/คีย์บอร์ดแล้ว
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    // ==========================================
    // 🌟 ระบบวาร์ป และ Knockback
    // ==========================================

    System.Collections.IEnumerator TeleportSequence()
    {
        nextTeleportTime = Time.time + teleportCooldown;
        isInvincible = true;

        if (anim != null) anim.SetTrigger("Dash");

        // ==========================================
        // 🌟 แทรกเสียงบลิ๊งค์ (Teleport) ตรงนี้! 
        // ==========================================
        if (audioSource != null && blinkSound != null)
        {
            audioSource.PlayOneShot(blinkSound);
        }

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