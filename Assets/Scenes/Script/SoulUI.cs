using UnityEngine;
using TMPro;

public class SoulUI : MonoBehaviour
{
    public TextMeshProUGUI soulTextDisplay;

    void Update()
    {
        // 🌟 แก้ชื่อคีย์ให้ดึงจาก Player_Souls เหมือนกัน
        int currentSouls = PlayerPrefs.GetInt("Player_Souls", 0);

        if (soulTextDisplay != null)
        {
            soulTextDisplay.text = currentSouls.ToString();
        }
    }
}