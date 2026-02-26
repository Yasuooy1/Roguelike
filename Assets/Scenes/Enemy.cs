using UnityEngine;
using UnityEngine.UI; // 🌟 สำคัญมาก ต้องมีเพื่อใช้คำสั่ง Image และ Canvas
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
    public GameObject puzzleCanvas;   // ตัวปิดเปิด Canvas บนหัว
    public Image[] puzzleSlots;       // ช่องใส่รูป 3 ช่อง (ลาก Orb_1, 2, 3 มาใส่)
    public Sprite fireSprite;         // รูปธาตุไฟ (แดง)
    public Sprite waterSprite;        // รูปธาตุน้ำ (ฟ้า)
    public Sprite lightningSprite;    // รูปธาตุสายฟ้า (เหลือง)

    [Header("UI Damage")]
    public GameObject damagePopupPrefab;

    [Header("Drop System")]
    public GameObject[] dropItems;
    [Range(0, 100)] public int dropChance = 30;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        patrolSpeed = Random.Range(1.2f, 2.0f);

        // สุ่มรหัสผ่านเกราะและแสดงผลบนหัวทันทีตอนเกิด
        GenerateRandomPuzzle();
    }

    // ==========================================
    // 🧩 ฟังก์ชันสุ่มรหัสผ่าน & วาด UI บนหัว
    // ==========================================
    void GenerateRandomPuzzle()
    {
        string[] elements = { "Fire", "Water", "Lightning" };
        List<string> puzzleList = new List<string>();

        // สุ่มมา 3 ลูก
        puzzleList.Add(elements[Random.Range(0, 3)]);
        puzzleList.Add(elements[Random.Range(0, 3)]);
        puzzleList.Add(elements[Random.Range(0, 3)]);

        // เรียงลำดับให้ตรงกัน
        puzzleList.Sort();
        requiredRecipe = puzzleList[0] + puzzleList[1] + puzzleList[2];

        // 🌟 วาดรูปลูกแก้วบนหัวมอนสเตอร์ให้ผู้เล่นเห็น
        for (int i = 0; i < puzzleSlots.Length; i++)
        {
            if (puzzleList[i] == "Fire") puzzleSlots[i].sprite = fireSprite;
            else if (puzzleList[i] == "Water") puzzleSlots[i].sprite = waterSprite;
            else if (puzzleList[i] == "Lightning") puzzleSlots[i].sprite = lightningSprite;
        }

        // เปิด Canvas ให้โชว์
        if (puzzleCanvas != null) puzzleCanvas.SetActive(true);

        Debug.Log(gameObject.name + " รหัสเกราะ: " + requiredRecipe);
    }

    void Update()
    {
        if (isBroken || isAttacking) return;

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
        if (isBroken || isAttacking) return;

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

    // ==========================================
    // 💥 ระบบโดนโจมตี
    // ==========================================
    public void TakeDamage(int damage)
    {
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

    // 🌟 เพิ่ม int damage เข้ามารับค่าจากกระสุน
    public void CheckPuzzleBullet(string playerRecipe, int damage)
    {
        if (isBroken) return;

        if (playerRecipe == requiredRecipe)
        {
            Debug.Log("🎯 รหัสเกราะถูกต้อง! เกราะแตก พร้อมโดนดาเมจ!");

            BreakArmor();          // 1. สั่งเกราะแตก (ตัวซีด)
            TakeDamage(damage);    // 2. 🌟 อัดดาเมจเข้าเลือดต่อทันที! (จะเด้งเลขสีขาวขึ้นมา)
        }
        else
        {
            Debug.Log("❌ รหัสผิด!");
            ShowDamagePopup(0, Color.red, 3f);
        }
    }

    void BreakArmor()
    {
        isBroken = true;
        spriteRenderer.color = Color.gray;

        // 🌟 ปิด UI ลูกแก้วบนหัวทิ้งไปเลย (เพราะเกราะแตกแล้ว)
        if (puzzleCanvas != null) puzzleCanvas.SetActive(false);

        StartCoroutine(RecoverShieldRoutine());
    }

    IEnumerator RecoverShieldRoutine()
    {
        yield return new WaitForSeconds(breakDuration);
        if (currentHealth > 0)
        {
            isBroken = false;
            spriteRenderer.color = Color.white;

            // 🌟 เกราะฟื้นปุ๊บ สุ่มรหัสใหม่ และเปิด UI ลูกแก้วบนหัวใหม่
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
        if (Random.Range(0, 100) <= dropChance && dropItems.Length > 0)
        {
            int randomItem = Random.Range(0, dropItems.Length);
            Instantiate(dropItems[randomItem], transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
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