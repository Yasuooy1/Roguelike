using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("ตั้งค่าเวฟ (Wave Settings)")]
    public int totalWaves = 5;         // จำนวนเวฟทั้งหมด
    private int currentWave = 0;       // ตอนนี้อยู่เวฟที่เท่าไหร่

    [Header("ศัตรูและบอส (Prefabs)")]
    public GameObject[] enemyPrefabs;  // ใส่ตัวศัตรูได้หลายๆ แบบ (ระบบจะสุ่มเสก)
    public GameObject bossPrefab;      // ใส่ Prefab ของบอส

    [Header("จุดเกิดศัตรู (Spawn Points)")]
    public Transform[] spawnPoints;    // ใส่จุดที่อยากให้ศัตรูโผล่มา

    [Header("สถานะปัจจุบัน (ห้ามแก้)")]
    public List<GameObject> aliveEnemies = new List<GameObject>();
    private bool isBossSpawned = false;
    private bool isRoomCleared = false;
    private bool isSpawning = false; // ตัวล็อกไม่ให้เสกซ้อนกัน

    void Start()
    {
        // เริ่มเกมปุ๊บ หน่วงเวลาแป๊บนึงแล้วเริ่มเวฟ 1
        StartCoroutine(StartNextWave());
    }

    void Update()
    {
        // ถ้าห้องเคลียร์แล้ว หรือกำลังยืนเสกมอนสเตอร์อยู่ ไม่ต้องทำอะไร
        if (isRoomCleared || isSpawning) return;

        // 💡 ทริคเด็ด: อัปเดตรายชื่อศัตรู (ถ้าศัตรูโดนตีตาย มันจะกลายเป็น null เราก็เตะมันออกจาก List)
        aliveEnemies.RemoveAll(item => item == null);

        // เช็กว่า "ศัตรูตายหมดเกลี้ยงแล้วใช่ไหม?"
        if (aliveEnemies.Count == 0 && currentWave > 0)
        {
            if (currentWave < totalWaves)
            {
                // ยังไม่ครบ 5 เวฟ -> เรียกเวฟต่อไป!
                StartCoroutine(StartNextWave());
            }
            else if (!isBossSpawned)
            {
                // ครบ 5 เวฟแล้ว -> เสกบอส!
                StartCoroutine(SpawnBoss());
            }
            else if (isBossSpawned)
            {
                // บอสโผล่มาแล้ว และบอสก็ตายแล้วด้วย! (เคลียร์ด่าน)
                isRoomCleared = true;
                Debug.Log("🎉 เคลียร์ห้องแล้ว! สุดยอดดด!");

                // TODO: ตรงนี้เดี๋ยวเราค่อยมาสั่งให้ "ประตูวาร์ป" โผล่มา หรือดรอปหีบสมบัติครับ
            }
        }
    }

    IEnumerator StartNextWave()
    {
        isSpawning = true;
        currentWave++; // อัปป้ายบอกเวฟ
        Debug.Log("ระวัง! เวฟที่ " + currentWave + " กำลังมา!");

        // ให้เวลาผู้เล่นหายใจเก็บเลือด 2 วินาทีก่อนศัตรูชุดใหม่จะเกิด
        yield return new WaitForSeconds(2f);

        // คำนวณจำนวนศัตรู (ยิ่งเวฟลึก ศัตรูยิ่งเยอะ เช่น เวฟ 1 มา 3 ตัว, เวฟ 2 มา 4 ตัว)
        int enemiesToSpawn = currentWave + 2;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // สุ่มเลือกศัตรู 1 ตัวจากในอาเรย์
            GameObject randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            // สุ่มเลือกจุดเกิด 1 จุด
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // สั่งเสกศัตรูลงฉาก
            GameObject spawnedEnemy = Instantiate(randomEnemy, randomPoint.position, Quaternion.identity);

            // จับศัตรูตัวนี้ยัดใส่ List เพื่อเอาไว้นับจำนวนตอนมันตาย
            aliveEnemies.Add(spawnedEnemy);

            // หน่วงเวลา 0.5 วินาทีต่อการเสก 1 ตัว (จะได้ดูเหมือนทยอยวาร์ปมา ไม่ได้โผล่พรวดเดียว)
            yield return new WaitForSeconds(0.5f);
        }

        isSpawning = false; // เสกเสร็จแล้ว ปลดล็อก
    }

    IEnumerator SpawnBoss()
    {
        isSpawning = true;
        isBossSpawned = true;
        Debug.Log("⚠️ คำเตือน! บอสปรากฏตัว!");

        // ให้เวลาพักหายใจ 3 วินาทีก่อนบอสลง
        yield return new WaitForSeconds(3f);

        // ให้บอสเกิดที่จุดเกิดอันแรกเสมอ (จะได้จัดตำแหน่งให้อยู่กลางห้องได้)
        Transform bossPoint = spawnPoints[0];

        GameObject spawnedBoss = Instantiate(bossPrefab, bossPoint.position, Quaternion.identity);
        aliveEnemies.Add(spawnedBoss); // จับบอสใส่ List ด้วย จะได้รู้ตอนมันตาย

        isSpawning = false;
    }
}
