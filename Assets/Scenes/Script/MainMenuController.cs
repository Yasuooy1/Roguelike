using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // 🌟 1. เพิ่มบรรทัดนี้เพื่อให้เรียกใช้ EventSystem ได้

public class MainMenuController : MonoBehaviour
{
    [Header("UI Navigation สำหรับจอย")]
    public GameObject firstMenuButton; // 🌟 2. ลากปุ่ม Start Game มาใส่ช่องนี้ใน Unity

    void Start()
    {
        // 🌟 3. สั่งให้จอยไปโฟกัสที่ปุ่มแรกทันทีตอนเปิดเมนู
        if (firstMenuButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // ล้างค่าเก่าก่อน
            EventSystem.current.SetSelectedGameObject(firstMenuButton);
        }
    }

    // ฟังก์ชันสำหรับปุ่ม Start
    public void ClickStartGame()
    {
        // สั่งงานบอส (GameManager ตัวที่เป็นอมตะ) ให้เริ่มเกม
        if (GameManager.instance != null)
        {
            GameManager.instance.StartGameFromMenu();
        }
        else
        {
            Debug.LogError("หา GameManager ไม่เจอครับ!");
        }
    }

    // ฟังก์ชันสำหรับปุ่มเข้าหน้า Stat
    public void ClickGoToStat()
    {
        // สั่งงานบอสให้เปลี่ยนหน้า
        if (GameManager.instance != null)
        {
            GameManager.instance.GoToStatMenu();
        }
    }

    // ฟังก์ชันสำหรับปุ่ม Exit (ถ้ามี)
    public void ClickQuitGame()
    {
        Debug.Log("ปิดเกมแล้วจ้า!");
        Application.Quit();
    }
}