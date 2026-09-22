using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;
    public GameObject impactEffect;

    [Header("Puzzle System")]
    public bool isPuzzleBullet = false;
    public string puzzleRecipe;

    [Header("AOE Settings")]
    public float aoeRadius = 2.5f;

    private Rigidbody2D rb;
    private bool isDeflected = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ==========================================
        // 🌪️ ระบบอัปเกรดธาตุลม (ขยายขนาด & เพิ่มความเร็ว)
        // ==========================================
        if (isPuzzleBullet && puzzleRecipe == "WindWindWind")
        {
            int buffLevel = BuffManager.instance != null ? BuffManager.instance.windLevel : 0;

            float sizeMultiplier = 1f + (buffLevel * 0.2f);
            sizeMultiplier = Mathf.Min(sizeMultiplier, 2f);
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * sizeMultiplier, transform.localScale.z);

            float speedMultiplier = 1f + (buffLevel * 0.1f);
            speedMultiplier = Mathf.Min(speedMultiplier, 1.5f);
            speed = speed * speedMultiplier;
        }

        rb.velocity = transform.right * speed;
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (isDeflected) return;

        // ==========================================
        // 💣 1. ถ้ายิงพายุเพลิง (ระเบิดหมู่ AOE)
        // ==========================================
        if (isPuzzleBullet && puzzleRecipe == "FireFireWind")
        {
            if (hitInfo.CompareTag("Enemy") || hitInfo.CompareTag("Boss") || hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground") || hitInfo.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                ExplodeAOE();
                ExplodeAndDestroy();
            }
            return;
        }

        // ==========================================
        // 🗡️ 2. ถ้ายิงธาตุปกติ หรือ ท่าคลื่นลมทะลวง (Air Slash)
        // ==========================================
        bool hitSomething = false;

        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            hitSomething = true;
            // 🌟 แก้ไข: ถ้าเป็นกระสุนพัซเซิล ให้ใช้คำสั่งเจาะเกราะ ถ้าไม่ใช่ให้ทำดาเมจปกติ
            if (isPuzzleBullet) enemy.CheckPuzzleBullet(puzzleRecipe, (int)damage);
            else enemy.TakeDamage((int)damage);
        }

        FlyingEnemy flyingEnemy = hitInfo.GetComponent<FlyingEnemy>();
        if (flyingEnemy != null && !hitSomething)
        {
            hitSomething = true;
            // 🌟 แก้ไข: นกบินยังไม่มีระบบเกราะ ให้ทำดาเมจปกติไปก่อน
            flyingEnemy.TakeDamage((int)damage);
        }

        Boss boss = hitInfo.GetComponent<Boss>();
        if (boss != null && !hitSomething)
        {
            hitSomething = true;
            // 🌟 แก้ไข: บอสให้รับดาเมจปกติไปก่อนเช่นกัน
            boss.TakeDamage((int)damage);
        }

        // 🌟 3. สรุปผลลัพธ์หลังชน
        if (hitSomething)
        {
            bool isAirSlash = (isPuzzleBullet && puzzleRecipe == "WindWindWind");

            if (!isAirSlash)
            {
                ExplodeAndDestroy();
            }
        }
        else if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground") || hitInfo.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            ExplodeAndDestroy();
        }
    }

    // ==========================================
    // 💥 ฟังก์ชันระเบิดหมู่
    // ==========================================
    void ExplodeAOE()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, aoeRadius);

        foreach (Collider2D col in hitEnemies)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 🌟 แก้ไข: กระจายแรงระเบิดให้เช็กระบบเจาะเกราะด้วย
                if (isPuzzleBullet) enemy.CheckPuzzleBullet(puzzleRecipe, (int)damage);
                else enemy.TakeDamage((int)damage);
            }

            FlyingEnemy flyingEnemy = col.GetComponent<FlyingEnemy>();
            if (flyingEnemy != null) flyingEnemy.TakeDamage((int)damage);

            Boss boss = col.GetComponent<Boss>();
            if (boss != null) boss.TakeDamage((int)damage);
        }
    }

    private void ExplodeAndDestroy()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Explode");
        }

        Destroy(gameObject);
    }

    private void DeflectBullet()
    {
        isDeflected = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.gray;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 2f;

            float bounceDirection = (transform.right.x > 0) ? -1f : 1f;
            float bounceX = bounceDirection * Random.Range(2f, 4f);
            float bounceY = Random.Range(3f, 6f);
            rb.velocity = new Vector2(bounceX, bounceY);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 1.5f);
    }

    public void DestroyBulletAfterAnim()
    {
        Destroy(gameObject);
    }
}