using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("UI เมนูหยุดเกม")]
    public GameObject pauseMenuUI;
    [Header("ปุ่มแรกที่จะให้เลือก")]
    public GameObject firstSelectedButton;

    void Update()
    {
        // 🌟 เพิ่มการเช็ก: ถ้าหน้าต่างเลือกการ์ด (BuffPanel) เปิดอยู่ ห้ามกดปุ่ม Pause ซ้ำเด็ดขาด!
        if (BuffManager.instance != null && BuffManager.instance.buffPanel != null && BuffManager.instance.buffPanel.activeSelf)
        {
            return; // ออกจากฟังก์ชันทันที ไม่ให้กด Pause ได้
        }

        // เช็กว่าผู้เล่นกดปุ่ม ESC หรือปุ่ม Start บนจอย
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // ฟังก์ชันให้เกมเดินต่อ
    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);

        // 🌟 เช็กย้ำอีกชั้น: ถ้าหน้าจอเลือกการ์ดไม่ได้เปิดอยู่ ถึงจะยอมคืนเวลา Time.timeScale = 1f
        bool isBuffPanelActive = (BuffManager.instance != null && BuffManager.instance.buffPanel != null && BuffManager.instance.buffPanel.activeSelf);

        if (!isBuffPanelActive)
        {
            Time.timeScale = 1f;
            GameIsPaused = false;
        }
    }

    // ฟังก์ชันหยุดเกม
    void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    // ฟังก์ชันสำหรับปุ่ม Quit ออกเกม
    public void QuitGame()
    {
        Time.timeScale = 1f; // คืนเวลาก่อนเปลี่ยนฉากเสมอ
        GameIsPaused = false;
        SceneManager.LoadScene("Menu"); // หรือชื่อซีนเมนูหลักของคุณอาร์ม
    }
}