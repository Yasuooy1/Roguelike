using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Movement & AI")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.5f;
    public float detectRange = 5f;
    public float changeDirectionTime = 3f;

    [Header("Attack System (พุ่งกระโจน)")]
    public float attackRange = 2f;       // ระยะที่จะเริ่มกระโจนใส่
    public float dashForce = 8f;         // ความแรงตอนพุ่ง
    public float attackCooldown = 2f;    // รอหลังพุ่งเสร็จ

    private bool isAttacking = false;
    private bool isOnCooldown = false;

    private float patrolTimer;
    private Transform player;
    private Rigidbody2D rb;
    private bool isChasing = false;

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

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        patrolTimer = changeDirectionTime;
        UpdateColor();
    }

    void Update()
    {
        // ถ้าเกราะแตก หรือ "กำลังกระโจนโจมตี" ให้หยุดคิดเรื่องเดินปกติไปเลย
        if (isBroken || isAttacking) return;

        if (player == null) return;

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
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                if (transform.eulerAngles.y == 0) transform.eulerAngles = new Vector3(0, 180f, 0);
                else transform.eulerAngles = new Vector3(0, 0, 0);

                patrolTimer = changeDirectionTime;
            }
        }
    }

    void FixedUpdate()
    {
        // ถ้าเกราะแตก หรือ กำลังพุ่งโจมตี ไม่ต้องสั่งเดินปกติ (เพราะตอนพุ่งเราจะใช้แรงส่งแทน)
        if (isBroken || isAttacking) return;

        float currentSpeed = isChasing ? chaseSpeed : patrolSpeed;
        rb.velocity = new Vector2(transform.right.x * currentSpeed, rb.velocity.y);
    }

    // ฟังก์ชันกระโจนโจมตี!
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        isOnCooldown = true;

        // 1. ชะงัก (Wind-up) ให้ผู้เล่นรู้ตัว 0.4 วินาที 
        rb.velocity = new Vector2(0, rb.velocity.y);

        // (กิมมิค: ให้มันย่อตัวลงนิดนึงตอนชาร์จพุ่ง จะได้ดูไม่แข็ง)
        transform.localScale = new Vector3(transform.localScale.x, 0.8f, 1f);
        yield return new WaitForSeconds(0.4f);

        // 2. กระโจน! (Dash) คืนร่างเดิมแล้วพุ่งไปข้างหน้า
        transform.localScale = new Vector3(transform.localScale.x, 1f, 1f);
        float dashDirection = (transform.eulerAngles.y == 0) ? 1f : -1f;
        rb.velocity = new Vector2(dashDirection * dashForce, rb.velocity.y);

        // ปล่อยให้ตัวลอยพุ่งไป 0.2 วินาที
        yield return new WaitForSeconds(0.2f);

        // 3. เบรก หยุดการพุ่ง
        rb.velocity = new Vector2(0, rb.velocity.y);
        isAttacking = false;

        // 4. รอคูลดาวน์ก่อนจะโจมตีรอบต่อไปได้
        yield return new WaitForSeconds(attackCooldown);
        isOnCooldown = false;
    }

    // ==========================================
    // ด้านล่างคือระบบต่อสู้ ตีเกราะแตก และโดนตัว (เหมือนเดิม)
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

    // วาดวงกลมดูระยะการพุ่ง (สีแดง) และระยะสายตา (สีเหลือง)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}