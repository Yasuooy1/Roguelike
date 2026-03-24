using UnityEngine;

public class UltimateSkill : MonoBehaviour
{
    public float speed = 15f;
    public int massiveDamage = 50;
    public float lifeTime = 3f;

    [Header("เอฟเฟกต์ความอลังการ")]
    public float rotationSpeed = 720f; // ความเร็วในการหมุนควงสว่าน! (องศาต่อวินาที)
    public GameObject impactEffect;    // Prefab เอฟเฟกต์ระเบิดตอนชนบอส

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = transform.right * speed;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 🌟 ทำให้ท่าไม้ตายหมุนติ้วๆ ตลอดเวลาที่พุ่งไปข้างหน้า (เหมือนกงจักร)
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. ชนบอส
        Boss boss = hitInfo.GetComponent<Boss>();
        if (boss != null)
        {
            boss.ForceBreakArmor();
            boss.TakeDamage(massiveDamage);
            Explode(); // เรียกฟังก์ชันระเบิด
            GameFeelManager.instance.ScreenShake(0.5f, 0.2f);
            GameFeelManager.instance.HitStop(0.1f);
        }

        // 2. ชนมอนสเตอร์ทั่วไป
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null) { enemy.isBroken = true; enemy.TakeDamage(massiveDamage); Explode(); }

        FlyingEnemy flyingEnemy = hitInfo.GetComponent<FlyingEnemy>();
        if (flyingEnemy != null) { flyingEnemy.isBroken = true; flyingEnemy.TakeDamage(massiveDamage); Explode(); }
    }

    // 🌟 ฟังก์ชันเสกระเบิดแล้วทำลายตัวเอง
    void Explode()
    {
        if (impactEffect != null)
        {
            // เสกเอฟเฟกต์ระเบิดตรงจุดที่ชน
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        // ถ้าอยากให้ท่าไม้ตายยิง "ทะลุ" มอนสเตอร์ไปเลย ให้ลบบรรทัด Destroy ข้างล่างนี้ทิ้งนะครับ!
        Destroy(gameObject);
    }
}