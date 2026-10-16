using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 17.3f;

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

    [Header("Knockback System")]
    public float knockbackForceX = 10f;
    public float knockbackForceY = 5f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    [Header("Magic Teleport (Blink)")]
    public float teleportDistance = 4f;
    public float teleportCooldown = 1f;
    private float nextTeleportTime = 0f;

    [Header("I-Frames Status")]
    public bool isInvincible = false;
    public bool isActionLocked = false;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    public Animator anim;

    [Header("ระบบเสียงเคลื่อนไหว")]
    public AudioSource audioSource;
    public AudioSource walkAudioSource;
    public AudioClip walkSound;
    public AudioClip jumpSound;
    public AudioClip highJumpSound;
    public AudioClip blinkSound;
    public float walkSoundRate = 0.3f;
    private float nextWalkSoundTime = 0f;
    public AudioClip landingSound;
    private bool wasGrounded;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (isActionLocked) return;
        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else if (rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }
    }

    public void OnDash(InputValue value)
    {
        if (isActionLocked) return;

        if (value.isPressed && Time.time >= nextTeleportTime)
        {
            StartCoroutine(TeleportSequence());
        }
    }

    public void OnFire(InputValue value)
    {
        if (value.isPressed && !isKnockedBack)
        {
            // สั่งเล่นท่าสะบัดไม้กายสิทธิ์เท่านั้น (ยังไม่เสกกระสุน)
            if (anim != null) anim.SetTrigger("Attack");
        }
    }

    // ==========================================
    //  ฟังก์ชันเสกกระสุน (เรียกโดย Animation Event)
    // ==========================================
    public void TriggerMagicShoot()
    {
        // โค้ดสำหรับเสกกระสุนของคุณอาร์มจะมาใส่ที่นี่ครับ
        // เช่น Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Debug.Log("เสกกระสุนออกไปในเฟรมที่กำหนด!");
    }

    void Update()
    {
        if (isKnockedBack) return;

        horizontalInput = moveInput.x;
        if (isActionLocked)
        {
            horizontalInput = 0;
        }

        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        if (horizontalInput > 0) transform.eulerAngles = new Vector3(0, 0, 0);
        else if (horizontalInput < 0) transform.eulerAngles = new Vector3(0, 180f, 0);

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

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f;

            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound, 0.6f);
            }
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;
        if (isActionLocked)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    System.Collections.IEnumerator TeleportSequence()
    {
        nextTeleportTime = Time.time + teleportCooldown;
        isInvincible = true;

        if (anim != null) anim.SetTrigger("Dash");

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
    public void LockMovement(float duration)
    {
        StartCoroutine(LockMovementRoutine(duration));
    }

    private System.Collections.IEnumerator LockMovementRoutine(float duration)
    {
        isActionLocked = true;
        yield return new WaitForSeconds(duration);
        isActionLocked = false;
    }
}