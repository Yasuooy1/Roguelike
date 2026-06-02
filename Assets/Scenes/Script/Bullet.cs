using UnityEngine;
public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;
    public GameObject impactEffect;

    [Header("Puzzle System")]
    public bool isPuzzleBullet = false;
    public string puzzleRecipe;

    private Rigidbody2D rb;
    private bool isDeflected = false; // เอาไว้ล็อกไม่ให้กระสุนทำดาเมจซ้ำตอนกำลังเด้ง

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // ถ้ายิงพลาดและกำลังกระดอนตกพื้นอยู่ ให้ทะลุผ่านทุกอย่างไปเลย
        if (isDeflected) return;

        bool hitEnemy = false;
        bool isCorrectPuzzle = true; // ตั้งค่าเริ่มต้นว่ายิงถูกไว้ก่อน

        // 1. เช็กมอนสเตอร์เดินดิน
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            hitEnemy = true;
            if (isPuzzleBullet && !enemy.isBroken)
            {
                isCorrectPuzzle = enemy.CheckPuzzleBullet(puzzleRecipe, (int)damage);
            }
            else enemy.TakeDamage((int)damage);
        }

        // 2. เช็กมอนสเตอร์บิน (นก)
        FlyingEnemy flyingEnemy = hitInfo.GetComponent<FlyingEnemy>();
        if (flyingEnemy != null && !hitEnemy)
        {
            hitEnemy = true;
            if (isPuzzleBullet && !flyingEnemy.isBroken)
            {
                isCorrectPuzzle = flyingEnemy.CheckPuzzleBullet(puzzleRecipe, (int)damage);
            }
            else flyingEnemy.TakeDamage((int)damage);
        }

        // 3. เช็กบอสหลัก
        Boss boss = hitInfo.GetComponent<Boss>();
        if (boss != null && !hitEnemy)
        {
            hitEnemy = true;
            if (isPuzzleBullet && !boss.isBroken)
            {
                isCorrectPuzzle = boss.CheckPuzzleBullet(puzzleRecipe, (int)damage);
            }
            else boss.TakeDamage((int)damage);
        }

        // 4. สรุปผลลัพธ์หลังชน
        if (hitEnemy)
        {
            if (isCorrectPuzzle)
            {
                // ยิงถูกธาตุ (หรือยิงมอนสเตอร์ธรรมดา) -> ระเบิดเอฟเฟกต์แล้วหายไป
                ExplodeAndDestroy();
            }
            else
            {
                // ยิงผิดธาตุ! -> กระเด้งกลับตกพื้น
                DeflectBullet();
            }
        }
        else if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // ชนพื้นดิน หรือกำแพง -> ระเบิดหายไป
            ExplodeAndDestroy();
        }
    }

    private void ExplodeAndDestroy()
    {
        // 1. ถ้ายังมีโค้ด Instantiate เอฟเฟกต์เก่าอยู่ ให้ลบออกหรือคอมเมนต์ทิ้ง
        // if (impactEffect != null) Instantiate(impactEffect, transform.position, transform.rotation);

        // 2. 🛡️ กันเหนียว: ปิด Collider และ Rigidbody ก่อนเพื่อไม่ให้มันชนอะไรซ้ำซ้อน
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero; // หยุดวิ่ง
            rb.simulated = false; // 🌟 หยุดระบบฟิสิกส์ทั้งหมดเพื่อความชัวร์
        }

        // 3. 🎬 พยายามสั่งเล่นแอนิเมชันระเบิด
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Explode");
        }

        // 4. 🌟 ท่าไม้ตาย: สั่งทำลายตัวเองทิ้ง "ทันที" หรือหลังจาก 0.1 วินาที (เพื่อให้พอเห็นระเบิดนิดนึง)
        // อย่ารอ 0.4 วินาที เพราะบางทีแอนิเมชันมันไม่เล่น มันเลยไม่ถูก Destroy ครับ
        Destroy(gameObject); // 🌟 แก้เป็นบรรทัดนี้เลยครับ เพื่อทำลายทันทีที่ชน

        // ถ้าอยากให้แอนิเมชันระเบิดเล่นจนจบ (0.4s) เราต้องใช้ทริคเพิ่มในหัวข้อที่ 3 ครับ
    }

    private void DeflectBullet()
    {
        isDeflected = true; // ล็อกสถานะกระสุน

        // 1. เปลี่ยนกระสุนเป็นสีเทา (แสดงว่าไร้พลัง)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.gray;

        // 2. ใส่ฟิสิกส์ให้เด้งกลับ
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // เบรกความเร็วพุ่งไปข้างหน้า
            rb.gravityScale = 2f; // เปิดแรงโน้มถ่วงให้ร่วงลงพื้น

            // เช็กทิศทางเดิม ถ้ายิงไปขวา ให้เด้งไปซ้าย
            float bounceDirection = (transform.right.x > 0) ? -1f : 1f;

            // สุ่มแรงกระดอน
            float bounceX = bounceDirection * Random.Range(2f, 4f);
            float bounceY = Random.Range(3f, 6f); // เด้งลอยขึ้น
            rb.velocity = new Vector2(bounceX, bounceY);
        }

        // 3. ปิด Collider ทิ้งไปเลย กระสุนจะได้ไม่ไปทำดาเมจซ้ำ หรือชนมอนตัวอื่น
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 4. สั่งเคลียร์ขยะ ทำลายทิ้งใน 1.5 วินาที
        Destroy(gameObject, 1.5f);
    }
    // ฟังก์ชันนี้จะถูกเรียกจากแอนิเมชันตอนเล่นจบเฟรมสุดท้าย
    public void DestroyBulletAfterAnim()
    {
        Destroy(gameObject);
    }
   
}
