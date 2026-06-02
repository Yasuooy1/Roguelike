using UnityEngine;
using UnityEngine.UI; // อย่าลืม using UI นะครับ

public class BossFightUI : MonoBehaviour
{
    public static BossFightUI instance;

    [Header("ข้อความแจ้งเตือนกลางจอ")]
    public Text warningText;

    [Header("หลอดชาร์จพลังงาน (เฟส 2)")]
    public GameObject energyPanel; // กรอบใส่ลูกแก้วพลังงาน
    public Image[] energySlots;
    public Sprite emptyEnergySprite;
    public Sprite fullEnergySprite;

    void Awake()
    {
        instance = this;

        // เริ่มเกมมา ซ่อนข้อความและหลอดพลังงานไว้ก่อน
        if (warningText != null) warningText.text = "";
        if (energyPanel != null) energyPanel.SetActive(false);
    }

    // ฟังก์ชันนี้แหละครับที่ PlayerCombat ร้องหา!
    public void UpdatePhase2Energy(int current, int max)
    {
        // พอเริ่มเก็บพลังงาน ให้โชว์ UI ขึ้นมา
        if (energyPanel != null) energyPanel.SetActive(true);

        // อัปเดตรูปลูกแก้ว
        for (int i = 0; i < energySlots.Length; i++)
        {
            if (i < current) energySlots[i].sprite = fullEnergySprite;
            else energySlots[i].sprite = emptyEnergySprite;
        }

        // เปลี่ยนข้อความแจ้งเตือน
        if (warningText != null)
        {
            if (current >= max)
            {
                warningText.text = "⚡ พลังงานเต็ม! ชี้เมาส์ไปที่บอสแล้ว [คลิกซ้าย] ยิงเลย! ⚡";
                warningText.color = Color.yellow;
            }
            else
            {
                warningText.text = "เก็บก้อนพลังงานที่พื้นเพื่อชาร์จท่าไม้ตาย! (" + current + "/" + max + ")";
                warningText.color = Color.cyan;
            }
        }
    }

    // ฟังก์ชันแถม: เอาไว้สั่งโชว์ข้อความเตือนอื่นๆ
    public void ShowWarning(string message, Color color)
    {
        if (warningText != null)
        {
            warningText.text = message;
            warningText.color = color;
        }
    }

    public void HideWarning()
    {
        if (warningText != null) warningText.text = "";
    }
}