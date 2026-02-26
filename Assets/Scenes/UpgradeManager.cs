using UnityEngine;
using TMPro; // สำหรับแสดงจำนวนแต้มที่มี
using UnityEngine.SceneManagement;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI Display")]
    public TextMeshProUGUI soulPointsText; // แสดงแต้มที่ใช้ซื้อ (เช่น วิญญาณ หรือ เงิน)

    // คีย์สำหรับเซฟข้อมูล
    private const string HEALTH_KEY = "Upgrade_Health";
    private const string MANA_KEY = "Upgrade_Mana";
    private const string DAMAGE_KEY = "Upgrade_Damage";
    private const string SOUL_KEY = "Player_Souls";

    void Start()
    {
        UpdateUI();
    }

    // --- 🟢 ฟังก์ชันสำหรับปุ่มกดอัปเกรด 🟢 ---

    public void UpgradeMaxHealth()
    {
        int souls = PlayerPrefs.GetInt(SOUL_KEY, 0);
        if (souls >= 10) // สมมติว่าค่าอัปเกรดคือ 10 แต้ม
        {
            PlayerPrefs.SetInt(SOUL_KEY, souls - 10);
            int currentLevel = PlayerPrefs.GetInt(HEALTH_KEY, 0);
            PlayerPrefs.SetInt(HEALTH_KEY, currentLevel + 1);
            UpdateUI();
            Debug.Log("อัปเลือดถาวรสำเร็จ!");
            FindObjectOfType<PlayerHealth>()?.RefreshHealthStat();
        }
    }

    public void UpgradeMaxMana()
    {
        int souls = PlayerPrefs.GetInt(SOUL_KEY, 0);
        if (souls >= 10)
        {
            PlayerPrefs.SetInt(SOUL_KEY, souls - 10);
            int currentLevel = PlayerPrefs.GetInt(MANA_KEY, 0);
            PlayerPrefs.SetInt(MANA_KEY, currentLevel + 1);
            UpdateUI();
            FindObjectOfType<PlayerMana>()?.RefreshManaStat();
        }
    }

    public void UpgradeDamage()
    {
        int souls = PlayerPrefs.GetInt(SOUL_KEY, 0);
        if (souls >= 15) // ดาเมจอาจจะแพงหน่อย
        {
            PlayerPrefs.SetInt(SOUL_KEY, souls - 15);
            int currentLevel = PlayerPrefs.GetInt(DAMAGE_KEY, 0);
            PlayerPrefs.SetInt(DAMAGE_KEY, currentLevel + 1);
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (soulPointsText != null)
            soulPointsText.text = "Souls: " + PlayerPrefs.GetInt(SOUL_KEY, 0);
    }

    // 💡 ฟังก์ชันโกงเงิน (เอาไว้เทสต์ระบบ)
    [ContextMenu("Add 100 Souls")]
    public void AddSoulsDebug()
    {
        PlayerPrefs.SetInt(SOUL_KEY, PlayerPrefs.GetInt(SOUL_KEY, 0) + 100);
        UpdateUI();
    }
    // ฟังก์ชันนี้เอาไว้ลากไปใส่ที่ปุ่ม (Button) บนหน้าจอ UI
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll(); // ลบเซฟทั้งหมด
        UpdateUI();              // รีเฟรชตัวเลขบนจอ

        // รีเซ็ตหลอดบนหัวตัวละคร
        FindObjectOfType<PlayerHealth>()?.RefreshHealthStat();
        FindObjectOfType<PlayerMana>()?.RefreshManaStat();

        Debug.Log("ผู้เล่นกดรีเซ็ตข้อมูลทั้งหมด!");
    }
    // ฟังก์ชันสำหรับปุ่ม "กลับเมนูหลัก"
    public void BackToMainMenu()
    {
        // สั่งโหลดหน้าเมนู (จากรูปของคุณอาร์ม ไฟล์ชื่อ "Menu" เป๊ะๆ)
        SceneManager.LoadScene("Menu");
    }
}