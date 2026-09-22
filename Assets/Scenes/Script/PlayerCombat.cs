using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("จุดปล่อยพลัง")]
    public Transform firePoint;

    [Header("🔮 เวทมนตร์ธาตุเดี่ยว (ใส่ธาตุเดียวกัน 3 ลูก)")]
    public GameObject fireBulletPrefab;
    public GameObject waterBulletPrefab;
    public GameObject WindBulletPrefab;

    [Header("✨ เวทมนตร์แบบผสม (Invoker Style!)")]
    public GameObject plasmaBulletPrefab;

    [Header("ท่าไม้ตาย")]
    public GameObject ultimatePrefab;
    public int ultimateManaCost = 2;

    [Header("ระบบคูลดาวน์และอื่นๆ")]
    public float fireCooldown = 0.5f;
    private float nextFireTime = 0f;
    public float bulletSizeMultiplier = 1f;
    public int ultimateManaDiscount = 0;

    private PlayerMana playerMana;
    private SpellMixer spellMixer;

    [Header("ระบบเสียงยิงตามธาตุ")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip WindSound;
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
        // 🌟 ปล่อยว่างไว้เลยครับ เราไม่ยิงออโต้แล้ว![cite: 2]
    }

    // ==========================================
    // 🌟 รับค่าปุ่มเวทมนตร์จาก New Input System 
    // ==========================================
    public void OnElement1(InputValue value)
    {
        if (value.isPressed && spellMixer != null) spellMixer.AddOrb(SpellMixer.Element.Fire);
    }

    public void OnElement2(InputValue value)
    {
        if (value.isPressed && spellMixer != null) spellMixer.AddOrb(SpellMixer.Element.Water);
    }

    public void OnElement3(InputValue value)
    {
        if (value.isPressed && spellMixer != null) spellMixer.AddOrb(SpellMixer.Element.Wind);
    }

    public void OnSkillAttack(InputValue value)
    {
        if (value.isPressed) CastUltimateSkill();
    }

    // ==========================================
    // 💥 รับค่าปุ่มยิงเวทมนตร์ (ปุ่ม Fire ใหม่) 
    // ==========================================
    public void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            // เช็กว่าผสมครบ 3 ลูกและคูลดาวน์เสร็จหรือยัง
            if (spellMixer != null && spellMixer.currentOrbs.Count >= 3 && Time.time >= nextFireTime)
            {
                // สั่งยิงและรับค่าคูลดาวน์กลับมา
                float currentCooldown = TryCastPuzzleSpell();
                nextFireTime = Time.time + currentCooldown;
            }
            else if (Time.time < nextFireTime)
            {
                Debug.Log("ปืน Overheat อยู่ รอก่อน!");
            }
            else if (spellMixer != null && spellMixer.currentOrbs.Count < 3)
            {
                Debug.Log("ยังผสมไม่ครบ 3 ลูก ยิงไม่ได้นะ!");
            }
        }
    }

    // ==========================================
    // 🧙‍♂️ ระบบยิงเวทมนตร์สไตล์ Invoker
    // ==========================================

    // 🌟 เปลี่ยนจาก void เป็น float เพื่อคืนค่าคูลดาวน์[cite: 2]
    float TryCastPuzzleSpell()
    {
        List<string> sortedOrbs = new List<string>();

        foreach (var orb in spellMixer.currentOrbs)
        {
            sortedOrbs.Add(orb.ToString());
        }

        sortedOrbs.Sort();
        string recipe = sortedOrbs[0] + sortedOrbs[1] + sortedOrbs[2];

        GameObject prefabToShoot = fireBulletPrefab;
        int soundToPlay = 0;

        // 🌟 ดึงค่าคูลดาวน์ปกติมาตั้งเป็นค่าเริ่มต้น
        float actualCooldown = fireCooldown;

        switch (recipe)
        {
            case "FireFireFire":
                prefabToShoot = fireBulletPrefab;
                soundToPlay = 0;
                break;
            case "WaterWaterWater":
                prefabToShoot = waterBulletPrefab;
                soundToPlay = 2;
                break;
            case "WindWindWind":
                prefabToShoot = WindBulletPrefab;
                soundToPlay = 1;
                break;

            case "FireFireWind":
                prefabToShoot = plasmaBulletPrefab != null ? plasmaBulletPrefab : fireBulletPrefab;
                soundToPlay = 1;
                actualCooldown = 2.5f; // 🛑 ปืนโอเวอร์ฮีต! ยิงพลาสมาเสร็จต้องพัก 2.5 วิ[cite: 2]
                break;

            default:
                prefabToShoot = fireBulletPrefab;
                break;
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

        // 🌟 ส่งค่าคูลดาวน์กลับไปให้ปุ่มยิงทำงาน
        return actualCooldown;
    }

    // ==========================================
    // 🎯 ระบบค้นหาเป้าหมาย (เหมือนเดิมเป๊ะ)
    // ==========================================
    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        ElementalObstacle[] obstacles = FindObjectsOfType<ElementalObstacle>();

        List<GameObject> allTargets = new List<GameObject>();
        allTargets.AddRange(enemies);
        allTargets.AddRange(bosses);

        foreach (ElementalObstacle obs in obstacles)
        {
            if (obs.gameObject.activeInHierarchy)
            {
                allTargets.Add(obs.gameObject);
            }
        }

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
                if (WindSound != null) audioSource.PlayOneShot(WindSound);
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