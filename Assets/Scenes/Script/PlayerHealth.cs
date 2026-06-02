using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI System")]
    public Image[] heartImages;
    public Sprite fullHeart;

    [Header("I-Frames (อมตะชั่วคราว)")]
    public float iFrameDuration = 1.5f;
    public int numberOfFlashes = 5;

    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    public bool hasShield = false;

    // 🌟 1. เพิ่มตัวแปร Animator ของผู้เล่น
    private Animator anim;
    [Header("ระบบเสียงบาดเจ็บและตาย")]
    public AudioSource audioSource; // ลาก Player ตัวเองมาใส่เหมือนเดิม
    public AudioClip hurtSound;     // เสียงตอนโดนตี (อั่ก!)
    public AudioClip deathSound;    // เสียงตอนเลือดหมด (อ๊ากก!)
    private bool isDead = false; // 🌟 เพิ่มกุญแจล็อกสถานะการตาย
    public AudioClip healSound;

    void Start()
    {
        // --- 🌟 จัดระเบียบโค้ดตอนเริ่มเกมให้กระชับขึ้น ---
        int bonusHealth = PlayerPrefs.GetInt("Upgrade_Health", 0);

        // (ถ้าในโปรเจกต์ไม่ได้ใช้ GlobalStats แล้ว สามารถลบ + GlobalStats.BonusMaxHealth ทิ้งได้ครับ)
        maxHealth = 5 + bonusHealth;
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>(); // 🌟 ดึง Animator มาเตรียมไว้เล่นท่าตาย

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
        
        // ==========================================
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

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
        float flashDuration = iFrameDuration / (numberOfFlashes * 2f);

        for (int i = 0; i < numberOfFlashes; i++)
        {
            // 🌟 แก้ตรงนี้! เปลี่ยนให้กระพริบเป็น "สีแดง" (และโปร่งแสงนิดๆ 50% ให้ดูเป็นอมตะ)
            // r=1 (แดงเต็ม), g=0, b=0, a=0.5f (โปร่งแสง)
            spriteRenderer.color = new Color(1f, 0f, 0f, 0.5f);

            // 💡 ทริค: ถ้าคุณอาร์มอยากได้แดงแป๊ดๆ แบบทึบแสงเลย ให้เปลี่ยนเป็น:
            // spriteRenderer.color = Color.red; 

            yield return new WaitForSeconds(flashDuration);

            // กระพริบกลับมาเป็นสีปกติ
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }

        // ชัวร์ว่าจบแล้วสีกลับมาเป็นปกติ 100%
        spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    void UpdateHealthUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (fullHeart != null)
            {
                heartImages[i].sprite = fullHeart;
            }

            if (i < currentHealth)
            {
                heartImages[i].color = Color.white;
            }
            else
            {
                heartImages[i].color = new Color(0, 0, 0, 0.5f);
            }

            if (i < maxHealth) heartImages[i].enabled = true;
            else heartImages[i].enabled = false;
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null && pc.walkAudioSource != null)
        {
            pc.walkAudioSource.Stop(); // สั่งหยุดเสียงเดินเด็ดขาด!
            pc.enabled = false;        // ปิดสคริปต์เดินไปเลย จะได้คุมไม่ได้อีกตอนตาย
        }
        Debug.Log("💀 Player Died!");
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        Debug.Log("1. เริ่มรัน DeathRoutine");
        isInvincible = true;
       
        // ==========================================
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        Debug.Log("2. เล่นเสียงตายเสร็จแล้ว");

        // 🌟 2. สั่งเล่นแอนิเมชันตาย! (อย่าลืมไปตั้งค่า Trigger "Die" ใน Animator ด้วยนะครับ)
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }
        Debug.Log("3. กำลังจะรอ 0.5 วินาที");

        // 🌟 3. ปิดกล่องชน ศัตรูจะได้เดินทะลุไปเลย ไม่มาเดินดันศพเรากระเด็น
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 🌟 4. ล็อกตัวละครให้อยู่กับที่! (สำคัญมาก ไม่งั้นพอปิดกล่องชน ตัวละครจะร่วงทะลุพื้นตกแมพ)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // หยุดความเร็วทั้งหมด
            rb.gravityScale = 0f;       // ปิดแรงโน้มถ่วงให้ศพลอยแตะพื้นตรงนั้นเลย
        }

        // 🌟 5. ปิดสคริปต์ควบคุมการเดินทั้งหมด (เพื่อไม่ให้ผู้เล่นกดปุ่มเดินหรือยิงต่อได้)
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null) playerController.enabled = false;

        PlayerCombat playerCombat = GetComponent<PlayerCombat>();
        if (playerCombat != null) playerCombat.enabled = false;

        // 🌟 6. ⏳ หน่วงเวลาเพิ่มเป็น 2 วินาที เพื่อให้คนเล่นได้ดูแอนิเมชันตอนตายจนจบ
        float waitTime = 2f; // ค่าเริ่มต้นกันเหนียว (เผื่อลืมใส่เสียง)
        if (deathSound != null)
        {
            // ให้เลือกระหว่าง 2 วินาที กับ ความยาวไฟล์เสียง อันไหนนานกว่าให้ใช้อันนั้น
            waitTime = Mathf.Max(2f, deathSound.length);
        }

        // รอจนกว่าจะครบเวลา (เสียงเล่นจบพอดี)
        yield return new WaitForSeconds(waitTime);

        // ==========================================

        // โชว์หน้าจอ Game Over
        if (EndGameManager.instance != null)
        {
            Debug.Log("5. เจอ instance แล้ว! กำลังเรียกโชว์หน้าจอ...");
            EndGameManager.instance.ShowGameOverScreen();
        }

        // ทำลายตัวละครทิ้ง
        Destroy(gameObject);
    }

    public void Heal(int healAmount)
    {
        if (currentHealth >= maxHealth)
        {
            Debug.Log("เลือดเต็มแล้วจ้า!");
            return;
        }

        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateHealthUI();  
        Debug.Log("ฮีลแล้ว! เลือดตอนนี้: " + currentHealth);
    }

    public void RefreshHealthStat()
    {
        int bonusHealth = PlayerPrefs.GetInt("Upgrade_Health", 0);
        maxHealth = 5 + bonusHealth; // แก้ให้ฐานเลือดเริ่มต้นเป็น 5 ให้ตรงกับด้านบน
        currentHealth = maxHealth;
        UpdateHealthUI();
    }
}