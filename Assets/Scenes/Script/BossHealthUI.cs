using UnityEngine;
using UnityEngine.UI; // ต้องใส่ตัวนี้ถึงจะคุม UI ได้
// using TMPro; // 🌟 ถ้าใช้ฟอนต์แบบ TextMeshPro ให้เอา // ข้างหน้าออกนะครับ

public class BossHealthUI : MonoBehaviour
{
    // 🌟 สร้างระบบ Singleton ให้บอสจากที่ไหนก็เรียกใช้ UI นี้ได้ง่ายๆ
    public static BossHealthUI instance;

    [Header("UI Elements")]
    public GameObject uiPanel;    // ตัวกรอบพื้นหลัง (BossHealthBackground)
    public Image healthFill;      // หลอดเลือดสีแดง (BossHealthFill)

    [Header("UI ชื่อบอส")]
    public Text bossNameText;     // 🌟 1. เพิ่มตัวช่องใส่ข้อความชื่อบอส (ถ้าใช้ TextMeshPro ให้เปลี่ยนคำว่า Text เป็น TextMeshProUGUI)

    private int maxBossHealth;

    void Awake()
    {
        // เซ็ตอัพระบบ Singleton
        if (instance == null) instance = this;

        // เริ่มเกมมาให้ซ่อนหลอดเลือดไว้ก่อน (รอจนกว่าบอสจะเกิด)
        uiPanel.SetActive(false);
    }

    // 🌟 2. แก้ฟังก์ชันให้รับค่า string bossName เพิ่มเข้ามา
    public void ShowBossUI(int maxHP, string bossName)
    {
        maxBossHealth = maxHP;
        healthFill.fillAmount = 1f; // เลือดเต็มหลอด
        uiPanel.SetActive(true);    // โชว์ UI!

        // 🌟 3. สั่งเปลี่ยนข้อความบนหลอดเลือด ให้ตรงกับชื่อบอสที่ส่งมา
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
    }

    // ฟังก์ชันให้บอสเรียกตอนโดนตี
    public void UpdateHealth(int currentHP)
    {
        // คำนวณเปอร์เซ็นต์เลือด (0.0 ถึง 1.0)
        healthFill.fillAmount = (float)currentHP / maxBossHealth;
    }

    // ฟังก์ชันให้บอสเรียกตอนตาย
    public void HideBossUI()
    {
        uiPanel.SetActive(false); // ซ่อน UI
    }
}