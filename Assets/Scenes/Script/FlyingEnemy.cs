using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FlyingEnemy : MonoBehaviour
{
    [Header("🦇 Flight & AI (อัปเกรดบินวนและโฉบ)")]
    public float detectRange = 20f;
    public float flySpeed = 2.5f;
    public float orbitRadius = 4f;
    public float orbitSpeed = 2f;

    public float swoopSpeed = 12f;
    public float prepTime = 0.8f;
    public float attackCooldown = 4f;

    private float orbitAngle = 0f;
    private bool isSwooping = false;
    private bool isOnCooldown = false;
    private Vector3 startPos;

    // 🌟 บัตรคิวส่วนกลาง: ป้องกันค้างคาวพุ่งพร้อมกัน
    private static float nextGlobalSwoopTime = 0f;

    private Transform player;
    private Rigidbody2D rb;

    [Header("⚠️ ระบบเตือนภัยก่อนโฉบ")]
    public GameObject exclamationPrefab;
    public float exclamationOffsetY = 1.5f;

    [Header("Health & Armor")]
    public int maxHealth = 15;
    private int currentHealth;

    public bool isBroken = false;
    public float breakDuration = 3f;

    private bool isDead = false;
    private Animator anim;

    [Header("🧩 Puzzle System (รหัสผ่านนก)")]
    public string requiredRecipe;
    private SpriteRenderer spriteRenderer;

    [Header("UI ลูกแก้วบนหัวนก")]
    public GameObject puzzleCanvas;
    public Image[] puzzleSlots;
    public Sprite fireSprite;
    public Sprite waterSprite;
    public Sprite lightningSprite;

    [Header("UI")]
    public GameObject damagePopupPrefab;

    [Header("Drop System")]
    public GameObject soulPrefab;
    public int soulAmount = 3;
    public GameObject[] dropItems;
    [Range(0, 100)] public int dropChance = 30;

    [Header("ระบบเสียงค้างคาว")]
    public AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;
        startPos = transform.position;

        // 🌟 สุ่มตำแหน่งเริ่มต้นของวงโคจร และรัศมีการบิน
        // ทำให้ค้างคาวแต่ละตัวบินแยกกัน ไม่ซ้อนทับเป็นก้อนเดียว
        orbitAngle = Random.Range(0f, 360f);
        orbitRadius = Random.Range(3.5f, 5.5f);

        GenerateRandomPuzzle();

        // 🌟 สุ่มดีเลย์ตั้งต้น เพื่อให้แต่ละตัวพร้อมโฉบไม่พร้อมกันตอนเกิด
        isOnCooldown = true;
        StartCoroutine(InitialCooldownRoutine());
    }

    private IEnumerator InitialCooldownRoutine()
    {
        yield return new WaitForSeconds(Random.Range(1f, 4.5f));
        isOnCooldown = false;
    }

    void GenerateRandomPuzzle()
    {
        string[] elements = { "Fire", "Water", "Lightning" };
        List<string> puzzleList = new List<string>();

        puzzleList.Add(elements[Random.Range(0, 3)]);
        puzzleList.Add(elements[Random.Range(0, 3)]);
        puzzleList.Add(elements[Random.Range(0, 3)]);
        puzzleList.Sort();
        requiredRecipe = puzzleList[0] + puzzleList[1] + puzzleList[2];

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
        if (isDead || isBroken) return;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            return;
        }

        if (!isSwooping)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectRange)
            {
                orbitAngle += orbitSpeed * Time.deltaTime;

                // 🌟 ใช้รัศมีวงโคจร (orbitRadius) ที่สุ่มไว้ ทำให้วงการบินแตกต่างกัน
                float offsetX = Mathf.Cos(orbitAngle) * orbitRadius;
                float offsetY = Mathf.Abs(Mathf.Sin(orbitAngle)) * (orbitRadius * 0.4f) + 1.8f;

                Vector3 targetPos = player.position + new Vector3(offsetX, offsetY, 0);

                transform.position = Vector3.MoveTowards(transform.position, targetPos, orbitSpeed * 2f * Time.deltaTime);

                if (player.position.x < transform.position.x) transform.localScale = new Vector3(1, 1, 1);
                else transform.localScale = new Vector3(-1, 1, 1);

                // 🌟 ระบบบัตรคิวโฉบ: เช็กว่าบัตรคิวว่างไหม? (ห่างจากตัวก่อนหน้า 1.5 วิ)
                if (!isOnCooldown && Time.time >= nextGlobalSwoopTime)
                {
                    // หยิบบัตรคิว! บังคับให้ค้างคาวตัวอื่นต้องรอไปอีก 1.5 วิ
                    nextGlobalSwoopTime = Time.time + 1.5f;
                    StartCoroutine(SwoopAttackRoutine());
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, startPos, flySpeed * Time.deltaTime);
                if (startPos.x < transform.position.x) transform.localScale = new Vector3(1, 1, 1);
                else transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    void FixedUpdate()
    {
        if (isBroken || isDead || isSwooping) return;
        rb.velocity = Vector2.zero;
    }

    IEnumerator SwoopAttackRoutine()
    {
        isSwooping = true;
        isOnCooldown = true;

        if (exclamationPrefab != null)
        {
            GameObject warningIcon = Instantiate(exclamationPrefab, transform.position + Vector3.up * exclamationOffsetY, Quaternion.identity, transform);
            Destroy(warningIcon, prepTime);
        }

        spriteRenderer.color = Color.red;
        if (anim != null) anim.SetTrigger("Prepare");

        yield return new WaitForSeconds(prepTime);

        if (isDead || isBroken) { isSwooping = false; yield break; }

        Vector3 swoopTarget = player.position;
        Vector3 swoopStart = transform.position;
        float swoopDuration = Vector2.Distance(swoopStart, swoopTarget) / swoopSpeed;
        float t = 0;

        while (t < 1f)
        {
            if (isDead || isBroken) break;
            t += Time.deltaTime / swoopDuration;
            transform.position = Vector3.Lerp(swoopStart, swoopTarget, t);
            yield return null;
        }

        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.3f);

        isSwooping = false;

        // สุ่มเวลาพักก่อนจะเริ่มเช็กบัตรคิวครั้งถัดไป
        yield return new WaitForSeconds(attackCooldown + Random.Range(0f, 1.5f));
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
            if (currentHealth <= 0) Die();
        }
        else ShowDamagePopup(0, Color.gray, 3f);
    }

    public bool CheckPuzzleBullet(string playerRecipe, int damage)
    {
        if (isBroken || isDead) return false;

        if (playerRecipe == requiredRecipe)
        {
            BreakArmor();
            TakeDamage(damage);
            return true;
        }
        else
        {
            ShowDamagePopup(0, Color.red, 3f);
            return false;
        }
    }

    void BreakArmor()
    {
        isBroken = true;
        isSwooping = false;

        spriteRenderer.color = Color.gray;
        if (puzzleCanvas != null) puzzleCanvas.SetActive(false);

        rb.gravityScale = 3f;

        StartCoroutine(RecoverShieldRoutine());
    }

    IEnumerator RecoverShieldRoutine()
    {
        yield return new WaitForSeconds(breakDuration);
        if (currentHealth > 0 && !isDead)
        {
            isBroken = false;
            spriteRenderer.color = Color.white;
            rb.gravityScale = 0f;
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
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);
        }

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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, orbitRadius);
    }
}