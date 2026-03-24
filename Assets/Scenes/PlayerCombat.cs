using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PlayerCombat : MonoBehaviour
{
    [Header("จุดปล่อยพลัง")]
    public Transform firePoint;

    [Header("กระสุนผสมธาตุ (ยิงออโต้เมื่อครบ 3 ลูก)")]
    public GameObject bulletPrefab;

    [Header("ท่าไม้ตาย (ปุ่ม K / เสียมานา)")]
    public GameObject ultimatePrefab;
    public int ultimateManaCost = 2;

    [Header("ระบบคูลดาวน์ (ความถี่ในการยิง)")]
    public float fireCooldown = 0.5f;
    private float nextFireTime = 0f;

    public float bulletSizeMultiplier = 1f;
    public int ultimateManaDiscount = 0;

    private PlayerMana playerMana;
    private SpellMixer spellMixer;

    void Start()
    {
        playerMana = GetComponent<PlayerMana>();
        spellMixer = GetComponent<SpellMixer>();
    }

    void Update()
    {
        // กันไม่ให้ยิงทะลุ UI 
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 🌟 1. ระบบยิงออโต้: ครบ 3 ลูก + คูลดาวน์เสร็จ = ยิงล็อกเป้าทันที!
        if (spellMixer != null && spellMixer.currentOrbs.Count >= 3 && Time.time >= nextFireTime)
        {
            TryCastPuzzleSpell();
            nextFireTime = Time.time + fireCooldown; // เริ่มนับคูลดาวน์ใหม่
        }

        // 💥 2. ท่าไม้ตาย (กดปุ่ม K เพื่อใช้ท่าใหญ่)
        if (Input.GetKeyDown(KeyCode.U))
        {
            CastUltimateSkill();
        }
    }

    // ==========================================
    // 🎯 ระบบ Auto-Aim (ค้นหาศัตรูที่ใกล้ที่สุด)
    // ==========================================
    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");

        List<GameObject> allTargets = new List<GameObject>();
        allTargets.AddRange(enemies);
        allTargets.AddRange(bosses);

        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject potentialTarget in allTargets)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
            }
        }
        return bestTarget;
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
        else
        {
            float defaultAngle = (transform.eulerAngles.y == 0) ? 0f : 180f;
            return Quaternion.Euler(0, 0, defaultAngle);
        }
    }

    // ==========================================
    // 🧩 ยิงกระสุนผสมธาตุ
    // ==========================================
    void TryCastPuzzleSpell()
    {
        List<string> sortedOrbs = new List<string>();
        foreach (var orb in spellMixer.currentOrbs)
        {
            sortedOrbs.Add(orb.ToString());
        }
        string recipe = sortedOrbs[0] + sortedOrbs[1] + sortedOrbs[2];

        if (bulletPrefab != null)
        {
            GameObject puzzleBullet = Instantiate(bulletPrefab, firePoint.position, GetAutoAimRotation());
            puzzleBullet.transform.localScale *= bulletSizeMultiplier;

            int damageUpgradeLevel = PlayerPrefs.GetInt("Upgrade_Damage", 0);
            float finalDamage = 10f + (damageUpgradeLevel * 2f);

            Bullet bulletScript = puzzleBullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.puzzleRecipe = recipe;
                bulletScript.isPuzzleBullet = true;
                bulletScript.damage = finalDamage;

                SpriteRenderer sr = puzzleBullet.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.cyan; // สีตั้งต้นของกระสุนผสม
            }
        }

        spellMixer.ClearOrbs(); // ยิงเสร็จเคลียร์ลูกแก้วบนหัวทิ้ง
    }

    // ==========================================
    // 💥 ยิงท่าไม้ตาย
    // ==========================================
    void CastUltimateSkill()
    {
        int finalCost = Mathf.Max(0, ultimateManaCost - ultimateManaDiscount);

        if (playerMana != null && !playerMana.UseMana(finalCost))
        {
            Debug.Log("❌ มานาไม่พอใช้ท่าไม้ตาย!");
            return;
        }

        if (ultimatePrefab != null)
        {
            Instantiate(ultimatePrefab, firePoint.position, GetAutoAimRotation());
            Debug.Log("🔥 ใช้ท่าไม้ตายใหญ่!!");
        }
    }
}