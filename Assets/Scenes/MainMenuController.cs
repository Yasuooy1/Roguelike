using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
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