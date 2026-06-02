using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Connect Settings")]
    public Transform roomExitPoint;   // จุดสิ้นสุดของห้องแรก (SampleScene)
    public GameObject[] roomPool;    // ลาก Prefab ห้องแบบต่างๆ (2, 3, 4) มาใส่ในนี้
    public GameObject bossRoomPrefab; // ห้องบอส (ล็อกไว้ท้ายสุด)

    [Header("Options")]
    public int roomsToSpawn = 3;     // จำนวนห้องสุ่มก่อนถึงบอส
    public float roomWidth = 20f;    // ความกว้างของห้อง Prefab

    void Start()
    {
        SpawnProceduralLevel();
    }

    void SpawnProceduralLevel()
    {
        for (int i = 0; i < roomsToSpawn; i++)
        {
            // 1. สุ่มเลือกห้องจาก Room Pool
            int randomIndex = Random.Range(0, roomPool.Length);

            // 2. คำนวณตำแหน่ง
            Vector3 spawnPos = roomExitPoint.position + new Vector3(i * roomWidth, 0, 0);

            // 3. เสกห้องสุ่มออกมา
            Instantiate(roomPool[randomIndex], spawnPos, Quaternion.identity);
        }

        // 4. เสกห้องบอสไว้ท้ายสุดเสมอ
        Vector3 bossRoomPos = roomExitPoint.position + new Vector3(roomsToSpawn * roomWidth, 0, 0);
        Instantiate(bossRoomPrefab, bossRoomPos, Quaternion.identity);
    }
}
