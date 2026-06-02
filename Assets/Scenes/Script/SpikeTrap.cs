using System.Collections;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("ตั้งค่าความโหด")]
    public int damageAmount = 1;         // ดาเมจที่ทำได้
    public float warningDelay = 0.5f;    // เวลาเตือนก่อนหนามพุ่ง (วินาที)
    public float activeDuration = 1.0f;  // เวลาที่หนามค้างอยู่ด้านบน (วินาที)
    public float cooldown = 1.5f;        // คูลดาวน์ก่อนจะทำงานรอบใหม่ได้

    [Header("อนิเมชัน & เสียง")]
    public Animator anim;
    public AudioSource audioSource;
    public AudioClip warningSound; // เสียงเตือน กริ๊ก!
    public AudioClip strikeSound;  // เสียงหนามแทง ฉึก!

    private bool isTriggered = false; // โดนเหยียบหรือยัง?
    private bool isSpikesUp = false;  // หนามแทงขึ้นมาสุดหรือยัง?

    // 🌟 1. ดักจับตอนคนเดินมาเหยียบ (ทริกเกอร์ทำงาน)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTriggered)
        {
            StartCoroutine(ActivateTrap());
        }
    }

    // 🌟 2. ดักจับตอนหนามแทงค้างอยู่ แล้วมีคนแช่อยู่ตรงนั้น (โดนดาเมจ)
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isSpikesUp && collision.CompareTag("Player"))
        {
            DealDamageToPlayer(collision.gameObject);
        }
    }

    // 🌟 3. กระบวนการทำงานของหนาม
    private IEnumerator ActivateTrap()
    {
        isTriggered = true; // ล็อกไว้ไม่ให้เหยียบซ้ำจนกว่าจะรีเซ็ต

        // --- เฟส 1: เตือนภัย (Warning) ---
        if (anim != null) anim.SetTrigger("Warning");
        if (audioSource != null && warningSound != null) audioSource.PlayOneShot(warningSound);

        yield return new WaitForSeconds(warningDelay);

        // --- เฟส 2: แทง! (Strike) ---
        isSpikesUp = true;
        if (anim != null) anim.SetTrigger("Strike");
        if (audioSource != null && strikeSound != null) audioSource.PlayOneShot(strikeSound);

        yield return new WaitForSeconds(activeDuration);

        // --- เฟส 3: หดกลับ & คูลดาวน์ (Reset) ---
        isSpikesUp = false;
        if (anim != null) anim.SetTrigger("Hide");

        yield return new WaitForSeconds(cooldown);
        isTriggered = false; // กลับมาพร้อมใช้งานอีกครั้ง
    }

    // 🌟 4. ฟังก์ชันส่งดาเมจไปหาผู้เล่น
    private void DealDamageToPlayer(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null && health.currentHealth > 0)
        {
            // เรียกฟังก์ชันลดเลือดที่คุณอาร์มทำไว้
            health.TakeDamage(damageAmount);

            // ถ้าอยากให้โดนหนามแล้วกระเด็นด้วย ก็เรียก Knockback ได้เลย!
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.Knockback(transform); // กระเด็นออกจากศูนย์กลางกับดัก
            }
        }
    }
}