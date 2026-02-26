using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("ใส่ชื่อซีนด่านทั้งหมดที่นี่")]
    public List<string> allMaps = new List<string> { "map1", "map2", "map3" };
    private List<string> remainingMaps;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ResetRoguelike();
        }
        else { Destroy(gameObject); }
    }

    // ฟังก์ชันสำหรับรีเซ็ตด่านใหม่ทั้งหมด
    public void ResetRoguelike()
    {
        remainingMaps = new List<string>(allMaps);
        Debug.Log("Reset Roguelike: ด่านถูกรีเซ็ตใหม่");
    }

    // ฟังก์ชันสำหรับสุ่มและโหลดด่านถัดไป
    public void LoadNextRandomMap()
    {
        if (remainingMaps.Count > 0)
        {
            int randomIndex = Random.Range(0, remainingMaps.Count);
            string nextMap = remainingMaps[randomIndex];
            remainingMaps.RemoveAt(randomIndex);
            SceneManager.LoadScene(nextMap);
        }
        else
        {
            // ถ้าด่านหมด ให้กลับไปหน้าเมนู หรือห้องบอส
            SceneManager.LoadScene("Menu"); // ⚠️ เช็กให้ชัวร์ว่าชื่อซีนเมนูคือ "Menu" นะครับ
        }
    }

    // ==========================================
    // 🌟 ส่วนที่เพิ่มเข้ามาใหม่สำหรับปุ่ม UI ครับ 🌟
    // ==========================================

    // 1. ฟังก์ชันสำหรับปุ่ม "Start Game" ในหน้า Menu
    public void StartGameFromMenu()
    {
        // ล้างข้อมูลเซฟเก่าทิ้ง (เพื่อให้เริ่มเล่นใหม่ เงิน/เลือด/สเตตัส กลับเป็น 0)
        //PlayerPrefs.DeleteAll();//

        // รีเซ็ตลิสต์ด่านให้กลับมาเต็มใหม่
        ResetRoguelike();

        // สุ่มโหลดด่านแรกทันที!
        LoadNextRandomMap();
    }

    // 2. ฟังก์ชันสำหรับปุ่ม "Main Menu" (กดเพื่อกลับหน้าแรกตอนตายหรือหยุดเกม)
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // 3. ฟังก์ชันสำหรับปุ่ม "Exit / Quit" ปิดเกม
    public void QuitGame()
    {
        Debug.Log("ปิดเกมแล้วจ้า!");
        Application.Quit(); // จะทำงานจริงตอนพอร์ตเกมเป็น .exe แล้วครับ
    }
    // ฟังก์ชันสำหรับปุ่มไปหน้า Stat / อัปเกรด
    public void GoToStatMenu()
    {
        // ⚠️ เปลี่ยนคำว่า "StatScene" ให้ตรงกับชื่อไฟล์ฉากหน้าสเตตัสของคุณอาร์มเป๊ะๆ นะครับ
        SceneManager.LoadScene("UpgradeMenu");
    }
}