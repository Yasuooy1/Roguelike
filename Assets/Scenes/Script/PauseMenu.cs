using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // ใช้สำหรับโหลดซีนหรือออกเกม

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false; // ตัวแปรเช็กว่าเกมหยุดอยู่ไหม

    [Header("UI เมนูหยุดเกม")]
    public GameObject pauseMenuUI;
    [Header("ปุ่มแรกที่จะให้เลือก")]
    public GameObject firstSelectedButton; // 🌟 2. สร้างช่องไว้ใส่ปุ่ม Resume

    void Update()
    {
        // เช็กว่าผู้เล่นกดปุ่ม ESC หรือไม่
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume(); // ถ้าหยุดอยู่ ให้เล่นต่อ
            }
            else
            {
                Pause();  // ถ้าเล่นอยู่ ให้หยุดเกม
            }
        }
    }

    // ฟังก์ชันให้เกมเดินต่อ
    public void Resume()
    {
        pauseMenuUI.SetActive(false); // ซ่อนเมนู
        Time.timeScale = 1f;          // ให้เวลาเดินปกติ (ความเร็ว 1)
        GameIsPaused = false;
    }

    // ฟังก์ชันหยุดเกม
    void Pause()
    {
        pauseMenuUI.SetActive(true);  // โชว์เมนู
        Time.timeScale = 0f;          // หยุดเวลา (ความเร็ว 0) ทุกอย่างจะหยุดนิ่ง
        GameIsPaused = true;
        EventSystem.current.SetSelectedGameObject(null); // เคลียร์การเลือกเก่าทิ้งก่อน
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    // ฟังก์ชันสำหรับปุ่ม Quit ออกเกม
    public void QuitGame()
    {
        Debug.Log("Quitting game..."); // แสดงข้อความใน Console ไว้เช็ก
        Application.Quit();            // ปิดเกม (จะทำงานจริงตอนบิลด์เกมเป็น .exe แล้ว)
    }
}