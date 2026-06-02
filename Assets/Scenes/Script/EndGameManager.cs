using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    public static EndGameManager instance;

    [Header("UI Panels")]
    public GameObject winPanel;
    public GameObject gameOverPanel;

    void Awake()
    {
        instance = this;
        // เริ่มเกมมา ซ่อนหน้าจอไว้ก่อน
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // ==========================================
    // 🌟 ระบบโชว์หน้าจอ
    // ==========================================
    public void ShowWinScreen()
    {
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f; // หยุดเวลา
    }

    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // หยุดเวลา
    }

    // ==========================================
    // 🌟 ฟังก์ชันสำหรับผูกกับปุ่มกด (UI Buttons)
    // ==========================================

    // 🔘 ผูกกับปุ่ม "เริ่มใหม่ (Retry)" ในหน้า Game Over
    public void RestartGame()
    {
        Time.timeScale = 1f; // คืนค่าเวลา

        // 🌟 เรียกระบบรีเซ็ตและสุ่มด่านใหม่ของคุณอาร์มมาทำงานตรงนี้!
        if (GameManager.instance != null)
        {
            GameManager.instance.ResetRoguelike();
            GameManager.instance.LoadNextRandomMap();
        }
        else
        {
            // แผนสำรองถ้าไม่มี GameManager ให้รีเฟรชด่านเดิม
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // 🔘 ผูกกับปุ่ม "กลับห้องพัก" ในหน้า Win (ชนะบอส)
    public void ReturnToHub()
    {
        // 🌟 เพิ่มโค้ดท่อนนี้เข้าไป! เพื่อล้างความจำ GameManager ว่าให้เริ่มนับด่าน 1 ใหม่นะ!
       
        Time.timeScale = 1f;
        // 🌟 เพิ่มโค้ดท่อนนี้เข้าไป! เพื่อล้างความจำ GameManager ว่าให้เริ่มนับด่าน 1 ใหม่นะ!
        if (GameManager.instance != null)
        {
            GameManager.instance.ResetRoguelike();
        }
        // ⚠️ เปลี่ยน "HubRoom" เป็นชื่อฉากห้องเซฟของคุณอาร์มนะครับ
        SceneManager.LoadScene("SafeRoom");
    }

    // 🔘 ผูกกับปุ่ม "กลับเมนูหลัก"
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // ⚠️ เปลี่ยน "MainMenu" เป็นชื่อฉากหน้าเมนูของคุณอาร์ม
        SceneManager.LoadScene("Menu");
    }
}