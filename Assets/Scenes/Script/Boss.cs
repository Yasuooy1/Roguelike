using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 200;
    private int currentHealth;

    [Header("ตัวคูณความแพ้ธาตุ (1 = ปกติ, 1.5 = แพ้, 0.5 = กัน)")]
    public float fireMultiplier = 1f;
    public float waterMultiplier = 1f;
    public float windMultiplier = 1f;

    [Header("ตั้งค่าไอคอนสถานะ")]
    public float iconOffsetY = 2.0f; // บอสตัวใหญ่ เอาไว้สูงๆ หน่อย
    public float iconSpacing = 0.8f;
    private List<GameObject> activeIcons = new List<GameObject>();

    [Header("Elemental VFX (เอฟเฟกต์ตอนติดสถานะ)")]
    public GameObject fireBurstVFX;
    public GameObject waterBurstVFX;
    public GameObject windBurstVFX;

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

    [Header("Action System")]
    public float attackInterval = 3f;
    private float currentAttackInterval; // ใช้เก็บค่า Cooldown เผื่อโดนสโลว์
    private float baseSmoothTime; // จำความเร็วก่อนโดนสโลว์

    [Header("Entrance Settings")]
    public float delayBeforeSpawn = 3f;
    private bool hasSpawned = false;
    private SpriteRenderer spriteRenderer;

    [Header("UI โชว์ตัวเลขดาเมจ")]
    public GameObject damagePopupPrefab;

    [Header("Animation & Blackhole (เฟส 2)")]
    public Animator bossAnim;
    public GameObject backBlackhole;
    public GameObject blackholeProjectilePrefab;
    public Transform firePoint;

    private Transform playerTransform;
    private bool isAttacking = false;

    // ตัวแปรสถานะ
    private bool isPhase2 = false;
    private bool isPullingPlayer = false;
    private bool isUninterruptible = false;

    // สถานะผิดปกติ
    private bool isDead = false;
    private bool isBurning = false;
    private bool isSlowed = false;
    private bool isStunned = false;

    [Header("⚡ Laser Effect (เฟส 2)")]
    public GameObject laserMuzzleFlash;
    public GameObject laserBeamLine;
    public bool isLaserUninterruptible = false;

    [Header("🎵 ระบบเพลงบอส (BGM)")]
    public AudioSource bgmSource;
    public AudioClip bossMusic;
    private AudioClip originalBGM;
    private float defaultVolume;

    [Header("🔊 เสียง Sound Effects")]
    public AudioSource bossAudio;
    public AudioClip laserSound;
    public AudioClip blackholeSound;

    private Animator bossAnimator;
    [Header("🔊 เสียงเจ็บ & ตาย (Boss)")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip victorySound;

    [Header("Warning System (ระบบเตือน)")]
    public GameObject dangerZonePrefab; // 🌟 ช่องสำหรับใส่ Prefab เป้าวงแดง

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossAudio = GetComponent<AudioSource>();
        bossAnimator = GetComponent<Animator>();

        baseSmoothTime = smoothTime;
        currentAttackInterval = attackInterval;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        GameObject bgmObj = GameObject.Find("BGM_Manager");
        if (bgmObj != null)
        {
            bgmSource = bgmObj.GetComponent<AudioSource>();
            if (bgmSource != null)
            {
                originalBGM = bgmSource.clip;
                defaultVolume = bgmSource.volume;
            }

            currentCenterPos = transform.position;
            spriteRenderer.enabled = false;
            GetComponent<Collider2D>().enabled = false;

            if (backBlackhole != null) backBlackhole.SetActive(false);

            StartCoroutine(BossEntranceRoutine());
        }
    }

    void Update()
    {
        if (hasSpawned && !isDead && !isAttacking && !isStunned)
        {
            if (playerTransform != null)
            {
                Vector3 targetPos = playerTransform.position + new Vector3(0, hoverHeight, 0);
                currentCenterPos = Vector3.SmoothDamp(currentCenterPos, targetPos, ref velocity, smoothTime);
            }
            transform.position = currentCenterPos + new Vector3(0, Mathf.Sin(Time.time * floatFrequency) * floatAmplitude, 0);
            FacePlayer();
        }

        if (isPhase2 && hasSpawned && isPullingPlayer && playerTransform != null && !isStunned)
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
        if (bgmSource != null)
        {
            float fadeOutTime = 1.5f;
            float tFade = 0;
            while (tFade < fadeOutTime)
            {
                tFade += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(defaultVolume, 0f, tFade / fadeOutTime);
                yield return null;
            }
        }

        yield return new WaitForSeconds(1.0f);

        if (bgmSource != null && bossMusic != null)
        {
            bgmSource.clip = bossMusic;
            bgmSource.volume = defaultVolume;
            bgmSource.Play();
        }

        spriteRenderer.enabled = true;
        float t = 0;
        while (t < 1f) { t += Time.deltaTime; spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, t); yield return null; }

        GetComponent<Collider2D>().enabled = true;
        hasSpawned = true;

        if (BossHealthUI.instance != null) BossHealthUI.instance.ShowBossUI(maxHealth);
        StartCoroutine(ActionRoutine());
    }

    IEnumerator ActionRoutine()
    {
        while (currentHealth > 0 && hasSpawned)
        {
            float waitTime = isPhase2 ? currentAttackInterval * 0.7f : currentAttackInterval;
            yield return new WaitForSeconds(waitTime);

            if (isDead || isAttacking || isStunned) continue;

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

    // ==========================================
    // ⚔️ ระบบรับดาเมจและคำนวณธาตุ
    // ==========================================
    public void TakeElementalDamage(int baseDamage, string spellRecipe)
    {
        if (!hasSpawned || isDead) return;

        // เช็กกันขัดท่าไม้ตาย
        if (isUninterruptible)
        {
            ShowDamagePopup(0, Color.gray, 3f);
            return;
        }

        float finalDamage = baseDamage;
        Color popupColor = Color.white;

        if (spellRecipe.Contains("Fire"))
        {
            finalDamage *= fireMultiplier;
            popupColor = new Color(1f, 0.5f, 0f);
            if (!isBurning && fireMultiplier >= 1f) StartCoroutine(ApplyBurn());
        }
        if (spellRecipe.Contains("Water"))
        {
            finalDamage *= waterMultiplier;
            popupColor = Color.cyan;
            if (!isSlowed && waterMultiplier >= 1f) StartCoroutine(ApplySlow());
        }
        if (spellRecipe.Contains("Wind"))
        {
            finalDamage *= windMultiplier;
            popupColor = Color.yellow;
            //if (!isStunned && lightningMultiplier >= 1f) StartCoroutine(ApplyStun());//
        }

        currentHealth -= (int)finalDamage;

        if (bossAudio != null && hurtSound != null) bossAudio.PlayOneShot(hurtSound);

        ShowDamagePopup((int)finalDamage, popupColor, 5f);
        if (BossHealthUI.instance != null) BossHealthUI.instance.UpdateHealth(currentHealth);

        if (currentHealth <= maxHealth / 2 && !isPhase2)
        {
            StartPhase2();
        }

        if (currentHealth <= 0)
        {
            if (BossHealthUI.instance != null) BossHealthUI.instance.HideBossUI();
            StopAllCoroutines();
            StartCoroutine(BossDeathRoutine());
        }
    }

    // รองรับระบบเก่า
    public void TakeDamage(int damage)
    {
        TakeElementalDamage(damage, "None");
    }

    // ==========================================
    // 🧪 ระบบสถานะผิดปกติและการเรียงไอคอน
    // ==========================================
    void RearrangeIcons()
    {
        activeIcons.RemoveAll(item => item == null);
        int count = activeIcons.Count;
        if (count == 0) return;

        float startX = -(count - 1) * iconSpacing / 2f;
        for (int i = 0; i < count; i++)
        {
            activeIcons[i].transform.localPosition = new Vector3(startX + (i * iconSpacing), iconOffsetY, 0);
        }
    }

    IEnumerator ApplyBurn()
    {
        isBurning = true;
        GameObject icon = null;
        SpriteRenderer iconSR = null;

        if (fireBurstVFX != null)
        {
            icon = Instantiate(fireBurstVFX, transform.position, Quaternion.identity, transform);
            iconSR = icon.GetComponent<SpriteRenderer>();
            activeIcons.Add(icon);
            RearrangeIcons();
        }

        for (int i = 0; i < 3; i++)
        {
            if (isDead) yield break;
            if (i == 2 && iconSR != null) StartCoroutine(BlinkIconRoutine(iconSR));
            yield return new WaitForSeconds(1f);

            // บอสโดนไฟเผาแรงกว่าปกตินิดนึง
            currentHealth -= 10;
            ShowDamagePopup(10, new Color(1f, 0.5f, 0f), 3f);
            if (BossHealthUI.instance != null) BossHealthUI.instance.UpdateHealth(currentHealth);

            if (currentHealth <= 0)
            {
                if (BossHealthUI.instance != null) BossHealthUI.instance.HideBossUI();
                StopAllCoroutines();
                StartCoroutine(BossDeathRoutine());
            }
        }

        if (icon != null)
        {
            activeIcons.Remove(icon);
            RearrangeIcons();
            Destroy(icon);
        }
        isBurning = false;
    }

    IEnumerator ApplySlow()
    {
        isSlowed = true;
        GameObject icon = null;
        SpriteRenderer iconSR = null;

        if (waterBurstVFX != null)
        {
            icon = Instantiate(waterBurstVFX, transform.position, Quaternion.identity, transform);
            iconSR = icon.GetComponent<SpriteRenderer>();
            activeIcons.Add(icon);
            RearrangeIcons();
        }

        if (spriteRenderer != null) spriteRenderer.color = Color.cyan;

        if (!isStunned)
        {
            smoothTime = baseSmoothTime * 2f; // ลอยอืดขึ้น 2 เท่า
            currentAttackInterval = attackInterval * 1.5f; // ออกท่าช้าลง
        }

        yield return new WaitForSeconds(2f);

        if (iconSR != null) StartCoroutine(BlinkIconRoutine(iconSR));
        yield return new WaitForSeconds(1f);

        if (!isStunned)
        {
            smoothTime = baseSmoothTime;
            currentAttackInterval = attackInterval;
        }

        if (spriteRenderer != null && !isDead) spriteRenderer.color = isPhase2 ? Color.cyan : Color.white;

        if (icon != null)
        {
            activeIcons.Remove(icon);
            RearrangeIcons();
            Destroy(icon);
        }
        isSlowed = false;
    }

    /*IEnumerator ApplyStun()
    {
        // 🌟 บอสมีความทนทาน สตันได้แค่ 0.5 วินาที เอาไว้ขัดจังหวะสั้นๆ
        isStunned = true;
        GameObject icon = null;
        SpriteRenderer iconSR = null;

        if (WindBurstVFX != null)
        {
            icon = Instantiate(lightningBurstVFX, transform.position, Quaternion.identity, transform);
            iconSR = icon.GetComponent<SpriteRenderer>();
            activeIcons.Add(icon);
            RearrangeIcons();
        }

        if (spriteRenderer != null) spriteRenderer.color = Color.yellow;
        velocity = Vector3.zero; // หยุดลอย

        if (iconSR != null) StartCoroutine(BlinkIconRoutine(iconSR));
        yield return new WaitForSeconds(0.5f); // บอสโดนสตันแป๊บเดียว

        if (spriteRenderer != null && !isDead) spriteRenderer.color = isPhase2 ? Color.cyan : Color.white;

        if (icon != null)
        {
            activeIcons.Remove(icon);
            RearrangeIcons();
            Destroy(icon);
        }
        isStunned = false;
    }*/

    IEnumerator BlinkIconRoutine(SpriteRenderer sr)
    {
        float timer = 0f;
        while (timer < 1f)
        {
            if (sr == null || isDead) yield break;
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.15f);
            timer += 0.15f;
        }
        if (sr != null) sr.enabled = true;
    }

    // ==========================================
    // กระบวนท่าโจมตีของบอส (อิงตามโค้ดเดิม)
    // ==========================================
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

        if (!isDead) spriteRenderer.color = isPhase2 ? Color.cyan : Color.white;
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

        if (!isDead) spriteRenderer.color = isPhase2 ? Color.cyan : Color.white;
        isAttacking = false;
    }

    IEnumerator DashAttackRoutine()
    {
        if (playerTransform == null) yield break;
        isAttacking = true;
        FacePlayer();
        // 🌟 เรียกใช้ระบบเตือน! (กระพริบ 1 วินาทีก่อนพุ่ง)
        yield return StartCoroutine(ShowWarningRoutine(1f));
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.8f);

        Vector3 targetPosition = playerTransform.position;
        Vector3 startPosition = transform.position;
        float dashTime = 0.4f;
        float t = 0f;

        while (t < 1f)
        {
            if (isDead) break;
            t += Time.deltaTime / dashTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        if (!isDead) spriteRenderer.color = isPhase2 ? Color.cyan : Color.white;
        velocity = Vector3.zero;
        currentCenterPos = transform.position;
        isAttacking = false;
    }

    IEnumerator LaserSweepRoutine()
    {
        if (playerTransform == null) yield break;
        isAttacking = true;
        isUninterruptible = true;

        FacePlayer();
        spriteRenderer.color = Color.red;

        Vector3 sweepStartPos = new Vector3(transform.position.x, transform.position.y + 3f, 0);
        float t = 0;
        while (t < 1f) { transform.position = Vector3.Lerp(transform.position, sweepStartPos, t); t += Time.deltaTime * 3f; yield return null; }

        yield return new WaitForSeconds(0.5f);
        if (bossAnim != null) bossAnim.SetTrigger("CastLaser");
        yield return new WaitForSeconds(0.2f);
        if (laserMuzzleFlash != null) laserMuzzleFlash.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        if (laserMuzzleFlash != null) laserMuzzleFlash.SetActive(false);
        if (laserBeamLine != null) laserBeamLine.SetActive(true);

        if (bossAudio != null && laserSound != null)
        {
            bossAudio.clip = laserSound;
            bossAudio.Play();
        }

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        float centerAngleStandard = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float sweepRange = 45f;
        float startAngle = centerAngleStandard - sweepRange;
        float endAngle = centerAngleStandard + sweepRange;
        float sweepDuration = 1.5f;
        float timer = 0f;

        while (timer < sweepDuration && currentHealth > 0)
        {
            timer += Time.deltaTime;
            float currentAngleStandard = Mathf.Lerp(startAngle, endAngle, timer / sweepDuration);
            if (laserBeamLine != null) laserBeamLine.transform.rotation = Quaternion.Euler(0, 0, currentAngleStandard);
            yield return null;
        }

        if (laserBeamLine != null) laserBeamLine.SetActive(false);
        if (bossAudio != null) bossAudio.Stop();

        spriteRenderer.color = Color.cyan;
        isUninterruptible = false;
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

        spriteRenderer.color = Color.red;

        // 🌟🌟 2. ล็อกเป้าหมายที่พื้น + เสกวงแดงเตือนผู้เล่น!
        Vector3 targetSlamPos = new Vector3(transform.position.x, playerTransform.position.y, 0);
        GameObject dangerZone = null;

        if (dangerZonePrefab != null)
        {
            // เสกวงแดงตรงจุดที่บอสจะตกลงมา
            dangerZone = Instantiate(dangerZonePrefab, targetSlamPos, Quaternion.identity);
        }

        // ค้างบนฟ้า 0.6 วินาที ให้ผู้เล่นเห็นวงแดงแล้วรีบกดหลบ!
        yield return new WaitForSeconds(0.6f);

        // 3. พุ่งกระแทกพื้นตรงจุดเป้าหมายวงแดงอย่างรวดเร็ว!
        float slamT = 0;
        while (slamT < 1f)
        {
            transform.position = Vector3.Lerp(highPos, targetSlamPos, slamT);
            slamT += Time.deltaTime * 10f;
            yield return null;
        }
        transform.position = targetSlamPos; // บังคับลงพื้นเป๊ะๆ

        // 🌟 ทุบถึงพื้นปุ๊บ ลบวงแดงเตือนทิ้งทันที
        if (dangerZone != null) Destroy(dangerZone);

        // (💡 ถ้ามีไฟล์เสียงทุบพื้น ให้เอา bossAudio.PlayOneShot(...) มาแทรกตรงนี้ได้เลย)

        // 4. ยิงคลื่นกระสุนกระจายออกซ้าย-ขวา
        if (currentHealth > 0 && !isDead)
        {
            for (int i = 0; i < 3; i++)
            {
                Quaternion rotL = Quaternion.Euler(0, 0, 90 - (i * 15));
                Instantiate(bossBulletPrefab, transform.position, rotL).GetComponent<BossBullet>().speed = bulletSpeed * 1.5f;
                Quaternion rotR = Quaternion.Euler(0, 0, -90 + (i * 15));
                Instantiate(bossBulletPrefab, transform.position, rotR).GetComponent<BossBullet>().speed = bulletSpeed * 1.5f;
            }
        }

        // 5. ค้างพักเหนื่อยที่พื้นแป๊บนึง
        yield return new WaitForSeconds(1f);
        if (!isDead) spriteRenderer.color = isPhase2 ? Color.cyan : Color.white;

        currentCenterPos = transform.position;
        isAttacking = false;
    }

    IEnumerator BlackHoleRoutine()
    {
        if (blackholeProjectilePrefab == null) yield break;
        isAttacking = true;
        isUninterruptible = true;

        FacePlayer();
        Vector3 centerPos = transform.position;
        float t = 0;
        while (t < 1f) { transform.position = Vector3.Lerp(transform.position, centerPos, t); t += Time.deltaTime * 2f; yield return null; }

        if (bossAnim != null) bossAnim.SetTrigger("Cast");
        yield return new WaitForSeconds(0.2f);

        if (backBlackhole != null)
        {
            backBlackhole.SetActive(true);
            backBlackhole.transform.localScale = Vector3.zero;
        }

        if (bossAudio != null && blackholeSound != null)
        {
            bossAudio.clip = blackholeSound;
            bossAudio.Play();
        }

        isPullingPlayer = true;
        float growDuration = 2.0f;
        float growTimer = 0f;
        float targetScale = 5f;

        while (growTimer < growDuration)
        {
            growTimer += Time.deltaTime;
            float currentScale = Mathf.Lerp(0f, targetScale, growTimer / growDuration);
            if (backBlackhole != null) backBlackhole.transform.localScale = new Vector3(currentScale, currentScale, 1f);
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        if (currentHealth > 0)
        {
            Vector2 dir = (playerTransform.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Instantiate(blackholeProjectilePrefab, firePoint.position, rot);
        }

        isPullingPlayer = false;
        if (backBlackhole != null) backBlackhole.SetActive(false);
        if (bossAudio != null) bossAudio.Stop();

        isUninterruptible = false;
        isAttacking = false;
    }

    // ==========================================
    // UI และการเปลี่ยนเฟส
    // ==========================================
    void ShowDamagePopup(int damageAmount, Color textColor, float fontSize)
    {
        if (damagePopupPrefab != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-1f, 1f), 1f, 0);
            GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
            popup.GetComponent<DamagePopup>().SetupCustom(damageAmount, textColor, fontSize);
        }
    }

    void StartPhase2()
    {
        isPhase2 = true;
        if (bossAnimator != null)
        {
            bossAnimator.SetTrigger("EnterPhase2");
            bossAnimator.SetBool("IsPhase2", true);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && hasSpawned && !isDead)
        {
            collision.gameObject.GetComponent<PlayerController>()?.Knockback(transform);
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(1);
        }
    }

    // ==========================================
    // 💀 ท่าตายของบอส
    // ==========================================
    IEnumerator BossDeathRoutine()
    {
        isDead = true;

        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);
        if (victorySound != null) AudioSource.PlayClipAtPoint(victorySound, transform.position, 1.2f);

        GetComponent<Collider2D>().enabled = false;

        if (backBlackhole != null) backBlackhole.SetActive(false);
        if (laserMuzzleFlash != null) laserMuzzleFlash.SetActive(false);
        if (laserBeamLine != null) laserBeamLine.SetActive(false);

        if (bossAnim != null) bossAnim.SetTrigger("Die");

        float dieDuration = 2.5f;
        float t = 0;
        while (t < dieDuration)
        {
            t += Time.deltaTime;
            if (bgmSource != null) bgmSource.volume = Mathf.Lerp(defaultVolume, 0f, t / dieDuration);
            yield return null;
        }

        if (bgmSource != null && originalBGM != null)
        {
            bgmSource.clip = originalBGM;
            bgmSource.volume = defaultVolume;
            bgmSource.Play();
        }

        Destroy(gameObject);
    }
    // ==========================================
    // 🚨 ระบบเตือนก่อนโจมตี (Telegraphing)
    // ==========================================
    [Header("Warning System")]
    public GameObject exclamationPrefab; // 🌟 ช่องสำหรับใส่ Prefab รูป !

    IEnumerator ShowWarningRoutine(float warningDuration)
    {
        GameObject alert = null;

        // 1. เสกเครื่องหมายตกใจไว้บนหัวบอส
        if (exclamationPrefab != null)
        {
            alert = Instantiate(exclamationPrefab, transform.position + new Vector3(0, iconOffsetY + 1f, 0), Quaternion.identity, transform);
        }

        // 2. ทำให้ตัวบอสกระพริบสีแดง-ขาวรัวๆ
        float timer = 0;
        bool isRed = false;
        while (timer < warningDuration)
        {
            if (isDead) break;
            if (spriteRenderer != null) spriteRenderer.color = isRed ? Color.white : Color.red;
            isRed = !isRed;

            yield return new WaitForSeconds(0.1f); // สลับสีทุกๆ 0.1 วินาที
            timer += 0.1f;
        }

        // 3. จบการเตือน ลบเครื่องหมายตกใจและคืนสีเดิม
        if (spriteRenderer != null && !isDead) spriteRenderer.color = isPhase2 ? Color.cyan : Color.white;
        if (alert != null) Destroy(alert);
    }
}