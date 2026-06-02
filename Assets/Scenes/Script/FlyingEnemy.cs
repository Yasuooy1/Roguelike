using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement & AI")]
    public float flySpeed = 2.5f;
    public float detectRange = 20f;

    private Transform player;
    private Rigidbody2D rb;

    [Header("Health & Armor")]
    public int maxHealth = 15;
    private int currentHealth;

    public bool isBroken = false;
    public float breakDuration = 3f;

    // 🌟 1. เพิ่มตัวแปร 2 ตัวนี้เข้ามา เพื่อให้ระบบ Die() ทำงานได้
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
    public AudioSource audioSource; // ลำโพงติดตัว (เอาไว้เล่นเสียงเจ็บ)
    public AudioClip hurtSound;     // เสียงโดนตี (จิ๊ด!)
    public AudioClip deathSound;    // เสียงตาย (แอ๊ก!)

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // 🌟 2. ดึง Animator มาใส่ตัวแปรเตรียมไว้เล่นท่าตอนตาย
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;

        GenerateRandomPuzzle();
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
        if (isDead) return; // ถ้าตายแล้วไม่ต้องหันหน้าหาผู้เล่น

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            return;
        }

        if (player != null)
        {
            if (player.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    void FixedUpdate()
    {
        if (isBroken || player == null || isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * flySpeed;
        }
        else rb.velocity = Vector2.zero;
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
        if (isDead) return; // กันตายซ้ำซ้อน
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
        // 🌟 เปลี่ยนจาก return; ลอยๆ เป็น return false;
        if (isBroken || isDead) return false;

        if (playerRecipe == requiredRecipe)
        {
            Debug.Log("🎯 รหัสนกถูกต้อง! ปีกหัก พร้อมโดนดาเมจ!");
            BreakArmor();
            TakeDamage(damage);
            return true; // 🌟 รหัสถูก! ส่งค่า true (กระสุนระเบิด)
        }
        else
        {
            Debug.Log("❌ รหัสผิด!");
            ShowDamagePopup(0, Color.red, 3f);
            return false; // 🌟 รหัสผิด! ส่งค่า false (กระสุนเด้ง)
        }
    }

    void BreakArmor()
    {
        isBroken = true;
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

        if (puzzleCanvas != null) puzzleCanvas.SetActive(false); // ปิดหลอดปุ่มเวทมนตร์ทิ้งด้วย

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

        // 1. สั่งเล่นแอนิเมชันตาย!
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 2. ปิดกล่องชน (Collider) ผู้เล่นจะได้เดินทะลุศพไปได้ ไม่โดนดาเมจอีก
        //GetComponent<Collider2D>().enabled = false;

        // 3. ให้ศพตกพื้น (ถ้าเกมมีแรงโน้มถ่วง) หรือหยุดอยู่กับที่
        rb.velocity = Vector2.zero;
        rb.gravityScale = 3f; // ให้ซากค้างคาวหล่นตุ๊บลงพื้น

        // 4. หน่วงเวลาทำลายทิ้ง 1 วินาที! ให้คนเล่นได้เห็นศพแบนๆ แป๊บนึงก่อนหายไป
        Destroy(gameObject, 1f);

    } // 🌟 3. จัดระเบียบปีกกาปิดให้ถูกต้องแล้ว
}