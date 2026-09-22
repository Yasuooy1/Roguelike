using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement & AI")]
    public float flySpeed = 2.5f;
    public float detectRange = 20f;
    private float baseFlySpeed; // จำความเร็วก่อนโดนสโลว์

    private Transform player;
    private Rigidbody2D rb;

    [Header("Health & Stats")]
    public int maxHealth = 15;
    private int currentHealth;

    [Header("ตัวคูณความแพ้ธาตุ (1 = ปกติ, 1.5 = แพ้, 0.5 = กัน)")]
    public float fireMultiplier = 1f;
    public float waterMultiplier = 1f;
    public float windMultiplier = 1f;

    [Header("ตั้งค่าไอคอนสถานะ")]
    public float iconOffsetY = 1.0f; // ค้างคาวตัวเล็ก อาจจะปรับให้ต่ำลงหน่อย
    public float iconSpacing = 0.5f;
    private List<GameObject> activeIcons = new List<GameObject>();

    [Header("Elemental VFX (เอฟเฟกต์ตอนติดสถานะ)")]
    public GameObject fireBurstVFX;
    public GameObject waterBurstVFX;
    public GameObject windBurstVFX;

    [Header("UI Damage")]
    public GameObject damagePopupPrefab;
    private SpriteRenderer spriteRenderer;

    [Header("Drop System")]
    public GameObject soulPrefab;
    public int soulAmount = 3;
    public GameObject[] dropItems;
    [Range(0, 100)] public int dropChance = 30;

    [Header("ระบบเสียงค้างคาว")]
    public AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    // สถานะ
    private bool isDead = false;
    private Animator anim;

    // สถานะผิดปกติ
    private bool isBurning = false;
    private bool isSlowed = false;
    private bool isStunned = false;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        baseFlySpeed = flySpeed;
        rb.gravityScale = 0f; // นกบินต้องไม่มีแรงโน้มถ่วง
    }

    void Update()
    {
        if (isDead) return;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            return;
        }

        // หันหน้าหาผู้เล่น
        if (player.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void FixedUpdate()
    {
        if (player == null || isDead || isStunned) return; // ถ้าโดนช็อตหรือตาย ไม่ต้องบินตาม

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * flySpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isDead)
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
    // ⚔️ ระบบรับดาเมจและคำนวณธาตุ
    // ==========================================
    // ==========================================
    // ⚔️ ระบบรับดาเมจและคำนวณธาตุ (อัปเดตระบบภูมิคุ้มกันสถานะ)
    // ==========================================
    public void TakeElementalDamage(int baseDamage, string spellRecipe)
    {
        if (isDead) return;

        float finalDamage = baseDamage;
        Color popupColor = Color.white;

        // ==========================================
        // 🌪️ 1. ดักท่าพายุเพลิง (สูตรใหม่ FireFireWind)
        // ==========================================
        if (spellRecipe == "FireFireWind")
        {
            finalDamage = baseDamage * 2f;
            popupColor = new Color(1f, 0.4f, 0f); // สีส้มแดงของพายุเพลิง
        }
        // ==========================================
        // ⚔️ 2. คำนวณธาตุปกติ (ไฟ, น้ำ, ลม)
        // ==========================================
        else
        {
            if (spellRecipe.Contains("Fire"))
            {
                finalDamage *= fireMultiplier;
                popupColor = new Color(1f, 0.5f, 0f);
                if (!isBurning && fireMultiplier >= 1f) StartCoroutine(ApplyBurn());
            }
            if (spellRecipe.Contains("Water"))
            {
                finalDamage *= waterMultiplier;
                popupColor = Color.cyan;
                if (!isSlowed && waterMultiplier >= 1f) StartCoroutine(ApplySlow());
            }
            // 🌟 ลบสายฟ้าทิ้ง เปลี่ยนเป็นลมแทน 
            if (spellRecipe.Contains("Wind"))
            {
                finalDamage *= windMultiplier;
                popupColor = Color.green; // สีเขียวธาตุลม
                
            }
        }

        currentHealth -= (int)finalDamage;

        // แสดงผล UI, เสียง, แอนิเมชัน และเช็กการตาย
        if (audioSource != null && hurtSound != null) audioSource.PlayOneShot(hurtSound);
        if (anim != null) anim.SetTrigger("Hurt");

        
        // if (!isKnockedBack) StartCoroutine(KnockbackRoutine());

        ShowDamagePopup((int)finalDamage, popupColor, 4f);

        if (currentHealth <= 0) Die();
    }

    // รองรับระบบสกิลเก่าที่ไม่ใช้ธาตุ
    public void TakeDamage(int damage)
    {
        TakeElementalDamage(damage, "None");
    }

    // ==========================================
    // 🧪 ระบบสถานะผิดปกติและการเรียงไอคอน
    // ==========================================
    void RearrangeIcons()
    {
        activeIcons.RemoveAll(item => item == null);
        int count = activeIcons.Count;
        if (count == 0) return;

        float startX = -(count - 1) * iconSpacing / 2f;
        for (int i = 0; i < count; i++)
        {
            activeIcons[i].transform.localPosition = new Vector3(startX + (i * iconSpacing), iconOffsetY, 0);
        }
    }

    IEnumerator ApplyBurn()
    {
        isBurning = true;
        GameObject icon = null;
        SpriteRenderer iconSR = null;

        if (fireBurstVFX != null)
        {
            icon = Instantiate(fireBurstVFX, transform.position, Quaternion.identity, transform);
            iconSR = icon.GetComponent<SpriteRenderer>();
            activeIcons.Add(icon);
            RearrangeIcons();
        }

        for (int i = 0; i < 3; i++)
        {
            if (isDead) yield break;
            if (i == 2 && iconSR != null) StartCoroutine(BlinkIconRoutine(iconSR));
            yield return new WaitForSeconds(1f);

            currentHealth -= 5;
            ShowDamagePopup(5, new Color(1f, 0.5f, 0f), 3f);
            if (currentHealth <= 0) Die();
        }

        if (icon != null)
        {
            activeIcons.Remove(icon);
            RearrangeIcons();
            Destroy(icon);
        }
        isBurning = false;
    }

    IEnumerator ApplySlow()
    {
        isSlowed = true;
        GameObject icon = null;
        SpriteRenderer iconSR = null;

        if (waterBurstVFX != null)
        {
            icon = Instantiate(waterBurstVFX, transform.position, Quaternion.identity, transform);
            iconSR = icon.GetComponent<SpriteRenderer>();
            activeIcons.Add(icon);
            RearrangeIcons();
        }

        if (spriteRenderer != null) spriteRenderer.color = Color.cyan;

        if (!isStunned)
        {
            // ==========================================
            // 🌟 Water Mastery: คำนวณความหนืดในการเดิน
            // ==========================================
            int buffLevel = BuffManager.instance != null ? BuffManager.instance.waterLevel : 0;

            // พื้นฐานความเร็วเหลือ 50% (0.5f) และหนืดขึ้นเลเวลละ 10% (0.1f)
            float speedMultiplier = 0.5f - (buffLevel * 0.1f);

            // 🛑 ติดเพดาน! บังคับให้ค่าน้อยสุดคือ 0.2f (ห้ามเดินช้ากว่า 20%)
            speedMultiplier = Mathf.Max(speedMultiplier, 0.2f);       
             flySpeed = baseFlySpeed * speedMultiplier;
        }

        yield return new WaitForSeconds(2f); // ระยะเวลาที่โดนสโลว์ (2 วินาที)

        if (iconSR != null) StartCoroutine(BlinkIconRoutine(iconSR));
        yield return new WaitForSeconds(1f);

        if (!isStunned)
        {
            // คืนค่าความเร็วกลับเป็นปกติ
            
            

            
            // flySpeed = baseFlySpeed;
        }

        if (spriteRenderer != null && !isDead) spriteRenderer.color = Color.white;

        if (icon != null)
        {
            activeIcons.Remove(icon);
            RearrangeIcons();
            Destroy(icon);
        }
        isSlowed = false;
    }

    /*IEnumerator ApplyStun()
    {
        isStunned = true;
        GameObject icon = null;
        SpriteRenderer iconSR = null;

        if (lightningBurstVFX != null)
        {
            icon = Instantiate(lightningBurstVFX, transform.position, Quaternion.identity, transform);
            iconSR = icon.GetComponent<SpriteRenderer>();
            activeIcons.Add(icon);
            RearrangeIcons();
        }

        if (spriteRenderer != null) spriteRenderer.color = Color.yellow;

        flySpeed = 0f;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.5f);

        if (iconSR != null) StartCoroutine(BlinkIconRoutine(iconSR));
        yield return new WaitForSeconds(1f);

        flySpeed = isSlowed ? baseFlySpeed * 0.5f : baseFlySpeed;

        if (spriteRenderer != null && !isDead) spriteRenderer.color = Color.white;

        if (icon != null)
        {
            activeIcons.Remove(icon);
            RearrangeIcons();
            Destroy(icon);
        }
        isStunned = false;
    }*/

    IEnumerator BlinkIconRoutine(SpriteRenderer sr)
    {
        float timer = 0f;
        while (timer < 1f)
        {
            if (sr == null || isDead) yield break;
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.15f);
            timer += 0.15f;
        }
        if (sr != null) sr.enabled = true;
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
        rb.gravityScale = 3f; // นกตายแล้วร่วงลงพื้น

        Destroy(gameObject, 1f);
    }
}