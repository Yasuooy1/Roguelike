using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Room Prefabs")]
    public GameObject startRoom;      // ลากห้องเริ่ม (ที่มีจุดเกิดผู้เล่น) มาใส่
    public GameObject[] normalRooms;  // ลากห้องธรรมดา (แม่พิมพ์ที่มีมอนสเตอร์) มาใส่
    public GameObject bossRoom;       // ลากห้องบอส (ที่มีบอส Souls-like ของเรา) มาใส่

    [Header("Generation Settings")]
    public int totalRooms = 5;         // อยากให้ด่านนี้มี่กี่ห้อง
    public float roomWidth = 20f;     // ความกว้างของห้องที่คุณออกแบบไว้

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        for (int i = 0; i < totalRooms; i++)
        {
            GameObject roomToSpawn;

            // ตรรกะ: ห้องแรก = เริ่ม, ห้องสุดท้าย = บอส, ที่เหลือ = สุ่ม
            if (i == 0)
            {
                roomToSpawn = startRoom;
            }
            else if (i == totalRooms - 1)
            {
                roomToSpawn = bossRoom;
            }
            else
            {
                int randomIndex = Random.Range(0, normalRooms.Length);
                roomToSpawn = normalRooms[randomIndex];
            }

            // คำนวณตำแหน่งวางห้อง (ต่อกันไปทางขวาเรื่อยๆ)
            Vector3 spawnPosition = new Vector3(i * roomWidth, 0, 0);

            // สร้างห้องออกมาในฉาก
            GameObject spawnedRoom = Instantiate(roomToSpawn, spawnPosition, Quaternion.identity);
            spawnedRoom.name = "Room_" + i;
        }
    }
}