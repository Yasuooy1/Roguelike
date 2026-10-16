using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    public int maxHealth = 5; // กลับมาใช้เลือด 5 หน่วยเพื่อให้ตรงกับรูป
    public int currentHealth;

    [Header("UI System (แบบสลับรูป)")]
    // 🌟 เปลี่ยนจาก Slider มาเป็น Image แบบสลับรูป
    public Image healthBarImage;
    public Sprite[] healthBarSprites; // Array สำหรับใส่รูป 6 เฟรม (เลือด 0 ถึง 5)

    [Header("I-Frames (อมตะชั่วคราว)")]
    public float iFrameDuration = 1.5f;
    public int numberOfFlashes = 5;

    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    public bool hasShield = false;

    private Animator anim;

    [Header("ระบบเสียงบาดเจ็บและตาย")]
    public AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    private bool isDead = false;
    public AudioClip healSound;
    [Header("หน้าจอจบเกม")]
    public GameObject defeatCanvas;

    void Start()
    {
        // คำนวณเลือด
        int bonusHealth = PlayerPrefs.GetInt("Upgrade_Health", 0);
        maxHealth = 5 + bonusHealth; // ปรับฐานเลือดกลับเป็น 5
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // 🌟 อัปเดตรูปหลอดเลือดตอนเริ่มเกม
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (hasShield)
        {
            hasShield = false;
            Debug.Log("💥 เกราะแตก! รอดตัวไปที ไม่เสียเลือด!");
            return;
        }

        if (isInvincible) return;

        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null && pc.isInvincible) return;

        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // 🌟 อัปเดตหลอดเลือดให้เปลี่ยนรูป
        UpdateHealthUI();

        Debug.Log("โดนโจมตี! เลือดเหลือ: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        
        if (playerLayer != -1 && enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        }

        float flashDuration = iFrameDuration / (numberOfFlashes * 2f);

        
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRenderer.color = new Color(1f, 0f, 0f, 0.5f);
            yield return new WaitForSeconds(flashDuration);

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }

        spriteRenderer.color = Color.white;
        isInvincible = false;

        
        if (playerLayer != -1 && enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        }
    }

    // 🌟 ฟังก์ชันอัปเดต UI แบบสลับรูป
    void UpdateHealthUI()
    {
        if (healthBarImage != null && healthBarSprites != null && healthBarSprites.Length > 0)
        {
            // ป้องกัน Error โดยการล็อกค่าให้อยู่ในช่วง 0 ถึงจำนวนรูปที่มี
            int index = Mathf.Clamp(currentHealth, 0, healthBarSprites.Length - 1);
            healthBarImage.sprite = healthBarSprites[index];
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null && pc.walkAudioSource != null)
        {
            pc.walkAudioSource.Stop();
            pc.enabled = false;
        }
        Debug.Log("💀 Player Died!");
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        isInvincible = true;

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null) playerController.enabled = false;

        // หมายเหตุ: สคริปต์ PlayerCombat 
        // PlayerCombat playerCombat = GetComponent<PlayerCombat>();
        // if (playerCombat != null) playerCombat.enabled = false;

        float waitTime = 2f;
        if (deathSound != null)
        {
            waitTime = Mathf.Max(2f, deathSound.length);
        }

        yield return new WaitForSeconds(waitTime);

        // หมายเหตุ: ถ้ายังไม่มีระบบ EndGameManager เจมิขอแนะนำให้คอมเมนต์บรรทัดนี้ไว้ก่อนครับ
        if (EndGameManager.instance != null) EndGameManager.instance.ShowGameOverScreen();
         // สั่งเปิด Canvas หน้า Defeat
        if (defeatCanvas != null)
        {
            defeatCanvas.SetActive(true);
        }

        Destroy(gameObject); // บรรทัดทำลายตัวละครเดิมของคุณอาร์ม

        Destroy(gameObject);
    }

    public void Heal(int healAmount)
    {
        if (currentHealth >= maxHealth) return;

        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // 🌟 อัปเดตหลอดเลือด
        UpdateHealthUI();
    }

    public void RefreshHealthStat()
    {
        int bonusHealth = PlayerPrefs.GetInt("Upgrade_Health", 0);
        maxHealth = 5 + bonusHealth;
        currentHealth = maxHealth;

        // 🌟 อัปเดตหลอดเลือด
        UpdateHealthUI();
    }
}