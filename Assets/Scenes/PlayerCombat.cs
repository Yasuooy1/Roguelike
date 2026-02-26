using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("จุดปล่อยพลัง")]
    public Transform firePoint;

    [Header("กระสุนผสมธาตุ (คลิกซ้าย)")]
    public GameObject bulletPrefab;

    [Header("ท่าไม้ตาย (คลิกขวา / เสียมานา)")]
    public GameObject ultimatePrefab;
    public int ultimateManaCost = 2;

    // 🌟 ตัวแปรธาตุเก่าเอาไว้ให้บอส/นก ไม่ด่า
    public enum Element { Red, Green, Blue }
    [HideInInspector] public Element currentElement = Element.Red;

    private PlayerMana playerMana;
    private SpellMixer spellMixer;

    void Start()
    {
        playerMana = GetComponent<PlayerMana>();
        spellMixer = GetComponent<SpellMixer>();
    }

    void Update()
    {
        // 🧩 1. คลิกซ้าย (Fire1) = ยิงกระสุนผสมธาตุ
        if (Input.GetButtonDown("Fire1"))
        {
            TryCastPuzzleSpell();
        }

        // 💥 2. คลิกขวา (Fire2) = ใช้ท่าไม้ตายใหญ่
        if (Input.GetButtonDown("Fire2"))
        {
            CastUltimateSkill();
        }
    }

    // ==========================================
    // 🧩 คลิกซ้าย: ระบบกระสุนผสมธาตุ (ตีแตกทำดาเมจเลย)
    // ==========================================
    void TryCastPuzzleSpell()
    {
        if (spellMixer == null || spellMixer.currentOrbs.Count < 3)
        {
            Debug.Log("ลูกแก้วยังไม่ครบ 3 ลูก ยิงไม่ได้!");
            return;
        }

        // 🌟 แปลงเป็นตัวหนังสือ (String) ก่อนแล้วค่อย Sort มันจะเรียง A-Z เหมือนมอนสเตอร์
        List<string> sortedOrbs = new List<string>();
        foreach (var orb in spellMixer.currentOrbs)
        {
            sortedOrbs.Add(orb.ToString());
        }
        sortedOrbs.Sort();
        string recipe = sortedOrbs[0] + sortedOrbs[1] + sortedOrbs[2];

        if (bulletPrefab != null)
        {
            GameObject puzzleBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            int damageUpgradeLevel = PlayerPrefs.GetInt("Upgrade_Damage", 0);
            float finalDamage = 10f + (damageUpgradeLevel * 2f);

            Bullet bulletScript = puzzleBullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.puzzleRecipe = recipe;
                bulletScript.isPuzzleBullet = true;
                bulletScript.damage = finalDamage;

                SpriteRenderer sr = puzzleBullet.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.cyan;
            }
        }

        spellMixer.ClearOrbs();
    }

    // ==========================================
    // 💥 คลิกขวา: ระบบท่าไม้ตาย (เสียมานา)
    // ==========================================
    void CastUltimateSkill()
    {
        if (playerMana != null && !playerMana.UseMana(ultimateManaCost))
        {
            Debug.Log("❌ มานาไม่พอใช้ท่าไม้ตาย!");
            return;
        }

        if (ultimatePrefab != null)
        {
            Instantiate(ultimatePrefab, firePoint.position, firePoint.rotation);
            Debug.Log("🔥 ใช้ท่าไม้ตายใหญ่ (คลิกขวา)!!");
        }
        else
        {
            Debug.Log("⚠️ ยังไม่ได้ใส่ Prefab ท่าไม้ตายครับ");
        }
    }
}