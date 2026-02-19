using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 200;
    private int currentHealth;

    [Header("Movement (ลอยตัว & วาร์ป)")]
    public float floatAmplitude = 0.5f; // ลอยขึ้นลงระยะแค่ไหน
    public float floatFrequency = 2f;   // ลอยเร็วแค่ไหน
    public Transform[] teleportPoints;  // จุดวาร์ปต่างๆ ในห้องบอส
    private Vector3 currentCenterPos;   // จุดศูนย์กลางที่บอสลอยอยู่ ณ ปัจจุบัน

    [Header("Bullet Hell (สาดกระสุน)")]
    public GameObject bossBulletPrefab; // ลาก Prefab กระสุนบอสมาใส่ช่องนี้
    public int bulletAmount = 8;        // จำนวนกระสุนที่ยิงออกรอบตัว (ยิ่งเยอะยิ่งหลบยาก)
    public float bulletSpeed = 4f;      // ความเร็วกระสุน

    [Header("Phase 2 Settings (Pillars)")]
    public GameObject pillarPrefab;
    public Transform[] pillarSpawnPoints;

    private bool isPhase2 = false;
    private bool isShielded = false;
    private int activePillars = 0;

    [Header("Gimmick (Phase 1)")]
    public float shiftInterval = 3f;
    public PlayerCombat.Element currentElement;

    [Header("Entrance Settings")]
    public float delayBeforeSpawn = 5f; // เวลาที่จะรอให้บอสปรากฏตัว (วินาที)
    private bool hasSpawned = false;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentCenterPos = transform.position; // จำจุดเริ่มต้นไว้ลอยตัว

        //StartCoroutine(ElementShiftRoutine());
        // 1. เริ่มต้นให้บอส "ล่องหน" และ "ปิดระบบชน" ไว้ก่อน
        spriteRenderer.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // 2. เริ่มนับถอยหลังเปิดตัว
        StartCoroutine(BossEntranceRoutine());
    }

    void Update()
    {
        // ให้บอสลอยขึ้นลงเบาๆ ตลอดเวลาด้วยสมการ Sine Wave (ทำให้ดูมีชีวิต)
        if (!isPhase2)
        {
            transform.position = currentCenterPos + new Vector3(0, Mathf.Sin(Time.time * floatFrequency) * floatAmplitude, 0);
        }
    }

    IEnumerator ElementShiftRoutine()
    {
        while (currentHealth > 0 && !isPhase2)
        {
            // 1. ระบบวาร์ป (ดึงแกน Z ไว้เหมือนเดิมกันทะลุมิติ)
            if (teleportPoints.Length > 0 && teleportPoints[0] != null)
            {
                Transform randomPoint = teleportPoints[Random.Range(0, teleportPoints.Length)];
                currentCenterPos = new Vector3(randomPoint.position.x, randomPoint.position.y, transform.position.z);
                transform.position = currentCenterPos;
            }

            // 2. สุ่มเปลี่ยนสี
            int randomElement = Random.Range(0, 3);
            currentElement = (PlayerCombat.Element)randomElement;
            UpdateBossColor();

            // 3. หน่วงเวลาชาร์จพลังแป๊บนึง (0.5 วินาที) แล้วสาดกระสุน!
            yield return new WaitForSeconds(0.5f);
            ShootBulletHell();

            // รอจนกว่าจะถึงรอบวาร์ปครั้งต่อไป
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

            if (currentHealth <= maxHealth / 2 && !isPhase2)
            {
                StartPhase2();
            }
        }

        if (currentHealth <= 0)
        {
            Debug.Log("บอสตาย!");
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
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

    // ==========================================
    // ระบบ Phase 2 (เรียกเสา)
    // ==========================================
    void StartPhase2()
    {   StopAllCoroutines();
        isPhase2 = true;
        isShielded = true;
       

        // 🛑 แก้ตรงนี้: บังคับให้ดึงมาแค่แกน X กับ Y ส่วนแกน Z ให้ใช้ของบอสตัวเดิม 
        // ป้องกันบอสวาร์ปทะลุไปหลังกล้องตอนเข้าเฟส 2
        currentCenterPos = new Vector3(teleportPoints[0].position.x, teleportPoints[0].position.y, transform.position.z);
        transform.position = currentCenterPos;

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

        // คำนวณองศาที่จะยิงกระจายออกรอบทิศทาง
        float angleStep = 360f / bulletAmount;
        float angle = 0f;

        for (int i = 0; i < bulletAmount; i++)
        {
            // สั่งหมุนกระสุนไปตามทิศต่างๆ (แกน Z)
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            // สร้างกระสุน
            GameObject bullet = Instantiate(bossBulletPrefab, transform.position, rotation);

            // ส่งข้อมูลความเร็วและ "สีธาตุ" ไปให้กระสุน
            BossBullet bulletScript = bullet.GetComponent<BossBullet>();
            bulletScript.speed = bulletSpeed;
            bulletScript.Setup(currentElement); // กระสุนจะเป็นสีเดียวกับบอสตอนนั้นเป๊ะๆ

            angle += angleStep; // ขยับมุมไปยิงนัดถัดไป
        }
    }
    IEnumerator Phase2ActionRoutine()
    {
        // บอสจะหยุดวาร์ปในเฟสนี้ แต่จะสาดกระสุนไม่หยุด
        while (isPhase2 && activePillars > 0)
        {
            // 1. สุ่มสีกระสุนที่บอสจะยิงออกมา (หรือจะล็อกเป็นสีขาวเพื่อให้หลบอย่างเดียวก็ได้)
            int randomElement = Random.Range(0, 3);
            currentElement = (PlayerCombat.Element)randomElement;

            // อัปเดตสีบอสให้ผู้เล่นรู้ว่าต้องใช้สีอะไรกันกระสุน
            UpdateBossColor();

            // 2. สาดกระสุนรอบทิศทาง (Bullet Hell)
            ShootBulletHell();

            // 3. รอจังหวะ (ปรับเวลาตามความยากที่ต้องการ ยิ่งน้อยยิ่งยิงถี่)
            yield return new WaitForSeconds(2.0f);
        }
    }
    IEnumerator BossEntranceRoutine()
    {
        Debug.Log("Waiting for Boss...");
        yield return new WaitForSeconds(delayBeforeSpawn);

        // 1. ตั้งตำแหน่งไปที่จุดเปิดตัว (TP 0) โดยที่ยังไม่เปิดภาพ
        if (teleportPoints.Length > 0)
        {
            transform.position = new Vector3(teleportPoints[0].position.x, teleportPoints[0].position.y, transform.position.z);
        }

        // 2. เอฟเฟกต์ Fade In (ค่อยๆ ชัดขึ้น)
        spriteRenderer.enabled = true;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            // ปรับความโปร่งใสจาก 0 ไป 1
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, t);
            yield return null;
        }

        // 3. เปิดระบบชนและเริ่มทำงาน
        GetComponent<Collider2D>().enabled = true;
        hasSpawned = true;

        Debug.Log("บอสโผล่มาแล้ว!");

        // เริ่มระบบเฟส 1 ทันที
        StartCoroutine(ElementShiftRoutine());
    }

    /*IEnumerator ElementShiftRoutine()
    {
        while (currentHealth > 0 && !isPhase2 && hasSpawned)
        {
            // ... โค้ดวาร์ปและยิงเดิมของคุณ ...
            yield return new WaitForSeconds(shiftInterval);
        }
    }*/ 
    
}
