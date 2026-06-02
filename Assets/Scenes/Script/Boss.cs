using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 200;
    private int currentHealth;

    [Header("Movement (เฟส 1 บินตาม)")]
    public float smoothTime = 1.2f;
    public float hoverHeight = 3.5f;
    public float floatAmplitude = 0.5f;
    public float floatFrequency = 2f;
    private Vector3 currentCenterPos;
    private Vector3 velocity = Vector3.zero;

    [Header("Bullet Hell (สาดกระสุน)")]
    public GameObject bossBulletPrefab;
    public int bulletAmount = 8;
    public float bulletSpeed = 4f;

    [Header("🧩 Puzzle System (รหัสเจาะเกราะ)")]
    public float attackInterval = 3f;
    public string requiredRecipe;
    public bool isBroken = false;
    public float breakDuration = 5f;

    [Header("UI ลูกแก้วบอส (บนหัว)")]
    public GameObject puzzleCanvas;
    public Image[] puzzleSlots;
    public Sprite fireSprite;
    public Sprite waterSprite;
    public Sprite lightningSprite;

    [Header("Entrance Settings")]
    public float delayBeforeSpawn = 3f;
    private bool hasSpawned = false;
    private SpriteRenderer spriteRenderer;

    [Header("UI โชว์ตัวเลขดาเมจ")]
    public GameObject damagePopupPrefab;

    // ==========================================
    // 🌟 เพิ่มตัวแปรสำหรับแอนิเมชันร่ายเวทแบล็คโฮล
    // ==========================================
    [Header("Animation & Blackhole (เฟส 2)")]
    public Animator bossAnim;
    public GameObject backBlackhole; // ลากออบเจกต์แบล็คโฮลลูกมาใส่
    public GameObject blackholeProjectilePrefab; // ลาก Prefab ลูกบอลแบล็คโฮลทึบๆ มาใส่
    public Transform firePoint;

    private Transform playerTransform;
    private bool isAttacking = false;

    // ตัวแปรสถานะ
    private bool isPhase2 = false;
    private bool isPullingPlayer = false;

    private bool isUninterruptible = false; //ล็อกสถานะห้ามโดนขัดตอนร่ายเวท!

    [Header("⚡ Laser Effect (เฟส 2)")]
    public GameObject laserMuzzleFlash; // วงเวทสามเหลี่ยม (image_1.png)
    public GameObject laserBeamLine;    // 🌟 ลากออบเจกต์ LaserBeam_Line มาใส่ช่องนี้!
    public bool isLaserUninterruptible = false;

    [Header("🎵 ระบบเพลงบอส (BGM)")]
    public AudioSource bgmSource; // ลำโพงฉาก (ลาก BGM_Manager มาใส่)
    public AudioClip bossMusic;   // แผ่นเพลงตอนบอสออก
    private AudioClip originalBGM; // 🌟 ความจำ: เก็บเพลงด่านเก่าเอาไว้
    private float defaultVolume;

    [Header("🔊 เสียง Sound Effects")]
    public AudioSource bossAudio; // ลำโพงบอส (เดี๋ยวเราจะไป Add Component กัน)
    public AudioClip laserSound;  // 🌟 ช่องใส่เสียง LASER.wav
    public AudioClip blackholeSound;

    private Animator bossAnimator;
    [Header("🔊 เสียงเจ็บ & ตาย (Boss)")]
    public AudioClip hurtSound;     // เสียงโดนตี
    public AudioClip deathSound;    // เสียงตอนตาย
    public AudioClip victorySound;  // 🌟 เสียงแตรฉลอง (Victory!)

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        bossAudio = GetComponent<AudioSource>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        GameObject bgmObj = GameObject.Find("BGM_Manager");
        if (bgmObj != null)
        {
            bgmSource = bgmObj.GetComponent<AudioSource>();
            if (bgmSource != null)
            {
                // 🌟 ให้บอสจดจำแผ่นเพลงเก่า และความดังเก่า เอาไว้ก่อน!
                originalBGM = bgmSource.clip;
                defaultVolume = bgmSource.volume;
            }

            currentCenterPos = transform.position;
            spriteRenderer.enabled = false;
            GetComponent<Collider2D>().enabled = false;
            if (puzzleCanvas != null) puzzleCanvas.SetActive(false);

            // ซ่อนแบล็คโฮลตอนเริ่มเกม
            if (backBlackhole != null) backBlackhole.SetActive(false);

            StartCoroutine(BossEntranceRoutine());
        }
        bossAnimator = GetComponent<Animator>();
    }

        void Update()
        {
            if (hasSpawned && !isBroken && !isAttacking)
            {
                // ระบบบินตามหน่วงๆ
                if (playerTransform != null)
                {
                    Vector3 targetPos = playerTransform.position + new Vector3(0, hoverHeight, 0);
                    currentCenterPos = Vector3.SmoothDamp(currentCenterPos, targetPos, ref velocity, smoothTime);
                }
                transform.position = currentCenterPos + new Vector3(0, Mathf.Sin(Time.time * floatFrequency) * floatAmplitude, 0);
                FacePlayer();
            }

            // เอฟเฟกต์ท่าหลุมดำ 
            if (isPhase2 && hasSpawned && isPullingPlayer && playerTransform != null)
            {
                playerTransform.position = Vector3.MoveTowards(playerTransform.position, transform.position, 2.5f * Time.deltaTime);
            }
        }

        void FacePlayer()
        {
            if (playerTransform != null)
            {
                if (playerTransform.position.x < transform.position.x) spriteRenderer.flipX = true;
                else spriteRenderer.flipX = false;
            }
        }

        IEnumerator BossEntranceRoutine()
        {
            // 🌟 1. เฟดเพลงด่านให้ค่อยๆ เงียบลง (หน่วงอารมณ์ก่อนบอสออก)
            if (bgmSource != null)
            {
                float fadeOutTime = 1.5f; // ใช้เวลาหรี่เสียง 1.5 วินาที
                float tFade = 0;
                while (tFade < fadeOutTime)
                {
                    tFade += Time.deltaTime;
                    bgmSource.volume = Mathf.Lerp(defaultVolume, 0f, tFade / fadeOutTime);
                    yield return null;
                }
            }

            // ให้จังหวะเงียบๆ บิลด์อารมณ์เดดแอร์สัก 1 วินาที
            yield return new WaitForSeconds(1.0f);

            // 🌟 2. เริ่มเปิดเพลงบอสเต็มหลอด! พร้อมกับบอสปรากฏตัว!
            if (bgmSource != null && bossMusic != null)
            {
                bgmSource.clip = bossMusic;
                bgmSource.volume = defaultVolume; // คืนความดังกลับมา
                bgmSource.Play();
            }

            // บอสค่อยๆ โผล่มา (โค้ดเดิม)
            spriteRenderer.enabled = true;
            float t = 0;
            while (t < 1f) { t += Time.deltaTime; spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, t); yield return null; }

            GetComponent<Collider2D>().enabled = true;
            hasSpawned = true;
            GenerateRandomPuzzle();
            if (BossHealthUI.instance != null) BossHealthUI.instance.ShowBossUI(maxHealth);
            StartCoroutine(ActionRoutine());
        }

        void GenerateRandomPuzzle()
        {
            string[] elements = { "Fire", "Water", "Lightning" };
            List<string> puzzleList = new List<string>();
            puzzleList.Add(elements[Random.Range(0, 3)]);
            puzzleList.Add(elements[Random.Range(0, 3)]);
            puzzleList.Add(elements[Random.Range(0, 3)]);
            puzzleList.Sort();
            requiredRecipe = puzzleList[0] + puzzleList[1] + puzzleList[2];

            for (int i = 0; i < puzzleSlots.Length; i++)
            {
                if (puzzleList[i] == "Fire") puzzleSlots[i].sprite = fireSprite;
                else if (puzzleList[i] == "Water") puzzleSlots[i].sprite = waterSprite;
                else if (puzzleList[i] == "Lightning") puzzleSlots[i].sprite = lightningSprite;
            }
            if (puzzleCanvas != null) puzzleCanvas.SetActive(true);
        }

        IEnumerator ActionRoutine()
        {
            while (currentHealth > 0 && hasSpawned)
            {
                float currentWait = isPhase2 ? attackInterval * 0.7f : attackInterval;
                yield return new WaitForSeconds(currentWait);

                if (isBroken || isAttacking) continue;

                FacePlayer();

                int randomAttack = Random.Range(0, 3);

                if (!isPhase2)
                {
                    if (randomAttack == 0) yield return StartCoroutine(BulletHellRoutine());
                    else if (randomAttack == 1) yield return StartCoroutine(ShotgunRoutine());
                    else if (randomAttack == 2) yield return StartCoroutine(DashAttackRoutine());
                }
                else
                {
                    if (randomAttack == 0) yield return StartCoroutine(LaserSweepRoutine());
                    else if (randomAttack == 1) yield return StartCoroutine(GroundSlamShockwaveRoutine());
                    else if (randomAttack == 2) yield return StartCoroutine(BlackHoleRoutine());
                }
            }
        }

        // ... (ท่าเก่าๆ เฟส 1 ให้คงเดิมตามโค้ดของคุณอาร์ม) ...
        IEnumerator BulletHellRoutine()
        {
            if (bossBulletPrefab == null || playerTransform == null) yield break;

            isAttacking = true;
            FacePlayer();

            spriteRenderer.color = Color.yellow;
            yield return new WaitForSeconds(0.7f);

            float angleStep = 360f / bulletAmount;
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            float angle = baseAngle;

            for (int i = 0; i < bulletAmount; i++)
            {
                Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                GameObject bullet = Instantiate(bossBulletPrefab, transform.position, rotation);
                bullet.GetComponent<BossBullet>().speed = bulletSpeed;
                angle += angleStep;
            }

            if (!isBroken) spriteRenderer.color = isPhase2 ? Color.cyan : Color.white;
            isAttacking = false;
        }

        IEnumerator ShotgunRoutine()
        {
            if (bossBulletPrefab == null || playerTransform == null) yield break;

            isAttacking = true;
            FacePlayer();

            spriteRenderer.color = new Color(1f, 0.5f, 0f);
            yield return new WaitForSeconds(0.6f);

            Vector2 direction = (playerTransform.position - transform.position).normalized;
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            float[] spreadAngles = { -30f, -15f, 0f, 15f, 30f };

            foreach (float angleOffset in spreadAngles)
            {
                Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, baseAngle + angleOffset));
                GameObject bullet = Instantiate(bossBulletPrefab, transform.position, rotation);
                bullet.GetComponent<BossBullet>().speed = bulletSpeed + 3f;
            }

            if (!isBroken) spriteRenderer.color = isPhase2 ? Color.cyan : Color.white;
            isAttacking = false;
        }

        IEnumerator DashAttackRoutine()
        {
            if (playerTransform == null) yield break;
            isAttacking = true;
            FacePlayer();

            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.8f);

            Vector3 targetPosition = playerTransform.position;
            Vector3 startPosition = transform.position;
            float dashTime = 0.4f;
            float t = 0f;

            while (t < 1f)
            {
                if (isBroken) break;
                t += Time.deltaTime / dashTime;
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            if (!isBroken) spriteRenderer.color = isPhase2 ? Color.cyan : Color.white;
            velocity = Vector3.zero;
            currentCenterPos = transform.position;
            isAttacking = false;
        }

        IEnumerator LaserSweepRoutine()
        {
            if (playerTransform == null) yield break;

            isAttacking = true;
            isLaserUninterruptible = true;

            FacePlayer();
            spriteRenderer.color = Color.red;

            // บินขึ้นไปตั้งหลักด้านบน
            Vector3 sweepStartPos = new Vector3(transform.position.x, transform.position.y + 3f, 0);
            float t = 0;
            while (t < 1f) { transform.position = Vector3.Lerp(transform.position, sweepStartPos, t); t += Time.deltaTime * 3f; yield return null; }

            yield return new WaitForSeconds(0.5f);

            // เริ่มแอนิเมชันร่าย + เปิดวงเวทสามเหลี่ยม
            Debug.Log("ร่ายเวทเลเซอร์!");
            if (bossAnim != null) bossAnim.SetTrigger("CastLaser");
            yield return new WaitForSeconds(0.2f);
            if (laserMuzzleFlash != null) laserMuzzleFlash.SetActive(true);

            // ⏳ รอเวลาชาร์จ 1.5 วินาที
            yield return new WaitForSeconds(1.5f);

            // --- จังหวะยิงกวาดจอ ---
            Debug.Log("ยิงกวาดจอ!");
            if (laserMuzzleFlash != null) laserMuzzleFlash.SetActive(false); // ปิดแฟลชสามเหลี่ยม

            // 🌟 1. เปิดแสดงเส้นเลเซอร์
            if (laserBeamLine != null) laserBeamLine.SetActive(true);

            if (bossAudio != null && laserSound != null)
            {
             bossAudio.clip = laserSound;
             bossAudio.Play();
            }

        // หามุมเริ่มต้น (เล็งไปหาผู้เล่น)
        Vector2 direction = (playerTransform.position - transform.position).normalized;
            float centerAngleStandard = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // กวาดออกไป 45 องศา (ไปและกลับ)
            float sweepRange = 45f;
            float startAngle = centerAngleStandard - sweepRange;
            float endAngle = centerAngleStandard + sweepRange;
            float sweepDuration = 1.5f; // ใช้เวลากวาด 1.5 วินาที
            float timer = 0f;

            // 🌟 2. ลูปกวาดเลเซอร์สมูทๆ
            while (timer < sweepDuration && currentHealth > 0)
            {
                timer += Time.deltaTime;
                // คำนวณองศาจากเริ่มไปจบแบบเนียนๆ
                float currentAngleStandard = Mathf.Lerp(startAngle, endAngle, timer / sweepDuration);

                // สั่งหมุนเส้นเลเซอร์ (เนื่องจากรูปหันไปทางขวาอยู่แล้ว เลยไม่ต้อง -90 เหมือนกระสุนปกติ)
                if (laserBeamLine != null)
                {
                    laserBeamLine.transform.rotation = Quaternion.Euler(0, 0, currentAngleStandard);
                }

                yield return null; // ทำงานทุกเฟรม ทำให้กวาดจอเนียนกริ๊บ
            }

            // 🌟 3. กวาดจบ ปิดเส้นเลเซอร์
            if (laserBeamLine != null) laserBeamLine.SetActive(false);
            if (bossAudio != null) bossAudio.Stop();

        spriteRenderer.color = Color.cyan;
            isLaserUninterruptible = false;
            isAttacking = false;
        }

    IEnumerator GroundSlamShockwaveRoutine()
    {
        if (playerTransform == null) yield break;
        isAttacking = true;

        // 1. บินชูตขึ้นไปตั้งหลักเหนือผู้เล่น
        Vector3 highPos = new Vector3(playerTransform.position.x, playerTransform.position.y + 6f, 0);
        float t = 0;
        while (t < 1f)
        {
            transform.position = Vector3.Lerp(transform.position, highPos, t);
            t += Time.deltaTime * 3f;
            yield return null;
        }

        spriteRenderer.color = Color.red; // ตัวแดงเตือน
        yield return new WaitForSeconds(0.5f); // ค้างบนฟ้าให้ผู้เล่นลุ้น

        // 🌟 2. พุ่งกระแทกพื้น! (แก้จากวาร์ป เป็นพุ่งลงมาอย่างไว)
        Vector3 slamPos = new Vector3(transform.position.x, playerTransform.position.y, 0);
        float slamT = 0;
        while (slamT < 1f)
        {
            transform.position = Vector3.Lerp(highPos, slamPos, slamT);
            slamT += Time.deltaTime * 10f; // ความเร็วพุ่งลงมา (เลขยิ่งเยอะยิ่งพุ่งเร็ว)
            yield return null;
        }
        transform.position = slamPos; // ล็อกให้ติดพื้นชัวร์ๆ

        // (💡 ถ้ามีไฟล์เสียงทุบพื้น เอา bossAudio.PlayOneShot(...) มาแทรกตรงนี้ได้เลยครับ!)

        // 3. ยิงคลื่นกระสุนกระจายออกซ้าย-ขวา
        if (currentHealth > 0 && !isBroken)
        {
            for (int i = 0; i < 3; i++)
            {
                Quaternion rotL = Quaternion.Euler(0, 0, 90 - (i * 15));
                Instantiate(bossBulletPrefab, transform.position, rotL).GetComponent<BossBullet>().speed = bulletSpeed * 1.5f;
                Quaternion rotR = Quaternion.Euler(0, 0, -90 + (i * 15));
                Instantiate(bossBulletPrefab, transform.position, rotR).GetComponent<BossBullet>().speed = bulletSpeed * 1.5f;
            }
        }

        // 4. ค้างที่พื้นหลังทุบเสร็จแป๊บนึง (1 วินาที)
        yield return new WaitForSeconds(1f);
        if (!isBroken) spriteRenderer.color = Color.cyan;

        // 🌟🌟 5. จุดสำคัญ! อัปเดตจุดศูนย์กลางใหม่ บอสจะได้ลอยขึ้นจากตรงที่เพิ่งทุบ 🌟🌟
        currentCenterPos = transform.position;

        isAttacking = false;
    }

    // ==========================================
    // 🌟 3. ท่าแบล็คโฮล (แก้ไขให้เรียกแอนิเมชันและปล่อยลูกแบล็คโฮล)
    // ==========================================
    // ==========================================
    // 🌟 ท่าแบล็คโฮล (ขยายขนาดสมูทๆ + หมุนตลอดเวลา)
    // ==========================================
    IEnumerator BlackHoleRoutine()
        {
            if (blackholeProjectilePrefab == null) yield break;

            isAttacking = true;
            isUninterruptible = true; // 🛡️ เปิดโหมดกันขัด

            FacePlayer();

            Vector3 centerPos = transform.position;
            float t = 0;
            while (t < 1f) { transform.position = Vector3.Lerp(transform.position, centerPos, t); t += Time.deltaTime * 2f; yield return null; }

            Debug.Log("ร่ายเวทแบล็คโฮล!");
            if (bossAnim != null) bossAnim.SetTrigger("Cast");

            yield return new WaitForSeconds(0.2f);

            // 🌟 1. เปิดหลุมดำ และเซ็ตขนาดให้เป็น 0 ก่อน
            if (backBlackhole != null)
            {
                backBlackhole.SetActive(true);
                backBlackhole.transform.localScale = Vector3.zero; // เริ่มจากมองไม่เห็น
            }
        // 🌟🌟 สั่งเล่นเสียงแบล็คโฮลตรงนี้เลย! 🌟🌟
        if (bossAudio != null && blackholeSound != null)
        {
            bossAudio.clip = blackholeSound;
            bossAudio.Play();
        }

        isPullingPlayer = true; // เริ่มดูดผู้เล่น

            // 🌟 2. โค้ดสั่งค่อยๆ ขยายขนาดจาก 0 ไป 5 (ใช้เวลา 2 วินาที)
            float growDuration = 2.0f; // เวลาที่ใช้ขยาย
            float growTimer = 0f;
            float targetScale = 5f; // ขนาดใหญ่สุดที่คุณอาร์มต้องการ (เปลี่ยนเป็นเลขอื่นได้)

            while (growTimer < growDuration)
            {
                growTimer += Time.deltaTime;
                // คำนวณขนาดปัจจุบัน (Lerp)
                float currentScale = Mathf.Lerp(0f, targetScale, growTimer / growDuration);

                // อัปเดตขนาดให้หลุมดำ
                if (backBlackhole != null)
                {
                    backBlackhole.transform.localScale = new Vector3(currentScale, currentScale, 1f);
                }
                yield return null; // รอเฟรมถัดไป (ทำให้สมูท)
            }

            // รออีก 1 วินาที ให้ร่ายครบ 3 วินาทีพอดี แล้วค่อยยิง
            yield return new WaitForSeconds(1.0f);

            // --- ยิงกระสุนแบล็คโฮล ---
            if (currentHealth > 0)
            {
                Vector2 dir = (playerTransform.position - firePoint.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.Euler(0, 0, angle);

                Instantiate(blackholeProjectilePrefab, firePoint.position, rot);
            }

            isPullingPlayer = false;
            if (backBlackhole != null) backBlackhole.SetActive(false); // ซ่อนหลุมดำ
        if (bossAudio != null) bossAudio.Stop();

        isUninterruptible = false;
            isAttacking = false;
        }


    // ==========================================
    // 💥 ระบบตรวจสอบรหัสเจาะเกราะ
    // ==========================================
    public bool CheckPuzzleBullet(string playerRecipe, int damage)
    {
        // 🌟 เปลี่ยนจาก return; ลอยๆ เป็น return false;
        if (!hasSpawned || isBroken) return false;

        if (isUninterruptible)
        {
            Debug.Log("🛡️ บอสร่ายเวทอยู่ โดนขัดไม่ได้!!");
            ShowDamagePopup(0, Color.gray, 3f);
            return false; // 🌟 เด้งออกและส่งค่า false กลับไปให้กระสุนเด้ง
        }

        if (playerRecipe == requiredRecipe)
        {
            ShowDamagePopup(damage, Color.yellow, 5f);
            BreakArmor();
            TakeDamage(damage);
            return true; // 🌟 รหัสถูก! ส่งค่า true บอกกระสุนว่าให้ระเบิดตัวเองเลย
        }
        else
        {
            ShowDamagePopup(0, Color.red, 3f);
            return false; // 🌟 รหัสผิด! ส่งค่า false บอกกระสุนว่าให้กระเด้งกลับไป
        }
    }


    void ShowDamagePopup(int damageAmount, Color textColor, float fontSize)
    {
        if (damagePopupPrefab != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-1f, 1f), 1f, 0);
            GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
            popup.GetComponent<DamagePopup>().SetupCustom(damageAmount, textColor, fontSize);
        }
    }



    public void ForceBreakArmor()
    {
        if (!hasSpawned || isBroken) return;
        BreakArmor();
    }

    void BreakArmor()
    {
        isBroken = true;
        isPullingPlayer = false;
        spriteRenderer.color = Color.gray;
        if (puzzleCanvas != null) puzzleCanvas.SetActive(false);

        // 🌟 ปิดแบล็คโฮลด้านหลังทิ้งเลยถ้าเกราะแตกตอนร่ายเวท
        if (backBlackhole != null) backBlackhole.SetActive(false);

        StartCoroutine(RecoverShieldRoutine());
    }

    IEnumerator RecoverShieldRoutine()
    {
        yield return new WaitForSeconds(breakDuration);
        if (currentHealth > 0)
        {
            isBroken = false;
            spriteRenderer.color = isPhase2 ? Color.cyan : Color.white;
            GenerateRandomPuzzle();
        }
    }

    public void TakeDamage(int damage)
    {
        if (!hasSpawned) return;
        if (isBroken)
        {
            // ==========================================
            // 🌟 แทรกเสียงเจ็บตรงนี้!
            // ==========================================
            if (bossAudio != null && hurtSound != null)
            {
                bossAudio.PlayOneShot(hurtSound);
            }

            currentHealth -= damage;
            ShowDamagePopup(damage, Color.white, 4f);
            if (BossHealthUI.instance != null) BossHealthUI.instance.UpdateHealth(currentHealth);

            if (currentHealth <= maxHealth / 2 && !isPhase2)
            {
                StartPhase2();
            }

            if (currentHealth <= 0)
            {
                if (BossHealthUI.instance != null) BossHealthUI.instance.HideBossUI();

                // หยุดการโจมตีทุกอย่างทันที (กันบั๊กเลเซอร์ค้างตอนบอสตาย)
                StopAllCoroutines();

                // เรียกใช้ฉากตาย
                StartCoroutine(BossDeathRoutine());
            }
        }
    }

    void StartPhase2()
    {
        isPhase2 = true;
        
        Debug.Log("⚠️ บอสเข้าสู่เฟส 2! เตรียมรับแรงกระแทก!");
        if (bossAnimator != null)
        {
            // ของเดิมที่ใช้สะกิดให้เปลี่ยนร่าง
            bossAnimator.SetTrigger("EnterPhase2");

            // 🌟 เพิ่มบรรทัดนี้! เพื่อชูตป้ายบอกทางว่า "ตอนนี้อยู่เฟส 2 ถาวรแล้วนะ!"
            bossAnimator.SetBool("IsPhase2", true);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && hasSpawned)
        {
            collision.gameObject.GetComponent<PlayerController>()?.Knockback(transform);
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(1);
        }
    }
    
    
    // ==========================================
    // 💀 ท่าตายของบอส (เล่นแอนิเมชันตาย + เฟดเพลงกลับ)
    // ==========================================
    IEnumerator BossDeathRoutine()
    {
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);
        }
        if (victorySound != null)
        {
            // เสกลำโพงล่องหนมาเล่นเสียงชนะให้ดังกระหึ่ม
            AudioSource.PlayClipAtPoint(victorySound, transform.position, 1.2f);
        }
        // 1. ปิดกล่องชนทันที
        GetComponent<Collider2D>().enabled = false;

        // 2. ปิดเอฟเฟกต์ท่าไม้ตายที่อาจจะค้างอยู่
        if (backBlackhole != null) backBlackhole.SetActive(false);
        if (laserMuzzleFlash != null) laserMuzzleFlash.SetActive(false);
        if (laserBeamLine != null) laserBeamLine.SetActive(false);

        // 🌟 3. สั่งเล่นแอนิเมชันตาย!
        if (bossAnim != null) bossAnim.SetTrigger("Die");

        // 🌟 4. ค่อยๆ เฟดเพลงบอสออก (ทำไปพร้อมๆ กับตอนเล่นแอนิเมชัน)
        float dieDuration = 2.5f; // ระยะเวลาให้สอดคล้องกับแอนิเมชัน
        float t = 0;
        while (t < dieDuration)
        {
            t += Time.deltaTime;
            if (bgmSource != null)
            {
                bgmSource.volume = Mathf.Lerp(defaultVolume, 0f, t / dieDuration);
            }
            yield return null;
        }

        // 5. คืนเพลงด่านเก่ากลับมา
        if (bgmSource != null && originalBGM != null)
        {
            bgmSource.clip = originalBGM;
            bgmSource.volume = defaultVolume;
            bgmSource.Play();
        }
        /*if (GameManager.instance != null)
        {
            GameManager.instance.LoadNextRandomMap();
        }*/

        // 6. บอสตายสมบูรณ์ ลบออกจากฉาก
        Destroy(gameObject);
    }
}
