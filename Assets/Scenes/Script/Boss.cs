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

    [Header("⚠️ ระบบเตือนภัยก่อนบอสโจมตี (Warning)")]
    public GameObject exclamationPrefab;
    public float exclamationOffsetY = 3.5f; // 🌟 เพิ่มตัวแปรให้ไปปรับความสูง ! ใน Unity ได้เลย
    public GameObject dangerZonePrefab;

    [Header("Animation & Blackhole (เฟส 2)")]
    public Animator bossAnim;
    public GameObject backBlackhole;
    public GameObject blackholeProjectilePrefab;
    public Transform firePoint;

    private Transform playerTransform;
    private bool isAttacking = false;
    private bool isPhase2 = false;
    private bool isPullingPlayer = false;
    private bool isUninterruptible = false;

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
                originalBGM = bgmSource.clip;
                defaultVolume = bgmSource.volume;
            }

            currentCenterPos = transform.position;
            spriteRenderer.enabled = false;
            GetComponent<Collider2D>().enabled = false;
            if (puzzleCanvas != null) puzzleCanvas.SetActive(false);
            if (backBlackhole != null) backBlackhole.SetActive(false);

            StartCoroutine(BossEntranceRoutine());
        }
        bossAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (hasSpawned && !isBroken && !isAttacking)
        {
            if (playerTransform != null)
            {
                Vector3 targetPos = playerTransform.position + new Vector3(0, hoverHeight, 0);
                currentCenterPos = Vector3.SmoothDamp(currentCenterPos, targetPos, ref velocity, smoothTime);
            }
            transform.position = currentCenterPos + new Vector3(0, Mathf.Sin(Time.time * floatFrequency) * floatAmplitude, 0);
            FacePlayer();
        }

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

    void ShowWarningExclamation()
    {
        if (exclamationPrefab != null)
        {
            // 🌟 ใช้ตัวแปร exclamationOffsetY เพื่อดันให้พ้นป้ายพัซเซิล
            GameObject warningIcon = Instantiate(exclamationPrefab, transform.position + Vector3.up * exclamationOffsetY, Quaternion.identity, transform);
            Destroy(warningIcon, 0.8f);
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

    IEnumerator BulletHellRoutine()
    {
        if (bossBulletPrefab == null || playerTransform == null) yield break;
        isAttacking = true;
        FacePlayer();

        // 🛑 ลบเตือนภัยออกจากท่าสาดกระสุน
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

        // 🛑 ลบเตือนภัยออกจากท่าลูกซอง
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

        ShowWarningExclamation(); // ⚠️ เก็บเตือนภัยไว้เฉพาะท่าหนักๆ
        spriteRenderer.color = Color.red;

        if (dangerZonePrefab != null)
        {
            Vector3 lockTargetPos = playerTransform.position;
            GameObject danger = Instantiate(dangerZonePrefab, lockTargetPos, Quaternion.identity);
            Destroy(danger, 0.8f);
        }

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

        ShowWarningExclamation();
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
        isLaserUninterruptible = false;
        isAttacking = false;
    }

    IEnumerator GroundSlamShockwaveRoutine()
    {
        if (playerTransform == null) yield break;
        isAttacking = true;

        Vector3 highPos = new Vector3(playerTransform.position.x, playerTransform.position.y + 6f, 0);
        float t = 0;
        while (t < 1f)
        {
            transform.position = Vector3.Lerp(transform.position, highPos, t);
            t += Time.deltaTime * 3f;
            yield return null;
        }

        ShowWarningExclamation();
        spriteRenderer.color = Color.red;

        Vector3 slamTarget = new Vector3(transform.position.x, playerTransform.position.y, 0);
        if (dangerZonePrefab != null)
        {
            GameObject danger = Instantiate(dangerZonePrefab, slamTarget, Quaternion.identity);
            Destroy(danger, 0.7f);
        }

        yield return new WaitForSeconds(0.6f);

        Vector3 slamPos = new Vector3(transform.position.x, playerTransform.position.y, 0);
        float slamT = 0;
        while (slamT < 1f)
        {
            transform.position = Vector3.Lerp(highPos, slamPos, slamT);
            slamT += Time.deltaTime * 10f;
            yield return null;
        }
        transform.position = slamPos;

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

        yield return new WaitForSeconds(1f);
        if (!isBroken) spriteRenderer.color = Color.cyan;

        currentCenterPos = transform.position;
        isAttacking = false;
    }

    IEnumerator BlackHoleRoutine()
    {
        if (blackholeProjectilePrefab == null) yield break;
        isAttacking = true;
        isUninterruptible = true;
        FacePlayer();

        ShowWarningExclamation();

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

    public bool CheckPuzzleBullet(string playerRecipe, int damage)
    {
        if (!hasSpawned || isBroken) return false;

        if (isUninterruptible)
        {
            ShowDamagePopup(0, Color.gray, 3f);
            return false;
        }

        if (playerRecipe == requiredRecipe)
        {
            ShowDamagePopup(damage, Color.yellow, 5f);
            BreakArmor();
            TakeDamage(damage);
            return true;
        }
        else
        {
            ShowDamagePopup(0, Color.red, 3f);
            return false;
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
            if (bossAudio != null && hurtSound != null) bossAudio.PlayOneShot(hurtSound);

            currentHealth -= damage;
            ShowDamagePopup(damage, Color.white, 4f);
            if (BossHealthUI.instance != null) BossHealthUI.instance.UpdateHealth(currentHealth);

            if (currentHealth <= maxHealth / 2 && !isPhase2) StartPhase2();

            if (currentHealth <= 0)
            {
                if (BossHealthUI.instance != null) BossHealthUI.instance.HideBossUI();
                StopAllCoroutines();
                StartCoroutine(BossDeathRoutine());
            }
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
        if (collision.gameObject.CompareTag("Player") && hasSpawned)
        {
            collision.gameObject.GetComponent<PlayerController>()?.Knockback(transform);
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(1);
        }
    }

    IEnumerator BossDeathRoutine()
    {
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
   
}