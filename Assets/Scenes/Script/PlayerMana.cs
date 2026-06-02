using UnityEngine;
using UnityEngine.UI;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Stats")]
    public int baseMaxMana = 3; // มานาเริ่มต้น
    public int maxMana;
    public int currentMana;

    [Header("UI System")]
    public Image[] manaImages; // อาเรย์เก็บรูปดวงมานา
    public Sprite fullMana;    // รูปมานาเต็มดวง (สีฟ้า)

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

    // ระบบวาด UI มานา (ใช้ตรรกะเปลี่ยนสีดำแบบเดียวกับเลือดเป๊ะๆ)
    void UpdateManaUI()
    {
        for (int i = 0; i < manaImages.Length; i++)
        {
            // ใส่รูปมานา
            if (fullMana != null)
            {
                manaImages[i].sprite = fullMana;
            }

            // ถ้ามานายังมีอยู่ ให้เป็นสีสว่าง (Color.white)
            if (i < currentMana)
            {
                manaImages[i].color = Color.white;
            }
            // ถ้ามานาหมดแล้วดวงนั้น ให้เปลี่ยนเป็นสีดำโปร่งแสง
            else
            {
                manaImages[i].color = new Color(0, 0, 0, 0.5f);
            }

            // เปิดปิดจำนวนดวงให้ตรงกับ Max Mana
            if (i < maxMana) manaImages[i].enabled = true;
            else manaImages[i].enabled = false;
        }
    }

    // ฟังก์ชันสำหรับโหลดสเตตัส (ถูกเรียกตอนเริ่มเกม หรือหลังอัปเกรด)
    public void RefreshManaStat()
    {
        // 1. ดึงค่าเลเวลมานาที่เพิ่งอัปเกรด (⚠️ เช็กชื่อคีย์ "Upgrade_Mana" ให้ตรงกับในสคริปต์อัปเกรดของคุณอาร์มด้วยนะครับ)
        int bonusMana = PlayerPrefs.GetInt("Upgrade_Mana", 0);

        // 2. คำนวณมานาสูงสุดใหม่
        maxMana = baseMaxMana + bonusMana;

        // 3. เติมมานาให้เต็ม
        currentMana = maxMana;

        // 4. สั่งวาดรูปคริสตัลมานาบนหน้าจอใหม่ทันที!
        UpdateManaUI();

        Debug.Log("โหลดสเตตัสมานาเรียบร้อย! Max Mana: " + maxMana);
    }
}