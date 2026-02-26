using UnityEngine;
using System.Collections.Generic;

public class MonsterPuzzle : MonoBehaviour
{
    [Header("รหัสผ่านบนหัวมอนสเตอร์")]
    public string requiredRecipe; // รหัสผ่านที่ต้องใช้ฆ่าตัวนี้

    void Start()
    {
        // 🌟 ตอนมอนสเตอร์เกิด ให้มันสุ่มรหัสผ่าน 3 ธาตุเลย!
        // (จริงๆ สามารถสั่งให้มันวาด UI ลูกแก้วบนหัวแบบเดียวกับที่ผู้เล่นทำได้เลยนะครับ)
        GenerateRandomPuzzle();
    }

    void GenerateRandomPuzzle()
    {
        // รายชื่อธาตุที่มี
        string[] elements = { "Fire", "Water", "Lightning" };
        List<string> puzzleList = new List<string>();

        // สุ่มมา 3 ลูก
        puzzleList.Add(elements[Random.Range(0, 3)]);
        puzzleList.Add(elements[Random.Range(0, 3)]);
        puzzleList.Add(elements[Random.Range(0, 3)]);

        // เรียงลำดับตัวอักษรให้เหมือนที่ Player ทำ
        puzzleList.Sort();

        // รวมร่างเป็นรหัสผ่าน
        requiredRecipe = puzzleList[0] + puzzleList[1] + puzzleList[2];
        Debug.Log(gameObject.name + " เกิดมาพร้อมกับรหัสลับ: " + requiredRecipe);
    }

    // ฟังก์ชันนี้โดนเรียกจาก Bullet.cs ตอนที่กระสุนบินมาชน
    public void CheckPuzzleBullet(string playerRecipe)
    {
        if (playerRecipe == requiredRecipe)
        {
            Debug.Log("🎯 รหัสผ่านถูกต้อง! เกราะแตก / ตายทันที!");
            // ใส่เอฟเฟกต์ตาย หรือโดนดาเมจมหาศาลตรงนี้
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("❌ ผิดสูตร! ไม่ระคายผิว!");
            // อาจจะทำเอฟเฟกต์สะท้อนกระสุน หรือขึ้นคำว่า Miss!
        }
    }
}