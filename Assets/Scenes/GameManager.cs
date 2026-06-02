using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("ตั้งค่าชื่อฉาก (Scenes)")]
    public string menuScene = "Menu";
    public string safeRoomScene = "SafeRoom"; // 🌟 เพิ่มช่องใส่ชื่อฉากห้องพัก

    [Header("ใส่ชื่อซีนด่านทั้งหมดที่นี่")]
    public List<string> allMaps = new List<string> { "map1", "map2", "map3" };
    private List<string> remainingMaps;

    void Awake()
    {
        // 🌟 ระบบอมตะ: ห้ามพัง ห้ามซ้ำ ลอยข้ามฉากได้!
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ResetRoguelike();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ฟังก์ชันสำหรับรีเซ็ตด่านใหม่ทั้งหมด (เริ่มรันใหม่)
    public void ResetRoguelike()
    {
        remainingMaps = new List<string>(allMaps);
        Debug.Log("Reset Roguelike: เติมลิสต์ด่านใหม่ พร้อมลุย!");
    }

    // ==========================================
    // 🌟 1. ปุ่ม "Start Game" ในหน้า Menu
    // ==========================================
    public void StartGameFromMenu()
    {
        // รีเซ็ตด่านให้กลับมาเต็มใหม่
        ResetRoguelike();

        // 🌟 เปลี่ยนจากการสุ่มด่าน เป็น "เข้าห้องพัก Safe Room" ก่อน!
        SceneManager.LoadScene(safeRoomScene);
    }

    // ==========================================
    // 🌟 2. ประตูวาร์ป (สุ่มและโหลดด่านถัดไป)
    // ==========================================
    public void LoadNextRandomMap()
    {
        if (remainingMaps.Count > 0)
        {
            int randomIndex = Random.Range(0, remainingMaps.Count);
            string nextMap = remainingMaps[randomIndex];
            remainingMaps.RemoveAt(randomIndex);

            Debug.Log("วาร์ปไปด่าน: " + nextMap);
            SceneManager.LoadScene(nextMap);
        }
        else
        {
            // 🌟 ถ้าสู้จนด่านหมดแล้ว ก็กลับมาที่ห้อง Safe Room ถือว่าจบเวฟ หรือจะให้เด้งกลับหน้า Menu ก็ได้ครับ
            Debug.Log("เคลียร์ทุกด่านแล้ว! กลับห้องพัก");
            SceneManager.LoadScene(safeRoomScene);
        }
    }

    // ==========================================
    // ปุ่ม UI ทั่วไป
    // ==========================================
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(menuScene);
    }

    public void GoToStatMenu()
    {
        SceneManager.LoadScene("UpgradeMenu");
    }

    public void QuitGame()
    {
        Debug.Log("ปิดเกมแล้วจ้า!");
        Application.Quit();
    }
}