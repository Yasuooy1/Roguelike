using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 🌟 เพิ่มบรรทัดนี้เพื่อเรียกใช้ TextMeshPro

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI System (แบบสลับรูป)")]
    public Image healthBarImage;
    public Sprite[] healthBarSprites;
    // 🌟 เพิ่มตัวแปรสำหรับข้อความ +เลือด
    public TextMeshProUGUI bonusHealthText;

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
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer != -1 && enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        }

        int bonusHealth = PlayerPrefs.GetInt("Upgrade_Health", 0);
        maxHealth = 5 + bonusHealth;
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

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

        UpdateHealthUI();

        if (currentHealth <= 0) Die();
        else StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer != -1 && enemyLayer != -1)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

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
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
    }

    // 🌟 ระบบอัปเดต UI แบบใหม่ (โชว์หลอดเต็ม + โชว์ตัวเลขส่วนเกิน)
    void UpdateHealthUI()
    {
        if (healthBarImage != null && healthBarSprites != null && healthBarSprites.Length > 0)
        {
            int baseMax = healthBarSprites.Length - 1; // ค่าสูงสุดของรูปคือ 5

            if (currentHealth <= baseMax)
            {
                // ถ้าเลือดไม่เกิน 5 โชว์รูประดับเลือดปกติ และซ่อนตัวเลข
                healthBarImage.sprite = healthBarSprites[currentHealth];
                if (bonusHealthText != null) bonusHealthText.gameObject.SetActive(false);
            }
            else
            {
                // ถ้าเลือดเกิน 5 โชว์รูป "หลอดเต็ม" ตลอดเวลา แล้วเปิดโชว์ตัวเลข +X
                healthBarImage.sprite = healthBarSprites[baseMax];

                if (bonusHealthText != null)
                {
                    bonusHealthText.gameObject.SetActive(true);
                    int overHeal = currentHealth - baseMax;
                    bonusHealthText.text = "+" + overHeal.ToString();
                }
            }
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
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        isInvincible = true;
        gameObject.tag = "Untagged";

        if (audioSource != null && deathSound != null) audioSource.PlayOneShot(deathSound);
        if (anim != null) anim.SetTrigger("Die");

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.isActionLocked = true;
            playerController.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = new Vector2(0, rb.velocity.y);

        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer != -1 && enemyLayer != -1)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        yield return new WaitForSeconds(3.0f);

        if (EndGameManager.instance != null) EndGameManager.instance.ShowGameOverScreen();
        if (ScoreManager.instance != null) ScoreManager.instance.GameOver();
        if (defeatCanvas != null)
        {
            defeatCanvas.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void Heal(int healAmount)
    {
        if (currentHealth >= maxHealth) return;
        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void RefreshHealthStat()
    {
        UpdateHealthUI();
    }
}