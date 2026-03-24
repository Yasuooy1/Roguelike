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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. เช็กมอนสเตอร์เดินดิน
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (isPuzzleBullet)
            {
                if (!enemy.isBroken) enemy.CheckPuzzleBullet(puzzleRecipe, (int)damage);
                else enemy.TakeDamage((int)damage);
            }
            else enemy.TakeDamage((int)damage);
        }

        // 2. เช็กมอนสเตอร์บิน (นก)
        FlyingEnemy flyingEnemy = hitInfo.GetComponent<FlyingEnemy>();
        if (flyingEnemy != null)
        {
            if (isPuzzleBullet)
            {
                if (!flyingEnemy.isBroken) flyingEnemy.CheckPuzzleBullet(puzzleRecipe, (int)damage);
                else flyingEnemy.TakeDamage((int)damage);
            }
            else flyingEnemy.TakeDamage((int)damage);
        }

        // 3. เช็กบอสหลัก
        Boss boss = hitInfo.GetComponent<Boss>();
        if (boss != null)
        {
            if (isPuzzleBullet)
            {
                if (!boss.isBroken) boss.CheckPuzzleBullet(puzzleRecipe, (int)damage);
                else boss.TakeDamage((int)damage);
            }
            else boss.TakeDamage((int)damage);
        }

        // 4. สั่งทำลายกระสุน
        if (enemy != null || boss != null || flyingEnemy != null || hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (impactEffect != null) Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}