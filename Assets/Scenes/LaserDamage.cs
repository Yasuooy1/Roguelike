using UnityEngine;

public class LaserDamage : MonoBehaviour
{
    [Header("ตั้งค่าความแรงเลเซอร์")]
    public int damageAmount = 1; // ลดกี่หัวใจ (ใส่ 1 คือลด 1 ดวง/หรือครึ่งดวงตามระบบของคุณอาร์ม)

    [Header("เวลาอมตะจากเลเซอร์ (วินาที)")]
    // 🌟 เปลี่ยนจาก 0.5 เป็น 2.0 วินาที! 
    // เพื่อให้โดนฟาดแล้ว จะไม่โดนเลเซอร์ซ้ำอีกจนกว่าจะจบการกวาดจอ
    public float damageCooldown = 2.0f;

    private float nextDamageTime = 0f;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // เช็กคูลดาวน์ ถ้ายืนแช่ก็จะไม่โดนดาเมจซ้ำจนกว่าจะครบ 2 วินาที
            if (Time.time >= nextDamageTime)
            {
                PlayerHealth pHealth = other.GetComponent<PlayerHealth>();
                if (pHealth != null)
                {
                    pHealth.TakeDamage(damageAmount);

                    // เริ่มนับเวลาคูลดาวน์อมตะจากเลเซอร์เส้นนี้
                    nextDamageTime = Time.time + damageCooldown;

                    Debug.Log("⚡ ผู้เล่นโดนเลเซอร์ฟาด! ลด " + damageAmount + " หัวใจ");
                }
            }
        }
    }
}