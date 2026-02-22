using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 200;
    private int currentHealth;

    [Header("Movement (ลอยตัว & วาร์ป)")]
    public float floatAmplitude = 0.5f;
    public float floatFrequency = 2f;

    // 🌟 ซ่อนช่องนี้ไว้เลยเพราะเดี๋ยวโค้ดจะหาให้เองอัตโนมัติ
    [HideInInspector] public Transform[] teleportPoints;
    private Vector3 currentCenterPos;

    [Header("Bullet Hell (สาดกระสุน)")]
    public GameObject bossBulletPrefab;
    public int bulletAmount = 8;
    public float bulletSpeed = 4f;

    [Header("Phase 2 Settings (Pillars)")]
    public GameObject pillarPrefab;
    // 🌟 ซ่อนช่องนี้ไว้ด้วย หาเองอัตโนมัติเหมือนกัน
    [HideInInspector] public Transform[] pillarSpawnPoints;

    private bool isPhase2 = false;
    private bool isShielded = false;
    private int activePillars = 0;

    [Header("Gimmick (Phase 1)")]
    public float shiftInterval = 3f;
    public PlayerCombat.Element currentElement;

    [Header("Entrance Settings")]
    public float delayBeforeSpawn = 3f; // 🌟 ผมแอบปรับให้เหลือ 3 วิ จะได้เทสต์ง่ายๆ ครับ
    private bool hasSpawned = false;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ==================================================
        // 📡 ระบบเรดาร์: ค้นหาจุดวาร์ป และ จุดเกิดเสา อัตโนมัติ!
        // ==================================================
        GameObject[] tpObjs = GameObject.FindGameObjectsWithTag("BossTP");
        teleportPoints = new Transform[tpObjs.Length];
        for (int i = 0; i < tpObjs.Length; i++)
        {
            teleportPoints[i] = tpObjs[i].transform;
        }

        GameObject[] pillarObjs = GameObject.FindGameObjectsWithTag("BossPillarSpawn");
        pillarSpawnPoints = new Transform[pillarObjs.Length];
        for (int i = 0; i < pillarObjs.Length; i++)
        {
            pillarSpawnPoints[i] = pillarObjs[i].transform;
        }
        // ==================================================

        // เซ็ตจุดเริ่มต้นกันเหนียว
        if (teleportPoints.Length > 0) currentCenterPos = teleportPoints[0].position;
        else currentCenterPos = transform.position;

        // 1. เริ่มต้นให้บอส "ล่องหน" และ "ปิดระบบชน" ไว้ก่อน
        spriteRenderer.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // 2. เริ่มนับถอยหลังเปิดตัว
        StartCoroutine(BossEntranceRoutine());
    }

    void Update()
    {
        if (!isPhase2 && hasSpawned)
        {
            transform.position = currentCenterPos + new Vector3(0, Mathf.Sin(Time.time * floatFrequency) * floatAmplitude, 0);
        }
    }

    IEnumerator BossEntranceRoutine()
    {
        Debug.Log("Waiting for Boss...");
        yield return new WaitForSeconds(delayBeforeSpawn);

        if (teleportPoints.Length > 0 && teleportPoints[0] != null)
        {
            transform.position = new Vector3(teleportPoints[0].position.x, teleportPoints[0].position.y, transform.position.z);
            currentCenterPos = transform.position;
        }

        spriteRenderer.enabled = true;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, t);
            yield return null;
        }

        GetComponent<Collider2D>().enabled = true;
        hasSpawned = true;

        Debug.Log("บอสโผล่มาแล้ว!");
        

        // 🩸 โชว์หลอดเลือดบอส!
        if (BossHealthUI.instance != null) BossHealthUI.instance.ShowBossUI(maxHealth);

        // เริ่มระบบเฟส 1 ทันที
        StartCoroutine(ElementShiftRoutine());
        
    }

    IEnumerator ElementShiftRoutine()
    {
        while (currentHealth > 0 && !isPhase2 && hasSpawned)
        {
            if (teleportPoints.Length > 0)
            {
                Transform randomPoint = teleportPoints[Random.Range(0, teleportPoints.Length)];
                currentCenterPos = new Vector3(randomPoint.position.x, randomPoint.position.y, transform.position.z);
                transform.position = currentCenterPos;
            }

            int randomElement = Random.Range(0, 3);
            currentElement = (PlayerCombat.Element)randomElement;
            UpdateBossColor();

            yield return new WaitForSeconds(0.5f);
            ShootBulletHell();

            yield return new WaitForSeconds(shiftInterval);
        }
    }

    void UpdateBossColor()
    {
        if (isShielded)
        {
            spriteRenderer.color = Color.white;
            return;
        }

        switch (currentElement)
        {
            case PlayerCombat.Element.Red: spriteRenderer.color = Color.red; break;
            case PlayerCombat.Element.Green: spriteRenderer.color = Color.green; break;
            case PlayerCombat.Element.Blue: spriteRenderer.color = Color.blue; break;
        }
    }

    public void TakeDamage(int damage, PlayerCombat.Element hitElement)
    {
        if (!hasSpawned) return; // ถ้าบอสยังไม่เปิดตัว ห้ามตี!

        if (isShielded)
        {
            Debug.Log("บอสกางโล่อมตะอยู่! ต้องทำลายเสาก่อน!");
            return;
        }

        bool isWeakness = false;
        if (hitElement == PlayerCombat.Element.Red && currentElement == PlayerCombat.Element.Green) isWeakness = true;
        else if (hitElement == PlayerCombat.Element.Green && currentElement == PlayerCombat.Element.Blue) isWeakness = true;
        else if (hitElement == PlayerCombat.Element.Blue && currentElement == PlayerCombat.Element.Red) isWeakness = true;

        if (isWeakness)
        {
            currentHealth -= damage;
            Debug.Log("ยิงถูกจุดอ่อน! เลือดบอสเหลือ: " + currentHealth);

            // 🩸 อัปเดตหลอดเลือดให้ลดลง!
            if (BossHealthUI.instance != null) BossHealthUI.instance.UpdateHealth(currentHealth);

            if (currentHealth <= maxHealth / 2 && !isPhase2)
            {
                StartPhase2();
            }
        }

        if (currentHealth <= 0)
        {
            Debug.Log("บอสตาย!");

            // 🩸 ซ่อนหลอดเลือดทิ้งไป!
            if (BossHealthUI.instance != null) BossHealthUI.instance.HideBossUI();

            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && hasSpawned)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

            if (playerHealth != null && playerController != null)
            {
                playerController.Knockback(transform);
                playerHealth.TakeDamage(2);
            }
        }
    }

    void StartPhase2()
    {
        StopAllCoroutines();
        isPhase2 = true;
        isShielded = true;

        if (teleportPoints.Length > 0)
        {
            currentCenterPos = new Vector3(teleportPoints[0].position.x, teleportPoints[0].position.y, transform.position.z);
            transform.position = currentCenterPos;
        }

        UpdateBossColor();

        for (int i = 0; i < pillarSpawnPoints.Length; i++)
        {
            if (i >= 3) break;
            GameObject pillarObj = Instantiate(pillarPrefab, pillarSpawnPoints[i].position, Quaternion.identity);
            pillarObj.GetComponent<BossPillar>().SetupPillar(this, (PlayerCombat.Element)i);
            activePillars++;
        }
        StartCoroutine(Phase2ActionRoutine());
    }

    public void OnPillarDestroyed()
    {
        activePillars--;
        if (activePillars <= 0)
        {
            isShielded = false;
            currentElement = PlayerCombat.Element.Red;
            spriteRenderer.color = Color.gray;
        }
    }

    void ShootBulletHell()
    {
        if (bossBulletPrefab == null) return;

        float angleStep = 360f / bulletAmount;
        float angle = 0f;

        for (int i = 0; i < bulletAmount; i++)
        {
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            GameObject bullet = Instantiate(bossBulletPrefab, transform.position, rotation);
            BossBullet bulletScript = bullet.GetComponent<BossBullet>();
            bulletScript.speed = bulletSpeed;
            bulletScript.Setup(currentElement);
            angle += angleStep;
        }
    }

    IEnumerator Phase2ActionRoutine()
    {
        while (isPhase2 && activePillars > 0)
        {
            int randomElement = Random.Range(0, 3);
            currentElement = (PlayerCombat.Element)randomElement;
            UpdateBossColor();
            ShootBulletHell();
            yield return new WaitForSeconds(2.0f);
        }
    }
}