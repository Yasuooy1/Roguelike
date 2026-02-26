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
    public GameObject[] dropItems;
    [Range(0, 100)] public int dropChance = 30;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // ปิดแรงโน้มถ่วงให้ลอยได้

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
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            return;
        }

        // หันหน้าหันหลังตาม Player
        if (player.position.x > transform.position.x) transform.eulerAngles = new Vector3(0, 0, 0);
        else transform.eulerAngles = new Vector3(0, 180f, 0);
    }

    void FixedUpdate()
    {
        if (isBroken || player == null) return;

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

    // =====================================
    // 💥 ระบบเจาะเกราะนก!
    // =====================================
    public void TakeDamage(int damage)
    {
        if (isBroken)
        {
            currentHealth -= damage;
            ShowDamagePopup(damage, Color.white, 4f);
            if (currentHealth <= 0) Die();
        }
        else ShowDamagePopup(0, Color.gray, 3f);
    }

    // 🌟 เพิ่ม int damage เข้ามารับค่า
    public void CheckPuzzleBullet(string playerRecipe, int damage)
    {
        if (isBroken) return;

        if (playerRecipe == requiredRecipe)
        {
            Debug.Log("🎯 รหัสนกถูกต้อง! ปีกหัก พร้อมโดนดาเมจ!");

            BreakArmor();          // 1. สั่งเกราะแตก (ปีกหักร่วงพื้น)
            TakeDamage(damage);    // 2. 🌟 อัดดาเมจเข้าเลือดต่อทันที!
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
        if (puzzleCanvas != null) puzzleCanvas.SetActive(false);

        // 🌟 ไฮไลต์: นกโดนยิงเกราะแตก เปิดแรงโน้มถ่วงให้ร่วงกระแทกพื้น!
        rb.gravityScale = 3f;

        StartCoroutine(RecoverShieldRoutine());
    }

    IEnumerator RecoverShieldRoutine()
    {
        yield return new WaitForSeconds(breakDuration);
        if (currentHealth > 0)
        {
            isBroken = false;
            spriteRenderer.color = Color.white;
            rb.gravityScale = 0f; // กลับมาบินได้เหมือนเดิม
            GenerateRandomPuzzle();
        }
    }

    // ... ฟังก์ชัน ShowDamagePopup และ Die เหมือนเดิมเลยครับ ...
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
}