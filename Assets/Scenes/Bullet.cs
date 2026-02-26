using UnityEngine;
using static PlayerCombat;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;

    //[HideInInspector]
    //public PlayerCombat.Element bulletElement;// // ของเดิม (เก็บไว้เผื่อบอส/นกยังต้องใช้)

    private Rigidbody2D rb;

    [Header("ระบบ Puzzle")]
    public bool isPuzzleBullet = false; // ตรวจสอบว่าเป็นกระสุนไขปริศนาหรือเปล่า
    public string puzzleRecipe = "";    // รหัสผ่านที่พกมาด้วย (เช่น "FireFireWater")

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        Destroy(gameObject, 2f); // ทำลายตัวเองถ้าไม่ชนอะไรใน 2 วิ
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. เช็กมอนสเตอร์เดินดิน
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (isPuzzleBullet)
            {
                if (!enemy.isBroken)
                {
                    // 🌟 ส่งรหัสไปเช็ก พร้อมกับส่ง "ดาเมจ" ไปด้วยเลย!
                    enemy.CheckPuzzleBullet(puzzleRecipe, (int)damage);
                }
                else
                {
                    enemy.TakeDamage((int)damage);
                }
            }
        }

        // 2. เช็กมอนสเตอร์บิน (นก)
        FlyingEnemy flyingEnemy = hitInfo.GetComponent<FlyingEnemy>();
        if (flyingEnemy != null)
        {
            if (isPuzzleBullet)
            {
                if (!flyingEnemy.isBroken)
                {
                    // 🌟 ส่งรหัสไปเช็ก พร้อมกับส่ง "ดาเมจ" ไปด้วย!
                    flyingEnemy.CheckPuzzleBullet(puzzleRecipe, (int)damage);
                }
                else
                {
                    flyingEnemy.TakeDamage((int)damage);
                }
            }
        }

        // ... (ส่วนโค้ดเช็ก Boss กับ Pillar ข้างล่างปล่อยไว้เหมือนเดิมได้เลยครับ) ...

        Boss boss = hitInfo.GetComponent<Boss>();
        // ...

        // ========================================================
        // ⚠️ ส่วนด้านล่างนี้ ผมคงระบบเก่าของคุณอาร์มไว้ให้ก่อนนะครับ 
        // เผื่อบอสกับนกยังไม่ได้อัปเกรดเป็นระบบ Puzzle จะได้ไม่ Error ครับ
        // ========================================================

        // 2. เช็กว่ายิงโดนบอสหลักไหม?
        /*Boss boss = hitInfo.GetComponent<Boss>();
        if (boss != null)
        {
            boss.TakeDamage((int)damage, bulletElement);
        }

        // 3. เช็กว่ายิงโดน "เสาพลังงาน" ของบอสไหม?
        BossPillar pillar = hitInfo.GetComponent<BossPillar>();
        if (pillar != null)
        {
            pillar.TakeDamage((int)damage, bulletElement);
        }

        // 🦇 3.5 เพิ่มระบบเช็กมอนสเตอร์บิน
        FlyingEnemy flyingEnemy = hitInfo.GetComponent<FlyingEnemy>();
        if (flyingEnemy != null)
        {
            flyingEnemy.TakeDamage((int)damage, bulletElement);
        }

        // 4. สั่งทำลายกระสุนเมื่อชนสิ่งมีชีวิต หรือชนพื้น/กำแพง
        if (enemy != null || boss != null || pillar != null || flyingEnemy != null || hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }*/
    }
}
