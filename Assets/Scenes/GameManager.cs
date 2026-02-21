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
            SceneManager.LoadScene("Menu");
        }
    }
}