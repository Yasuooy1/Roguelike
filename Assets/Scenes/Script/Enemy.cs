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

    // ==========================================
    // 🌟 ระบบ Tactical AI (มีชั้นเชิง & ฟุตเวิร์ก)
    // ==========================================
    [Header("Tactical AI (มีชั้นเชิง)")]
    public float sweetSpotDistance = 6f; // ระยะคุมเชิงที่มันชอบยืน
    public float retreatDistance = 3f;   // ถ้าระยะใกล้กว่านี้ มันจะพยายามเดินถอยหนี
    private int tacticalAction = 0;      // 1 = เดินหน้าบุก, 0 = หยุดจ้อง, -1 = ถอยหลังฉากหลบ

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

    private float basePatrolSpeed;
    private float baseChaseSpeed;

    [Header("Health & Armor")]
    public int maxHealth = 30;
    private int currentHealth;

    public bool isBroken = false;
    public float breakDuration = 3f;

    [Header("Knockback Effect")]
    public float knockbackForce = 5f;
    public float knockbackUpward = 3f;
    public float knockbackTime = 0.3f;
    private bool isKnockedBack = false;

    [Header("⏳ ระบบ Dynamic Puzzle (โจทย์ดิ้นได้)")]
    public bool isDynamicPuzzle = true;
    public float shuffleTime = 3f;
    private float currentShuffleTime;
    private bool isWarningPuzzle = false;

    [Header("🧩 Puzzle System (ไอคอนธาตุ)")]
    public string requiredRecipe;
    public GameObject puzzleCanvas;
    public Image[] puzzleSlots;
    public Sprite fireSprite;
    public Sprite waterSprite;
    public Sprite lightningSprite;

    private SpriteRenderer spriteRenderer;

    [Header("UI Damage")]
    public GameObject damagePopupPrefab;

    [Header("Elemental VFX (เอฟเฟกต์ตอนระเบิดธาตุ)")]
    public GameObject fireBurstVFX;
    public GameObject waterBurstVFX;
    public GameObject lightningBurstVFX;

    [Header("Drop System")]
    public GameObject[] dropItems;
    [Range(0, 100)] public int dropChance = 30;
    public GameObject soulPrefab;
    public int soulAmount = 3;

    private bool isDead = false;
    private Animator anim;
    private int currentWaveMode = 0;
    private int mySpawnIndex = 0;

    [Header("ระบบเสียงมอนสเตอร์")]
    public AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        patrolSpeed = Random.Range(1.2f, 2.0f);
        basePatrolSpeed = patrolSpeed;
        baseChaseSpeed = chaseSpeed;

        GenerateRandomPuzzle();
        currentShuffleTime = shuffleTime;

        // 🌟 เรียกใช้ระบบ Tactical AI ตั้งแต่เริ่มเกิด
        StartCoroutine(TacticalAI_Routine());
    }

    public void SetTutorialWave(int waveNumber, int spawnIndex)
    {
        currentWaveMode = waveNumber;
        mySpawnIndex = spawnIndex;
        GenerateRandomPuzzle();
    }

    void GenerateRandomPuzzle()
    {
        string[] elements = { "Fire", "Lightning", "Water" };
        List<string> puzzleList = new List<string>();

        if (currentWaveMode == 1)
        {
            string singleElement = elements[mySpawnIndex % 3];
            puzzleList.Add(singleElement);
            puzzleList.Add(singleElement);
            puzzleList.Add(singleElement);
        }
        else if (currentWaveMode == 2)
        {
            string elementA = elements[Random.Range(0, 3)];
            string elementB;
            do { elementB = elements[Random.Range(0, 3)]; } while (elementB == elementA);

            puzzleList.Add(elementA);
            puzzleList.Add(elementA);
            puzzleList.Add(elementB);
        }
        else
        {
            string[] allElements = { "Fire", "Water", "Lightning" };
            puzzleList.Add(allElements[Random.Range(0, 3)]);
            puzzleList.Add(allElements[Random.Range(0, 3)]);
            puzzleList.Add(allElements[Random.Range(0, 3)]);
        }

        puzzleList.Sort();
        requiredRecipe = puzzleList[0] + puzzleList[1] + puzzleList[2];

        if (puzzleSlots != null && puzzleSlots.Length == 3)
        {
            for (int i = 0; i < puzzleSlots.Length; i++)
            {
                if (puzzleList[i] == "Fire") puzzleSlots[i].sprite = fireSprite;
                else if (puzzleList[i] == "Water") puzzleSlots[i].sprite = waterSprite;
                else if (puzzleList[i] == "Lightning") puzzleSlots[i].sprite = lightningSprite;
            }
        }

        if (puzzleCanvas != null) puzzleCanvas.SetActive(true);
    }

    void Update()
    {
        if (isBroken || isAttacking || isDead || isKnockedBack) return;

        if (isDynamicPuzzle && !isWarningPuzzle)
        {
            currentShuffleTime -= Time.deltaTime;
            if (currentShuffleTime <= 0)
            {
                // 🌟 แทนที่จะสุ่มใหม่ปุบปับ ให้เรียก Coroutine กระพริบเตือนก่อน
                StartCoroutine(PuzzleWarningRoutine());
            }
        }

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
        if (isBroken || isAttacking || isDead || isKnockedBack) return;

        if (isChasing)
        {
            float currentSpeed = chaseSpeed;

            // 🌟 ตีความคำสั่งจาก AI
            if (tacticalAction == 0) // สั่งหยุด
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else if (tacticalAction == 1) // สั่งบุก
            {
                if (!canMoveForward) { rb.velocity = new Vector2(0, rb.velocity.y); return; }
                rb.velocity = new Vector2(transform.right.x * currentSpeed, rb.velocity.y);
            }
            else if (tacticalAction == -1) // สั่งถอยหลัง (ชั้นเชิงสูง)
            {
                // ถอยด้วยความเร็ว 70% ของวิ่งบุก จะได้ดูพริ้วๆ
                rb.velocity = new Vector2(-transform.right.x * (currentSpeed * 0.7f), rb.velocity.y);
            }
        }
        else
        {
            // หลุดระยะมองเห็น เดินลาดตระเวนปกติ
            if (!canMoveForward)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                return;
            }
            rb.velocity = new Vector2(transform.right.x * patrolSpeed, rb.velocity.y);
        }
    }

    // ==========================================
    // 🌟 Coroutine จัดการจังหวะ AI (Tactical AI)
    // ==========================================
    private IEnumerator TacticalAI_Routine()
    {
        tacticalAction = 0;
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        while (!isDead)
        {
            if (player == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            float distToPlayer = Vector2.Distance(transform.position, player.position);

            // 🧠 คิดชั้นเชิงที่ 1: ถ้าผู้เล่นอยู่ใกล้เกินไป แล้วเราโจมตีไม่ได้ (ติดคูลดาวน์) -> ถอยหนี!
            if (distToPlayer < retreatDistance && isOnCooldown)
            {
                tacticalAction = -1; // สั่งถอยหลัง
                yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            }
            // 🧠 คิดชั้นเชิงที่ 2: ถ้าผู้เล่นอยู่ไกล -> วิ่งเข้าใส่เลย
            else if (distToPlayer > sweetSpotDistance)
            {
                tacticalAction = 1; // สั่งเดินหน้า
                yield return new WaitForSeconds(Random.Range(1f, 2f));
            }
            // 🧠 คิดชั้นเชิงที่ 3: ระยะพอดี (คุมเชิง) -> สุ่มโยกหลอก เดินหน้า, ถอยหลัง, หรือหยุดนิ่ง
            else
            {
                int randMove = Random.Range(-1, 2); // สุ่มเลข -1, 0, 1
                tacticalAction = randMove;

                // ให้เวลาทำฟุตเวิร์กแป๊บเดียว จะได้ดูพริ้วๆ
                yield return new WaitForSeconds(Random.Range(0.5f, 1.2f));
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        isOnCooldown = true;

        rb.velocity = new Vector2(0, rb.velocity.y);
        transform.localScale = new Vector3(transform.localScale.x, 0.8f, 1f);
        yield return new WaitForSeconds(0.4f);

        if (isDead || isKnockedBack) { isAttacking = false; yield break; }

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

        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        if (isBroken)
        {
            currentHealth -= damage;
            ShowDamagePopup(damage, Color.white, 4f);

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                if (!isKnockedBack) StartCoroutine(KnockbackRoutine());
            }
        }
        else
        {
            ShowDamagePopup(0, Color.gray, 3f);
        }
    }
    public bool CheckPuzzleBullet(string playerRecipe, int damage)
    {
        if (isBroken || isDead) return false;

        if (playerRecipe == requiredRecipe)
        {
            BreakArmor();
            TriggerElementalBurst(requiredRecipe);
            TakeDamage(damage);

            // 🩸 [บัฟ Bloodthirst] โจมตีพัซเซิลถูก ได้เลือดคืน 1 หน่วย
            if (BuffManager.instance != null && BuffManager.instance.HasBuff("Bloodthirst"))
            {
                PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(1);
                }
            }

            return true;
        }
        else
        {
            ShowDamagePopup(0, Color.red, 3f);
            return false;
        }
    }


    private void TriggerElementalBurst(string recipe)
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        if (recipe == "FireFireFire")
        {
            int hasFireUpgrade = PlayerPrefs.GetInt("Skill_Fire", 0);
            if (hasFireUpgrade == 1)
            {
                if (fireBurstVFX != null) Destroy(Instantiate(fireBurstVFX, transform.position, Quaternion.identity), 2f);
                int burstDamage = 100;
                currentHealth -= burstDamage;
                ShowDamagePopup(burstDamage, Color.red, 8f);
                StartCoroutine(BurnRoutine(5, 10));
            }
            else
            {
                if (fireBurstVFX != null) Destroy(Instantiate(fireBurstVFX, transform.position, Quaternion.identity), 1.5f);
                int burstDamage = 30;
                currentHealth -= burstDamage;
                ShowDamagePopup(burstDamage, Color.red, 6f);
                StartCoroutine(BurnRoutine(5, 5));
            }
        }
        else if (recipe == "WaterWaterWater")
        {
            if (waterBurstVFX != null) Destroy(Instantiate(waterBurstVFX, transform.position, Quaternion.identity), 1.5f);
            int burstDamage = 20;
            currentHealth -= burstDamage;
            ShowDamagePopup(burstDamage, Color.cyan, 6f);
            StartCoroutine(SuperKnockbackRoutine());
        }
        else if (recipe == "LightningLightningLightning")
        {
            if (lightningBurstVFX != null) Destroy(Instantiate(lightningBurstVFX, transform.position, Quaternion.identity), 1.5f);
            int burstDamage = 15;
            currentHealth -= burstDamage;
            ShowDamagePopup(burstDamage, Color.yellow, 6f);
            StartCoroutine(StunRoutine(5f));
        }
        else
        {
            int defaultDamage = 15;
            currentHealth -= defaultDamage;
            ShowDamagePopup(defaultDamage, Color.white, 4f);
        }

        if (currentHealth <= 0) Die();
    }

    private IEnumerator StunRoutine(float duration)
    {
        spriteRenderer.color = Color.yellow;
        patrolSpeed = 0f;
        chaseSpeed = 0f;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        patrolSpeed = basePatrolSpeed;
        chaseSpeed = baseChaseSpeed;
        if (!isDead && spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    private IEnumerator BurnRoutine(int ticks, int damagePerTick)
    {
        for (int i = 0; i < ticks; i++)
        {
            yield return new WaitForSeconds(1f);
            if (isDead) yield break;

            currentHealth -= damagePerTick;
            ShowDamagePopup(damagePerTick, new Color(1f, 0.5f, 0f), 3.5f);

            if (currentHealth <= 0) Die();
        }
    }

    private IEnumerator SuperKnockbackRoutine()
    {
        isKnockedBack = true;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            float bounceDir = (player != null && player.position.x > transform.position.x) ? -1f : 1f;

            rb.velocity = new Vector2(bounceDir * (knockbackForce * 2f), knockbackUpward * 1.5f);
        }

        yield return new WaitForSeconds(knockbackTime * 1.5f);

        if (rb != null && !isDead) rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            float bounceDir = 1f;
            if (player != null && player.position.x > transform.position.x) bounceDir = -1f;

            rb.velocity = new Vector2(bounceDir * knockbackForce, knockbackUpward);
        }

        yield return new WaitForSeconds(knockbackTime);

        if (rb != null && !isDead) rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }

    void BreakArmor()
    {
        isBroken = true;

        if (puzzleCanvas != null) puzzleCanvas.SetActive(false);
        spriteRenderer.color = Color.gray;

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
            currentShuffleTime = shuffleTime;
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

        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);

        if (puzzleCanvas != null) puzzleCanvas.SetActive(false);
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        if (BuffManager.instance != null && BuffManager.instance.HasBuff("Soul Detonation"))
        {
            if (fireBurstVFX != null) Destroy(Instantiate(fireBurstVFX, transform.position, Quaternion.identity), 1.5f);

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 3f);
            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.CompareTag("Enemy") && enemy.gameObject != this.gameObject)
                {
                    Enemy enemyScript = enemy.GetComponent<Enemy>();
                    if (enemyScript != null) enemyScript.TakeDamage(30);
                }
            }
        }

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

        if (anim != null) anim.SetTrigger("Die");
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
    }
    private IEnumerator PuzzleWarningRoutine()
    {
        isWarningPuzzle = true;

        // สั่งให้กระพริบเปิด-ปิด 6 ครั้ง (ใช้เวลาประมาณ 1 วินาทีก่อนเปลี่ยนธาตุ)
        for (int i = 0; i < 6; i++)
        {
            // เช็กระบบป้องกัน: ถ้ามอนสเตอร์ตายหรือเกราะแตกไประหว่างนี้ ให้ยกเลิกการกระพริบเลย
            if (isDead || isBroken)
            {
                isWarningPuzzle = false;
                yield break;
            }

            // สลับสถานะ ปิด/เปิด หน้าต่างพัซเซิล
            if (puzzleCanvas != null) puzzleCanvas.SetActive(!puzzleCanvas.activeSelf);

            yield return new WaitForSeconds(0.15f); // ความเร็วกระพริบ (ปรับให้ถี่ขึ้นได้)
        }

        // เมื่อกระพริบเสร็จ มั่นใจว่าเปิดภาพกลับมาแล้วค่อยสุ่มธาตุใหม่
        if (!isDead && !isBroken && puzzleCanvas != null)
        {
            puzzleCanvas.SetActive(true);
            GenerateRandomPuzzle();
        }

        currentShuffleTime = shuffleTime; // รีเซ็ตเวลาเริ่มต้นนับใหม่
        isWarningPuzzle = false;
    }
}