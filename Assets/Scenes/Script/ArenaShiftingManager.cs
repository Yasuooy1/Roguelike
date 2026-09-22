using UnityEngine;
using System.Collections.Generic;

public class ArenaShiftingManager : MonoBehaviour
{
    [Header("จัดการอุปสรรคในฉาก (ลากใส่ให้ตรงหมวด)")]
    public List<GameObject> thornObstacles;
    public List<GameObject> electricObstacles;
    public List<GameObject> lavaObstacles;

    // ลิสต์รวมอุปสรรคทั้งหมดเอาไว้สุ่ม
    private List<GameObject> allObstacles = new List<GameObject>();

    void Start()
    {
        // 1. รวบรวมอุปสรรคทุกประเภทไว้ในตะกร้าเดียวกันตอนเริ่มเกม
        allObstacles.AddRange(thornObstacles);
        allObstacles.AddRange(electricObstacles);
        allObstacles.AddRange(lavaObstacles);

        // 2. เคลียร์ฉากให้โล่งก่อนเริ่มเวฟแรก
        ClearAllObstacles();
    }

    // ฟังก์ชันนี้ถูกเรียกจาก WaveManager ทุกครั้งที่เริ่มเวฟ
    public void OnWaveStarted(int currentWave)
    {
        // กำหนดว่าแต่ละเวฟจะให้มีกับดัก "งอกเพิ่ม" กี่จุด
        // เช่น เวฟ 1-2 งอกเพิ่มเวฟละ 1 จุด | เวฟ 3 เป็นต้นไป งอกเพิ่มเวฟละ 2 จุด (เพิ่มความตึง)
        int obstaclesToAdd = (currentWave >= 3) ? 2 : 1;

        Debug.Log($"🌪️ Arena Shifting เวฟ {currentWave}: สุ่มอุปสรรคงอกเพิ่ม {obstaclesToAdd} จุด!");

        // กรองหาเฉพาะอุปสรรคที่ยัง "ถูกซ่อนอยู่" (ผู้เล่นเผาทิ้งไปแล้ว หรือยังไม่งอก)
        List<GameObject> inactiveObstacles = new List<GameObject>();
        foreach (GameObject obj in allObstacles)
        {
            if (obj != null && !obj.activeInHierarchy)
            {
                inactiveObstacles.Add(obj);
            }
        }

        // ถ้าอุปสรรคงอกเต็มฉากหมดแล้ว ไม่มีเหลือให้งอก ก็ข้ามไป
        if (inactiveObstacles.Count == 0) return;

        // สุ่มเปิดการทำงานของอุปสรรค
        for (int i = 0; i < obstaclesToAdd; i++)
        {
            if (inactiveObstacles.Count > 0)
            {
                int randomIndex = Random.Range(0, inactiveObstacles.Count);
                GameObject chosenObstacle = inactiveObstacles[randomIndex];

                chosenObstacle.SetActive(true); // 🌟 สั่งให้งอก!
                inactiveObstacles.RemoveAt(randomIndex); // เอาออกจากลิสต์สุ่มชั่วคราวเพื่อไม่ให้ซ้ำ
            }
        }
    }

    public void ClearAllObstacles()
    {
        foreach (GameObject obj in allObstacles)
        {
            if (obj != null) obj.SetActive(false);
        }
    }
}