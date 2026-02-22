using UnityEngine;
using System.Collections;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement & AI")]
    public float flySpeed = 2.5f;      // ความเร็วในการบิน
    public float detectRange = 20f;    // ระยะมองเห็น (ตั้งไว้กว้างๆ เลย)

    private Transform player;
    private Rigidbody2D rb;

    [Header("Health & Shield")]
    public int maxHealth = 15;         // เลือดน้อยกว่าตัวบนดินนิดนึง
    private int currentHealth;
    public int maxShield = 5;
    private int currentShield;

    [Header("Break System")]
    public bool isBroken = false;
    public float breakDuration = 3f;

    [Header("Element Setup")]
    public PlayerCombat.Element enemyElement;
    private SpriteRenderer spriteRenderer;

    [Header("UI")]
    public GameObject damagePopupPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // 🌟 ปิดแรงโน้มถ่วงให้เป็น 0 เพื่อให้มันลอยได้!
        rb.gravityScale = 0f;

        UpdateColor();
    }

    void Update()
    {
        // ค้นหาผู้เล่น
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            return;
        }

        // หันหน้าหาผู้เล่น
        if (player.position.x > transform.position.x)
            transform.eulerAngles = new Vector3(0, 0, 0);
        else
            transform.eulerAngles = new Vector3(0, 180f, 0);
    }

    void FixedUpdate()
    {
        if (isBroken || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectRange)
        {
            // 🦇 บินตรงดิ่งไปหาผู้เล่นเลย (คำนวณเวกเตอร์ทิศทาง)
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * flySpeed;
        }
        else
        {
            rb.velocity = Vector2.zero; // อยู่นอกระยะก็บินลอยโง่ๆ อยู่กับที่
        }
    }

    // ==========================================
    // ระบบต่อสู้และการโดนตี (เหมือนตัวบนดินเป๊ะ)
    // ==========================================

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBroken)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

            if (playerHealth != null && playerController != null)
            {
                playerController.Knockback(transform);
                playerHealth.TakeDamage(1);
            }
        }
    }

    void UpdateColor()
    {
        if (isBroken) { spriteRenderer.color = Color.gray; return; }
        switch (enemyElement)
        {
            case PlayerCombat.Element.Red: spriteRenderer.color = Color.red; break;
            case PlayerCombat.Element.Green: spriteRenderer.color = Color.green; break;
            case PlayerCombat.Element.Blue: spriteRenderer.color = Color.blue; break;
        }
    }

    public void TakeDamage(int damage, PlayerCombat.Element hitElement)
    {
        if (isBroken)
        {
            currentHealth -= damage;
            ShowDamagePopup(damage, Color.white, 4f);
            if (currentHealth <= 0) Die();
        }
        else
        {
            bool isWeakness = false;
            if (hitElement == PlayerCombat.Element.Red && enemyElement == PlayerCombat.Element.Green) isWeakness = true;
            else if (hitElement == PlayerCombat.Element.Green && enemyElement == PlayerCombat.Element.Blue) isWeakness = true;
            else if (hitElement == PlayerCombat.Element.Blue && enemyElement == PlayerCombat.Element.Red) isWeakness = true;

            if (isWeakness)
            {
                currentShield -= damage;
                ShowDamagePopup(damage, Color.yellow, 5f);
                if (currentShield <= 0) BreakArmor();
            }
            else ShowDamagePopup(0, Color.gray, 3f);
        }
    }

    void BreakArmor()
    {
        isBroken = true;
        UpdateColor();

        // 💥 ลูกเล่นสะใจ: เกราะแตกปุ๊บ เปิดแรงโน้มถ่วงให้ร่วงลงพื้น!
        rb.gravityScale = 3f;

        StartCoroutine(RecoverShieldRoutine());
    }

    IEnumerator RecoverShieldRoutine()
    {
        yield return new WaitForSeconds(breakDuration);
        if (currentHealth > 0)
        {
            isBroken = false;
            currentShield = maxShield;
            UpdateColor();

            // ฟื้นตัวแล้ว ปิดแรงโน้มถ่วงให้ลอยขึ้นไปใหม่
            rb.gravityScale = 0f;
        }
    }

    void ShowDamagePopup(int damageAmount, Color textColor, float fontSize)
    {
        if (damagePopupPrefab != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, 0);
            GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
            popup.GetComponent<DamagePopup>().SetupCustom(damageAmount, textColor, fontSize);
        }
    }

    void Die() { Destroy(gameObject); }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}