using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [Header("จุดปล่อยพลัง")]
    public Transform firePoint;
    [Header("✨ ระบบแอนิเมชันตัวละคร")]
    public Animator playerAnimator;

    [Header("กระสุนแยกตามธาตุ")]
    public GameObject fireBulletPrefab;
    public GameObject waterBulletPrefab;
    public GameObject lightningBulletPrefab;

    [Header("ท่าไม้ตาย (สโลว์เวลา)")]
    public GameObject ultimatePrefab;
    public int ultimateManaCost = 2;
    public GameObject bulletTimeVFX;

    [Header("ระบบคูลดาวน์")]
    public float fireCooldown = 0.5f;
    private float nextFireTime = 0f;

    public float bulletSizeMultiplier = 1f;
    public int ultimateManaDiscount = 0;

    private PlayerMana playerMana;
    private SpellMixer spellMixer;

    [Header("ระบบเสียง")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip lightningSound;
    public AudioClip waterSound;
    public AudioClip timeStopSound;

    void Start()
    {
        Time.timeScale = 1f;
        playerMana = GetComponent<PlayerMana>();
        spellMixer = GetComponent<SpellMixer>();
        nextFireTime = Time.time + 0.5f;
        if (spellMixer != null) spellMixer.ClearOrbs();

        if (bulletTimeVFX != null) bulletTimeVFX.SetActive(false);
    }

    void Update() { }

    public void OnElement1(InputValue value) { if (value.isPressed && spellMixer != null) spellMixer.AddOrb(SpellMixer.Element.Fire); }
    public void OnElement2(InputValue value) { if (value.isPressed && spellMixer != null) spellMixer.AddOrb(SpellMixer.Element.Water); }
    public void OnElement3(InputValue value) { if (value.isPressed && spellMixer != null) spellMixer.AddOrb(SpellMixer.Element.Lightning); }

    public void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            // 🛑 ถ้าธาตุยังไม่ครบ 3 ลูก บล็อกไม่ให้โจมตีและไม่สะบัดไม้คฑา
            if (spellMixer != null && spellMixer.currentOrbs.Count < 3)
            {
                if (playerAnimator != null) playerAnimator.SetBool("CanAttack", false);
                Debug.Log("ยังผสมธาตุไม่ครบ 3 ลูก ยิงไม่ได้!");
                return;
            }

            // ✨ ถ้าธาตุครบ 3 ลูกแล้ว อนุญาตให้โจมตีได้
            if (spellMixer != null && spellMixer.currentOrbs.Count >= 3 && Time.time >= nextFireTime)
            {
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("CanAttack", true);
                    playerAnimator.SetTrigger("Attack"); // สั่งเล่นท่าโจมตี
                }
                PlayerController controller = GetComponent<PlayerController>();
                if (controller != null) controller.LockMovement(0.5f);

                TryCastPuzzleSpell();
                nextFireTime = Time.time + fireCooldown;
            }
        }
    }

    public void OnSkillAttack(InputValue value)
    {
        if (value.isPressed) CastUltimateSkill();
    }

    void TryCastPuzzleSpell()
    {
        List<string> sortedOrbs = new List<string>();
        int fireCount = 0, waterCount = 0, lightningCount = 0;

        foreach (var orb in spellMixer.currentOrbs)
        {
            string element = orb.ToString();
            sortedOrbs.Add(element);
            if (element == "Fire") fireCount++;
            else if (element == "Water") waterCount++;
            else if (element == "Lightning") lightningCount++;
        }

        string recipe = sortedOrbs[0] + sortedOrbs[1] + sortedOrbs[2];
        GameObject prefabToShoot = fireBulletPrefab;
        int soundToPlay = 0;

        if (waterCount > fireCount && waterCount >= lightningCount) { prefabToShoot = waterBulletPrefab; soundToPlay = 2; }
        else if (lightningCount > fireCount && lightningCount > waterCount) { prefabToShoot = lightningBulletPrefab; soundToPlay = 1; }
        else if (fireCount == 1 && waterCount == 1 && lightningCount == 1) { prefabToShoot = fireBulletPrefab; soundToPlay = 0; }

        if (prefabToShoot != null)
        {
            GameObject puzzleBullet = Instantiate(prefabToShoot, firePoint.position, GetAutoAimRotation());
            PlayElementShootSound(soundToPlay);
            puzzleBullet.transform.localScale *= bulletSizeMultiplier;

            Bullet bulletScript = puzzleBullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.isPuzzleBullet = true;
                bulletScript.puzzleRecipe = recipe;
                int damageUpgradeLevel = PlayerPrefs.GetInt("Upgrade_Damage", 0);

                // 🌟 แก้ไขจุดที่ตัวหนังสือติดกันให้ถูกต้องเรียบร้อยครับ
                bulletScript.damage = 10f + (damageUpgradeLevel * 2f);
            }
        }
        spellMixer.ClearOrbs();
    }

    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        List<GameObject> allTargets = new List<GameObject>();
        allTargets.AddRange(enemies); allTargets.AddRange(bosses);
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach (GameObject potentialTarget in allTargets)
        {
            float dSqrToTarget = (potentialTarget.transform.position - transform.position).sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
            }
        }
        return bestTarget;
    }

    public void PlayElementShootSound(int currentElementIndex)
    {
        if (audioSource == null) return;
        switch (currentElementIndex)
        {
            case 0: if (fireSound != null) audioSource.PlayOneShot(fireSound); break;
            case 1: if (lightningSound != null) audioSource.PlayOneShot(lightningSound); break;
            case 2: if (waterSound != null) audioSource.PlayOneShot(waterSound); break;
        }
    }

    Quaternion GetAutoAimRotation()
    {
        Transform target = FindClosestEnemy();
        if (target != null)
        {
            Vector2 lookDir = target.position - firePoint.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, 0, angle);
        }
        float defaultAngle = (transform.eulerAngles.y == 0) ? 0f : 180f;
        return Quaternion.Euler(0, 0, defaultAngle);
    }

    void CastUltimateSkill()
    {
        int finalCost = Mathf.Max(0, ultimateManaCost - ultimateManaDiscount);

        if (Time.timeScale < 1f) return;

        if (playerMana != null && playerMana.UseMana(finalCost))
        {
            StartCoroutine(SupportBulletTimeRoutine());
        }
    }

    private IEnumerator SupportBulletTimeRoutine()
    {
        if (audioSource != null && timeStopSound != null)
        {
            audioSource.PlayOneShot(timeStopSound);
        }

        Time.timeScale = 0.05f;

        Image overlayImage = null;
        float maxAlpha = 0.5f;
        Color grayColor = new Color(0.2f, 0.2f, 0.2f, maxAlpha);

        if (bulletTimeVFX != null)
        {
            bulletTimeVFX.SetActive(true);
            overlayImage = bulletTimeVFX.GetComponent<Image>();
        }

        if (overlayImage != null)
        {
            overlayImage.color = new Color(1f, 1f, 1f, 0.7f);
        }

        yield return new WaitForSecondsRealtime(0.15f);

        Time.timeScale = 0.4f;

        if (overlayImage != null)
        {
            float fadeTime = 0.2f;
            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                overlayImage.color = Color.Lerp(new Color(1f, 1f, 1f, 0.7f), grayColor, t / fadeTime);
                yield return null;
            }
            overlayImage.color = grayColor;
        }

        yield return new WaitForSecondsRealtime(2.6f);

        if (overlayImage != null)
        {
            Color c = overlayImage.color;
            float fadeTime = 0.2f;
            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                c.a = Mathf.Lerp(maxAlpha, 0f, t / fadeTime);
                overlayImage.color = c;
                yield return null;
            }
        }

        Time.timeScale = 1f;
        if (bulletTimeVFX != null) bulletTimeVFX.SetActive(false);
    }
}