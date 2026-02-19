using System.Collections; // ต้องมีบรรทัดนี้เพื่อใช้ระบบหน่วงเวลา
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI System")]
    public Image[] heartImages;
    public Sprite fullHeart;

    [Header("I-Frames (อมตะชั่วคราว)")]
    public float iFrameDuration = 1.5f; // ระยะเวลาที่เป็นอมตะ (วินาที)
    public int numberOfFlashes = 5;     // จำนวนครั้งที่ตัวกะพริบ

    private bool isInvincible = false;  // ตัวแปรเช็กว่าตอนนี้เป็นอมตะอยู่ไหม
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>(); // ดึงภาพตัวละครมาเพื่อสั่งกะพริบ
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        // 🛑 ถ้าเป็นอมตะอยู่ ให้หยุดการทำงานตรงนี้เลย (ไม่เสียเลือด)
        if (isInvincible) return;

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
            // ถ้ายังไม่ตาย ให้เริ่มระบบอมตะและกะพริบ
            StartCoroutine(InvincibilityRoutine());
        }
    }

    // ระบบจับเวลาและกะพริบตัว
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true; // เปิดโหมดอมตะ

        // คำนวณเวลาที่ต้องใช้ในการกะพริบ 1 จังหวะ (มืดสลับสว่าง)
        float flashDuration = iFrameDuration / (numberOfFlashes * 2f);

        for (int i = 0; i < numberOfFlashes; i++)
        {
            // 1. ทำให้ตัวโปร่งแสง 50% (ค่า Alpha = 0.5f)
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(flashDuration);

            // 2. ทำให้ตัวกลับมาสว่างปกติ 100%
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }

        // ทำให้แน่ใจว่าตอนจบ สีกลับมาเป็นปกติ 100% เสมอ
        spriteRenderer.color = Color.white;

        isInvincible = false; // ปิดโหมดอมตะ โดนตีเข้าแล้ว
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
        Debug.Log("Player Died!");
        // โหลดฉากปัจจุบันใหม่ (ชื่อฉากต้องตรงกับใน Unity)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}