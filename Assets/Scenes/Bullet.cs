using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10; // ดาเมจพื้นฐาน

    [HideInInspector]
    public PlayerCombat.Element bulletElement; // เก็บว่ากระสุนนัดนี้สีอะไร

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        Destroy(gameObject, 2f); // ทำลายตัวเองถ้าไม่ชนอะไรใน 2 วิ
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. เช็กว่ายิงโดนมอนสเตอร์ทั่วไปไหม?
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, bulletElement);
        }

        // 2. เช็กว่ายิงโดนบอสหลักไหม?
        Boss boss = hitInfo.GetComponent<Boss>();
        if (boss != null)
        {
            boss.TakeDamage(damage, bulletElement);
        }

        // 3. เช็กว่ายิงโดน "เสาพลังงาน" ของบอสไหม?
        BossPillar pillar = hitInfo.GetComponent<BossPillar>();
        if (pillar != null)
        {
            pillar.TakeDamage(damage, bulletElement);
        }

        // 4. สั่งทำลายกระสุนเมื่อชนสิ่งมีชีวิต หรือชนพื้น/กำแพง
        // ถ้าสิ่งที่ชนมีสคริปต์ Enemy, Boss, BossPillar หรือเป็นเลเยอร์ Ground กระสุนจะหายไป
        if (enemy != null || boss != null || pillar != null || hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}