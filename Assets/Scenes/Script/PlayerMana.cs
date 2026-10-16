using UnityEngine;
using UnityEngine.UI;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Stats")]
    public int baseMaxMana = 3; // มานาเริ่มต้น
    public int maxMana;
    public int currentMana;

    [Header("UI System (แบบสลับรูป)")]
    // 🌟 เปลี่ยนจาก Array เก็บรูปหลายดวง มาเป็น Image เดี่ยวสำหรับสลับรูป
    public Image manaBarImage;
    public Sprite[] manaBarSprites; // Array สำหรับใส่รูปมานา (เฟรมมานา 0 ถึง Max)

    void Start()
    {
        // 🌟 เรียกใช้ฟังก์ชันอัปเดตสเตตัสตอนเริ่มเกมทันที!
        RefreshManaStat();
    }

    // ฟังก์ชันเพิ่มมานา
    public void AddMana(int amount)
    {
        if (currentMana >= maxMana)
        {
            Debug.Log("มานาเต็มแล้วจ้า!");
            return;
        }

        currentMana += amount;

        // กันมานาทะลุหลอด
        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }

        UpdateManaUI();
        Debug.Log("ฮีลมานาแล้ว! มานาตอนนี้: " + currentMana);
    }

    // ฟังก์ชันใช้มานา
    public bool UseMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            UpdateManaUI();
            return true; // มานาพอ ยิงได้
        }
        else
        {
            Debug.Log("มานาไม่พอ!");
            return false; // มานาไม่พอ ห้ามยิง
        }
    }

    // 🌟 ระบบวาด UI มานาแบบใหม่ (สลับรูปภาพ)
    void UpdateManaUI()
    {
        if (manaBarImage != null && manaBarSprites != null && manaBarSprites.Length > 0)
        {
            // ป้องกัน Error โดยการล็อกค่าให้อยู่ในช่วง 0 ถึงจำนวนรูปที่มี
            int index = Mathf.Clamp(currentMana, 0, manaBarSprites.Length - 1);
            manaBarImage.sprite = manaBarSprites[index];
        }
    }

    // ฟังก์ชันสำหรับโหลดสเตตัส (ถูกเรียกตอนเริ่มเกม หรือหลังอัปเกรด)
    public void RefreshManaStat()
    {
        // 1. ดึงค่าเลเวลมานาที่เพิ่งอัปเกรด
        int bonusMana = PlayerPrefs.GetInt("Upgrade_Mana", 0);

        // 2. คำนวณมานาสูงสุดใหม่
        maxMana = baseMaxMana + bonusMana;

        // 3. เติมมานาให้เต็ม
        currentMana = maxMana;

        // 4. สั่งวาดรูปมานาบนหน้าจอใหม่ทันที!
        UpdateManaUI();

        Debug.Log("โหลดสเตตัสมานาเรียบร้อย! Max Mana: " + maxMana);
    }
}