using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Movement & AI")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.5f;
    public float detectRange = 5f;

    [Header("เซนเซอร์ส่องพื้น & กำแพง")]
    public Transform edgeCheck;
    public float edgeCheckDistance = 1f;
    public LayerMask groundLayer;

    [Header("Attack System (พุ่งกระโจน)")]
    public float attackRange = 3f;       // ระยะที่จะเริ่มกระโจน (ปรับให้ไกลขึ้นได้)
    public float dashForce = 12f;        // ความแรงพุ่งไปข้างหน้า (ความไกล)
    public float jumpForce = 15f;        // ความแรงกระโดดขึ้นข้างบน (ความสูง)
    public float dashTime = 0.35f;       // ⏳ ระยะเวลาลอยตัว (ยิ่งนานยิ่งพุ่งไปได้ไกล)
    public float attackCooldown = 2f;

    private bool isAttacking = false;
    private bool isOnCooldown = false;

    private Transform player;
    private Rigidbody2D rb;
    private bool isChasing = false;

    // 🛑 เพิ่มตัวแปรความจำว่าข้างหน้าไปต่อได้ไหม
    private bool canMoveForward = true;

    [Header("Health & Shield")]
    public int maxHealth = 30;
    private int currentHealth;
    public int maxShield = 10;
    private int currentShield;

    [Header("Break System")]
    public bool isBroken = false;
    public float breakDuration = 3f;

    [Header("Element Setup")]
    public PlayerCombat.Element enemyElement;
    private SpriteRenderer spriteRenderer;

    [Header("UI")]
    public GameObject damagePopupPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // สุ่มความเร็วตอนเกิด มอนสเตอร์จะได้ไม่เดินซ้อนทับกันเป็นก้อนเดียว
        patrolSpeed = Random.Range(1.2f, 2.0f);

        UpdateColor();
    }

    void Update()
    {
        if (isBroken || isAttacking) return;

        // ระบบค้นหาผู้เล่นแบบปลอดภัย
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            return;
        }

        // 🔍 ส่องเลเซอร์ตลอดเวลา ไม่ว่าจะเดินเล่นหรือวิ่งไล่!
        RaycastHit2D groundInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, edgeCheckDistance, groundLayer);
        RaycastHit2D wallInfo = Physics2D.Raycast(edgeCheck.position, transform.right, 0.2f, groundLayer);

        // อัปเดตความจำว่าข้างหน้ามีพื้นให้เหยียบ และไม่ติดกำแพง
        canMoveForward = (groundInfo.collider != null && wallInfo.collider == null);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- ระบบพุ่งโจมตี (Lunge Attack) ---
        if (distanceToPlayer <= attackRange && !isOnCooldown)
        {
            StartCoroutine(AttackRoutine());
        }
        // --- ระบบวิ่งไล่ (Chase) ---
        else if (distanceToPlayer <= detectRange)
        {
            isChasing = true;
            if (player.position.x > transform.position.x)
                transform.eulerAngles = new Vector3(0, 0, 0);
            else
                transform.eulerAngles = new Vector3(0, 180f, 0);
        }
        // --- ระบบเดินเล่น (Patrol) ---
        else
        {
            isChasing = false;
            // 🛑 ถ้าเดินลาดตระเวนอยู่แล้วเจอเหว ให้หมุนตัวหันหลังกลับ
            if (!canMoveForward)
            {
                if (transform.eulerAngles.y == 0) transform.eulerAngles = new Vector3(0, 180f, 0);
                else transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }
    }

    void FixedUpdate()
    {
        // ถ้าพุ่งโจมตีอยู่ หรือเกราะแตก ปล่อยให้มันลอยไปตามฟิสิกส์
        if (isBroken || isAttacking) return;

        // 🛑 ถ้าวิ่งไล่ผู้เล่นอยู่ แต่ข้างหน้าเป็นเหวหรือกำแพง ให้ "เบรกเอี๊ยด" รอที่ขอบเหว!
        if (isChasing && !canMoveForward)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return; // หยุดการทำงานเดินหน้าไปเลย
        }

        float currentSpeed = isChasing ? chaseSpeed : patrolSpeed;
        rb.velocity = new Vector2(transform.right.x * currentSpeed, rb.velocity.y);
    }

    // --- ท่ากระโจน (มีระบบ Anti-Camp กระโดดตะปบคนบนแท่น) ---
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        isOnCooldown = true;

        // 1. ย่อตัวชาร์จ
        rb.velocity = new Vector2(0, rb.velocity.y);
        transform.localScale = new Vector3(transform.localScale.x, 0.8f, 1f);
        yield return new WaitForSeconds(0.4f);

        // 2. กระโจน!
        transform.localScale = new Vector3(transform.localScale.x, 1f, 1f);
        float dashDirection = (transform.eulerAngles.y == 0) ? 1f : -1f;

        float currentJumpForce = rb.velocity.y; // แรงโน้มถ่วงปกติ

        // ถ้าผู้เล่นอยู่สูงกว่า ให้ใช้แรง jumpForce ที่ตั้งไว้
        if (player != null && player.position.y > transform.position.y + 1f)
        {
            currentJumpForce = jumpForce;
        }

        // อัดแรงส่งตัว
        rb.velocity = new Vector2(dashDirection * dashForce, currentJumpForce);

        // ⏳ ใช้เวลาลอยตัวตามที่เราตั้งค่าไว้ในหน้าต่าง Inspector
        yield return new WaitForSeconds(dashTime);

        // 3. เบรกกลางอากาศ
        rb.velocity = new Vector2(0, rb.velocity.y);
        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        isOnCooldown = false;
    }

    // ==========================================
    // ระบบโดนตีและเกราะแตก (เหมือนเดิมเป๊ะ)
    // ==========================================
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBroken)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

            if (playerHealth != null && playerController != null)
            {
                playerController.Knockback(transform);
                playerHealth.TakeDamage(1);
            }
        }
    }

    void UpdateColor()
    {
        if (isBroken) { spriteRenderer.color = Color.gray; return; }
        switch (enemyElement)
        {
            case PlayerCombat.Element.Red: spriteRenderer.color = Color.red; break;
            case PlayerCombat.Element.Green: spriteRenderer.color = Color.green; break;
            case PlayerCombat.Element.Blue: spriteRenderer.color = Color.blue; break;
        }
    }

    public void TakeDamage(int damage, PlayerCombat.Element hitElement)
    {
        if (isBroken)
        {
            currentHealth -= damage;
            ShowDamagePopup(damage, Color.white, 4f);
            if (currentHealth <= 0) Die();
        }
        else
        {
            bool isWeakness = false;
            if (hitElement == PlayerCombat.Element.Red && enemyElement == PlayerCombat.Element.Green) isWeakness = true;
            else if (hitElement == PlayerCombat.Element.Green && enemyElement == PlayerCombat.Element.Blue) isWeakness = true;
            else if (hitElement == PlayerCombat.Element.Blue && enemyElement == PlayerCombat.Element.Red) isWeakness = true;

            if (isWeakness)
            {
                currentShield -= damage;
                ShowDamagePopup(damage, Color.yellow, 5f);
                if (currentShield <= 0) BreakArmor();
            }
            else ShowDamagePopup(0, Color.gray, 3f);
        }
    }

    void BreakArmor()
    {
        isBroken = true;
        UpdateColor();
        StartCoroutine(RecoverShieldRoutine());
    }

    IEnumerator RecoverShieldRoutine()
    {
        yield return new WaitForSeconds(breakDuration);
        if (currentHealth > 0)
        {
            isBroken = false;
            currentShield = maxShield;
            UpdateColor();
        }
    }

    void ShowDamagePopup(int damageAmount, Color textColor, float fontSize)
    {
        if (damagePopupPrefab != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, 0);
            GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
            popup.GetComponent<DamagePopup>().SetupCustom(damageAmount, textColor, fontSize);
        }
    }

    void Die() { Destroy(gameObject); }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (edgeCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(edgeCheck.position, edgeCheck.position + Vector3.down * edgeCheckDistance);
            Gizmos.DrawLine(edgeCheck.position, edgeCheck.position + transform.right * 0.2f);
        }
    }
}