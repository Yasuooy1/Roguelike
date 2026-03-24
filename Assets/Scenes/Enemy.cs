using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Movement & AI")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.5f;
    public float detectRange = 5f;

    public Transform edgeCheck;
    public float edgeCheckDistance = 1f;
    public LayerMask groundLayer;

    [Header("Attack System")]
    public float attackRange = 3f;
    public float dashForce = 12f;
    public float jumpForce = 15f;
    public float dashTime = 0.35f;
    public float attackCooldown = 2f;

    private bool isAttacking = false;
    private bool isOnCooldown = false;

    private Transform player;
    private Rigidbody2D rb;
    private bool isChasing = false;
    private bool canMoveForward = true;

    [Header("Health & Armor")]
    public int maxHealth = 30;
    private int currentHealth;

    public bool isBroken = false;
    public float breakDuration = 3f;

    [Header("🧩 Puzzle System (รหัสผ่านเกราะ)")]
    public string requiredRecipe;
    private SpriteRenderer spriteRenderer;

    [Header("UI ลูกแก้วบนหัวมอนสเตอร์")]
    public GameObject puzzleCanvas;
    public Image[] puzzleSlots;
    public Sprite fireSprite;
    public Sprite waterSprite;
    public Sprite lightningSprite;

    [Header("UI Damage")]
    public GameObject damagePopupPrefab;

    [Header("Drop System")]
    public GameObject[] dropItems;
    [Range(0, 100)] public int dropChance = 30;
    public GameObject soulPrefab;
    public int soulAmount = 3;

    private bool isDead = false;
    private Animator anim;

    // 🌟 ตัวแปรใหม่ เอาไว้จำว่าตัวเองอยู่ในเวฟไหน และเกิดเป็นคิวที่เท่าไหร่
    private int currentWaveMode = 0;
    private int mySpawnIndex = 0;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        patrolSpeed = Random.Range(1.2f, 2.0f);

        // ตอนเริ่มเกมก็ให้มันสร้างเกราะตามเวฟของตัวเอง
        GenerateRandomPuzzle();
    }

    // ==========================================
    // 🌟 1. ฟังก์ชันรับคำสั่งจาก WaveManager (รับค่าคิวมาด้วย)
    // ==========================================
    public void SetTutorialWave(int waveNumber, int spawnIndex)
    {
        currentWaveMode = waveNumber;
        mySpawnIndex = spawnIndex; // จำคิวตัวเองไว้
        GenerateRandomPuzzle();
    }

    // ==========================================
    // 🌟 2. อัปเกรดระบบสุ่มรหัส (ล็อกคิวสำหรับเวฟ 1)
    // ==========================================
    void GenerateRandomPuzzle()
    {
        // ลำดับธาตุ: 0=ไฟ(แดง), 1=สายฟ้า(เหลือง), 2=น้ำ(ฟ้า)
        string[] elements = { "Fire", "Lightning", "Water" };
        List<string> puzzleList = new List<string>();

        if (currentWaveMode == 1)
        {
            // เวฟ 1: สีเดียวล้วน และ ล็อกสีตามคิวการเกิดเป๊ะๆ!
            string singleElement = elements[mySpawnIndex % 3];
            puzzleList.Add(singleElement);
            puzzleList.Add(singleElement);
            puzzleList.Add(singleElement);
        }
        else if (currentWaveMode == 2)
        {
            // เวฟ 2: สองสีผสมกัน (เช่น ฟ้า ฟ้า แดง)
            string elementA = elements[Random.Range(0, 3)];
            string elementB;
            do { elementB = elements[Random.Range(0, 3)]; } while (elementB == elementA);

            puzzleList.Add(elementA);
            puzzleList.Add(elementA);
            puzzleList.Add(elementB);
        }
        else
        {
            // เวฟ 3 ขึ้นไป: สุ่มมั่ว 3 ลูกตามปกติ
            string[] allElements = { "Fire", "Water", "Lightning" };
            puzzleList.Add(allElements[Random.Range(0, 3)]);
            puzzleList.Add(allElements[Random.Range(0, 3)]);
            puzzleList.Add(allElements[Random.Range(0, 3)]);
        }

        // เรียงตัวอักษร A-Z ให้ตรงกับระบบ
        puzzleList.Sort();
        requiredRecipe = puzzleList[0] + puzzleList[1] + puzzleList[2];

        // อัปเดตรูปลูกแก้วบนหัวมอนสเตอร์
        for (int i = 0; i < puzzleSlots.Length; i++)
        {
            if (puzzleList[i] == "Fire") puzzleSlots[i].sprite = fireSprite;
            else if (puzzleList[i] == "Water") puzzleSlots[i].sprite = waterSprite;
            else if (puzzleList[i] == "Lightning") puzzleSlots[i].sprite = lightningSprite;
        }

        if (puzzleCanvas != null) puzzleCanvas.SetActive(true);
    }

    void Update()
    {
        if (isBroken || isAttacking || isDead) return;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            return;
        }

        RaycastHit2D groundInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, edgeCheckDistance, groundLayer);
        RaycastHit2D wallInfo = Physics2D.Raycast(edgeCheck.position, transform.right, 0.2f, groundLayer);

        canMoveForward = (groundInfo.collider != null && wallInfo.collider == null);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && !isOnCooldown)
        {
            StartCoroutine(AttackRoutine());
        }
        else if (distanceToPlayer <= detectRange)
        {
            isChasing = true;
            if (player.position.x > transform.position.x)
                transform.eulerAngles = new Vector3(0, 0, 0);
            else
                transform.eulerAngles = new Vector3(0, 180f, 0);
        }
        else
        {
            isChasing = false;
            if (!canMoveForward)
            {
                if (transform.eulerAngles.y == 0) transform.eulerAngles = new Vector3(0, 180f, 0);
                else transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }
    }

    void FixedUpdate()
    {
        if (isBroken || isAttacking || isDead) return;

        if (isChasing && !canMoveForward)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        float currentSpeed = isChasing ? chaseSpeed : patrolSpeed;
        rb.velocity = new Vector2(transform.right.x * currentSpeed, rb.velocity.y);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        isOnCooldown = true;

        rb.velocity = new Vector2(0, rb.velocity.y);
        transform.localScale = new Vector3(transform.localScale.x, 0.8f, 1f);
        yield return new WaitForSeconds(0.4f);

        if (isDead) yield break;

        transform.localScale = new Vector3(transform.localScale.x, 1f, 1f);
        float dashDirection = (transform.eulerAngles.y == 0) ? 1f : -1f;

        float currentJumpForce = rb.velocity.y;
        if (player != null && player.position.y > transform.position.y + 1f)
        {
            currentJumpForce = jumpForce;
        }

        rb.velocity = new Vector2(dashDirection * dashForce, currentJumpForce);
        yield return new WaitForSeconds(dashTime);

        rb.velocity = new Vector2(0, rb.velocity.y);
        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        isOnCooldown = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBroken && !isDead)
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

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (isBroken)
        {
            currentHealth -= damage;
            ShowDamagePopup(damage, Color.white, 4f);
            if (currentHealth <= 0) Die();
        }
        else
        {
            ShowDamagePopup(0, Color.gray, 3f);
        }
    }

    public void CheckPuzzleBullet(string playerRecipe, int damage)
    {
        if (isBroken || isDead) return;

        if (playerRecipe == requiredRecipe)
        {
            BreakArmor();
            TakeDamage(damage);
        }
        else
        {
            ShowDamagePopup(0, Color.red, 3f);
        }
    }

    void BreakArmor()
    {
        isBroken = true;
        spriteRenderer.color = Color.gray;

        if (puzzleCanvas != null) puzzleCanvas.SetActive(false);

        StartCoroutine(RecoverShieldRoutine());
    }

    IEnumerator RecoverShieldRoutine()
    {
        yield return new WaitForSeconds(breakDuration);
        if (currentHealth > 0 && !isDead)
        {
            isBroken = false;
            spriteRenderer.color = Color.white;
            GenerateRandomPuzzle();
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

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (puzzleCanvas != null) puzzleCanvas.SetActive(false);

        if (soulPrefab != null)
        {
            for (int i = 0; i < soulAmount; i++)
            {
                GameObject soul = Instantiate(soulPrefab, transform.position, Quaternion.identity);
                Rigidbody2D soulRb = soul.GetComponent<Rigidbody2D>();
                if (soulRb != null)
                {
                    Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f));
                    soulRb.AddForce(randomDir * 5f, ForceMode2D.Impulse);
                }
            }
        }

        if (Random.Range(0, 100) <= dropChance && dropItems.Length > 0)
        {
            int randomItem = Random.Range(0, dropItems.Length);
            Instantiate(dropItems[randomItem], transform.position, Quaternion.identity);
        }

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        rb.velocity = Vector2.zero;
        rb.gravityScale = 3f;

        Destroy(gameObject, 1f);
    }

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