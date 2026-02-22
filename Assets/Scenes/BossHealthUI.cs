using UnityEngine;
using UnityEngine.UI; // ต้องใส่ตัวนี้ถึงจะคุม UI ได้

public class BossHealthUI : MonoBehaviour
{
    // 🌟 สร้างระบบ Singleton ให้บอสจากที่ไหนก็เรียกใช้ UI นี้ได้ง่ายๆ
    public static BossHealthUI instance;

    [Header("UI Elements")]
    public GameObject uiPanel;    // ตัวกรอบพื้นหลัง (BossHealthBackground)
    public Image healthFill;      // หลอดเลือดสีแดง (BossHealthFill)

    private int maxBossHealth;

    void Awake()
    {
        // เซ็ตอัพระบบ Singleton
        if (instance == null) instance = this;

        // เริ่มเกมมาให้ซ่อนหลอดเลือดไว้ก่อน (รอจนกว่าบอสจะเกิด)
        uiPanel.SetActive(false);
    }

    // ฟังก์ชันให้บอสเรียกตอนเปิดตัว
    public void ShowBossUI(int maxHP)
    {
        maxBossHealth = maxHP;
        healthFill.fillAmount = 1f; // เลือดเต็มหลอด
        uiPanel.SetActive(true);    // โชว์ UI!
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