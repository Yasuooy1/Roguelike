using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI ตอนเล่นเกม (Gameplay)")]
    public TextMeshProUGUI currentWaveText;

    [Header("UI หน้า Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultWaveText; // โชว์เวฟที่จบในรอบนี้
    public TextMeshProUGUI bestWaveText;   // โชว์สถิติเวฟสูงสุด (High Score)
    public GameObject newRecordAlert;      // ป้ายแจ้งเตือน "New Record!" 

    private int currentWave = 1;

    void Awake()
    {
        instance = this;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (newRecordAlert != null) newRecordAlert.SetActive(false);
        UpdateGameplayUI();
    }

    // 🌟 เรียกใช้ฟังก์ชันนี้จาก WaveManager ตอนเริ่มเวฟใหม่
    public void UpdateWave(int waveNumber)
    {
        currentWave = waveNumber;
        UpdateGameplayUI();
    }

    private void UpdateGameplayUI()
    {
        if (currentWaveText != null)
            currentWaveText.text = "Wave: " + currentWave;
    }

    // 🌟 เรียกใช้ฟังก์ชันนี้จาก PlayerHealth ตอนผู้เล่นเลือดหมด
    public void GameOver()
    {
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // ดึงสถิติเวฟสูงสุดเก่ามาเทียบ
        int bestWave = PlayerPrefs.GetInt("BestWave", 0);
        bool isNewRecord = false;

        // เช็กว่ารอบนี้ไปไกลกว่าสถิติเดิมไหม
        if (currentWave > bestWave)
        {
            bestWave = currentWave;
            PlayerPrefs.SetInt("BestWave", bestWave); // อัปเดตค่าใหม่
            PlayerPrefs.Save(); // เซฟลงเครื่อง
            isNewRecord = true;
        }

        // อัปเดตข้อความบนหน้า Game Over
        if (resultWaveText != null) resultWaveText.text = "You Reached Wave: " + currentWave;
        if (bestWaveText != null) bestWaveText.text = "Best Wave (High Score): " + bestWave;

        // เปิดป้ายแจ้งเตือนถ้าทำลายสถิติ
        if (newRecordAlert != null) newRecordAlert.SetActive(isNewRecord);
    }
}