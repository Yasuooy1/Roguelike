using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem; // 🌟 1. เพิ่มบรรทัดนี้เพื่อให้รู้จัก InputValue

public class PlayerCombat : MonoBehaviour
{
    [Header("จุดปล่อยพลัง")]
    public Transform firePoint;

    [Header("กระสุนแยกตามธาตุ")]
    public GameObject fireBulletPrefab;
    public GameObject waterBulletPrefab;
    public GameObject lightningBulletPrefab;

    [Header("ท่าไม้ตาย")]
    public GameObject ultimatePrefab;
    public int ultimateManaCost = 2;

    [Header("ระบบคูลดาวน์")]
    public float fireCooldown = 0.5f;
    private float nextFireTime = 0f;

    public float bulletSizeMultiplier = 1f;
    public int ultimateManaDiscount = 0;

    private PlayerMana playerMana;
    private SpellMixer spellMixer;

    [Header("ระบบเสียงยิงตามธาตุ")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip lightningSound;
    public AudioClip waterSound;

    void Start()
    {
        Time.timeScale = 1f;
        playerMana = GetComponent<PlayerMana>();
        spellMixer = GetComponent<SpellMixer>();
        nextFireTime = Time.time + 0.5f;
        if (spellMixer != null)
        {
            spellMixer.ClearOrbs();
        }
    }

    void Update()
    {
        // 🌟 2. ลบ Input.GetKeyDown(KeyCode.U) ทิ้งไปเลย เพราะเราจะใช้ OnSkillAttack แทน

        // เช็คว่าผสมครบ 3 ลูกและคูลดาวน์เสร็จหรือยัง (การตรวจสอบยิงกระสุนยังคงเหมือนเดิม)
        if (spellMixer != null && spellMixer.currentOrbs.Count >= 3 && Time.time >= nextFireTime)
        {
            TryCastPuzzleSpell();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    // ==========================================
    // 🌟 รับค่าปุ่มเวทมนตร์จาก New Input System (จอยและคีย์บอร์ด)
    // ==========================================

    // ธาตุไฟ (กด J หรือ X/สี่เหลี่ยม)
    public void OnElement1(InputValue value)
    {
        if (value.isPressed && spellMixer != null)
        {
            spellMixer.AddOrb(SpellMixer.Element.Fire); // 🌟 เรียกใช้ AddOrb แบบตรงๆ
        }
    }

    // ท่าน้ำ (กด K หรือ Y/สามเหลี่ยม)
    public void OnElement2(InputValue value)
    {
        if (value.isPressed && spellMixer != null)
        {
            spellMixer.AddOrb(SpellMixer.Element.Water);
        }
    }

    // สายฟ้า (กด L หรือ B/วงกลม)
    public void OnElement3(InputValue value)
    {
        if (value.isPressed && spellMixer != null)
        {
            spellMixer.AddOrb(SpellMixer.Element.Lightning);
        }
    }

    // ยิงเวทไม้ตาย (กด U หรือ LB/R2)
    public void OnSkillAttack(InputValue value)
    {
        if (value.isPressed)
        {
            CastUltimateSkill();
        }
    }

    // ==========================================
    // 🧩 ฟังก์ชันยิงกระสุนและการค้นหาเป้าหมาย (เหมือนเดิมเป๊ะ)
    // ==========================================

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

        if (waterCount > fireCount && waterCount >= lightningCount)
        {
            prefabToShoot = waterBulletPrefab;
            soundToPlay = 2;
        }
        else if (lightningCount > fireCount && lightningCount > waterCount)
        {
            prefabToShoot = lightningBulletPrefab;
            soundToPlay = 1;
        }
        else if (fireCount == 1 && waterCount == 1 && lightningCount == 1)
        {
            prefabToShoot = fireBulletPrefab;
            soundToPlay = 0;
        }

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
            case 0:
                if (fireSound != null) audioSource.PlayOneShot(fireSound);
                break;
            case 1:
                if (lightningSound != null) audioSource.PlayOneShot(lightningSound);
                break;
            case 2:
                if (waterSound != null) audioSource.PlayOneShot(waterSound);
                break;
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
        if (playerMana != null && playerMana.UseMana(finalCost) && ultimatePrefab != null)
        {
            Instantiate(ultimatePrefab, firePoint.position, GetAutoAimRotation());
        }
    }
}