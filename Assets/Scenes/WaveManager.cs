using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("UI แสดงผลบนหน้าจอ")]
    public TextMeshProUGUI waveCountText;
    public TextMeshProUGUI warningDirectionText;

    [Header("ตั้งค่าเวฟ (Wave Settings)")]
    public int totalWaves = 5;
    private int currentWave = 0;

    // ==========================================
    // 🌟 ระบบแยกมอนสเตอร์ตามเวฟ (Tutorial System)
    // ==========================================
    [Header("ศัตรูแยกตามเวฟ (Tutorial)")]
    public GameObject[] wave1Enemies; // ใส่เฉพาะมอนธาตุเดียว (เช่น แดงล้วน, ฟ้าล้วน)
    public GameObject[] wave2Enemies; // ใส่เฉพาะมอนธาตุผสม (เช่น แดงแดงฟ้า)
    public GameObject[] wave3Enemies; // ใส่ตัวบิน หรือตัวที่อยากให้สอนในเวฟ 3
    public GameObject[] normalEnemies; // ใส่ศัตรู "ทุกแบบ" รวมกัน (ใช้ตั้งแต่เวฟ 4 ขึ้นไป)

    [Header("บอส")]
    public GameObject bossPrefab;

    [Header("จุดเกิดศัตรู (Spawn Points)")]
    public Transform[] spawnPoints;

    [Header("สถานะปัจจุบัน (ห้ามแก้)")]
    public List<GameObject> aliveEnemies = new List<GameObject>();
    private bool isBossSpawned = false;
    private bool isRoomCleared = false;
    private bool isSpawning = false;

    private bool isWaitingForReward = false;

    [Header("ระบบแท่นอัปเกรด & แจ้งเตือน")]
    public GameObject upgradeShrinePrefab;
    public Transform spawnPoint;
    public GameObject warningPrefab;

    void Start()
    {
        if (warningDirectionText != null) warningDirectionText.gameObject.SetActive(false);
        UpdateWaveUI();
        StartCoroutine(StartNextWave());
    }

    void Update()
    {
        if (isRoomCleared || isSpawning) return;

        aliveEnemies.RemoveAll(item => item == null);

        if (aliveEnemies.Count == 0 && currentWave > 0 && !isWaitingForReward)
        {
            isWaitingForReward = true;

            if (currentWave < totalWaves) OnWaveClear();
            else if (!isBossSpawned) OnWaveClear();
            else if (isBossSpawned)
            {
                isRoomCleared = true;
                if (warningDirectionText != null)
                {
                    warningDirectionText.text = "🎉 เคลียร์ด่านสำเร็จ!";
                    warningDirectionText.gameObject.SetActive(true);
                }
                OnWaveClear();
            }
        }
    }

    void UpdateWaveUI()
    {
        if (waveCountText != null) waveCountText.text = "Wave: " + currentWave + " / " + totalWaves;
    }

    IEnumerator StartNextWave()
    {
        isSpawning = true;
        currentWave++;
        UpdateWaveUI();

        yield return new WaitForSeconds(2f);

        int enemiesToSpawn = currentWave + 2;

        GameObject[] currentEnemyPool;
        if (currentWave == 1) currentEnemyPool = wave1Enemies;
        else if (currentWave == 2) currentEnemyPool = wave2Enemies;
        else if (currentWave == 3) currentEnemyPool = wave3Enemies;
        else currentEnemyPool = normalEnemies;

        if (currentEnemyPool.Length == 0) currentEnemyPool = normalEnemies;

        // 🌟 เพิ่มบรรทัดนี้: หาตำแหน่งผู้เล่นเพื่อเอามาเป็นจุดอ้างอิงตรงกลาง
        Transform playerPos = GameObject.FindGameObjectWithTag("Player").transform;
        float centerX = (playerPos != null) ? playerPos.position.x : 0f;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject randomEnemy = currentEnemyPool[Random.Range(0, currentEnemyPool.Length)];
            Transform chosenPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // 🌟 แก้ระบบหาจุดเกิดซ้าย-ขวา โดยเทียบกับตำแหน่งผู้เล่นแทน!
            if (currentWave == 1)
            {
                // เวฟ 1 บังคับเกิดขวา (หาจุดที่ตำแหน่ง x มากกว่าตัวผู้เล่น)
                foreach (Transform pt in spawnPoints) { if (pt.position.x > centerX) { chosenPoint = pt; break; } }
            }
            else if (currentWave == 2)
            {
                // เวฟ 2 บังคับเกิดซ้าย (หาจุดที่ตำแหน่ง x น้อยกว่าตัวผู้เล่น)
                foreach (Transform pt in spawnPoints) { if (pt.position.x < centerX) { chosenPoint = pt; break; } }
            }

            // 🌟 แจ้งเตือนทิศทางให้ถูกต้องตามตัวผู้เล่น
            if (warningDirectionText != null)
            {
                string direction = (chosenPoint.position.x < centerX) ? "ซ้าย" : "ขวา";
                warningDirectionText.text = "⚠️ ระวัง! ศัตรูมาทาง" + direction + "!";
                warningDirectionText.gameObject.SetActive(true);
            }

            GameObject warning = null;
            if (warningPrefab != null) warning = Instantiate(warningPrefab, chosenPoint.position, Quaternion.identity);

            yield return new WaitForSeconds(1.5f);

            if (warningDirectionText != null) warningDirectionText.gameObject.SetActive(false);
            if (warning != null) Destroy(warning);

            GameObject spawnedEnemy = Instantiate(randomEnemy, chosenPoint.position, Quaternion.identity);

            Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.SetTutorialWave(currentWave, i);
            }

            aliveEnemies.Add(spawnedEnemy);

            yield return new WaitForSeconds(0.5f);
        }

        isSpawning = false;
    }

    IEnumerator SpawnBoss()
    {
        isSpawning = true;
        isBossSpawned = true;

        if (warningDirectionText != null)
        {
            warningDirectionText.text = "⚠️ คำเตือน! บอสปรากฏตัว!";
            warningDirectionText.color = Color.red;
            warningDirectionText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(3f);

        if (warningDirectionText != null) warningDirectionText.gameObject.SetActive(false);

        Transform bossPoint = spawnPoints[0];
        GameObject warning = null;
        if (warningPrefab != null)
        {
            warning = Instantiate(warningPrefab, bossPoint.position, Quaternion.identity);
            warning.transform.localScale = new Vector3(3f, 3f, 1f);
        }

        yield return new WaitForSeconds(1.5f);
        if (warning != null) Destroy(warning);

        GameObject spawnedBoss = Instantiate(bossPrefab, bossPoint.position, Quaternion.identity);
        aliveEnemies.Add(spawnedBoss);

        BossCutscene cutsceneManager = FindObjectOfType<BossCutscene>();
        if (cutsceneManager != null)
        {
            cutsceneManager.PlayCutscene(spawnedBoss.transform);
        }

        isSpawning = false;
    }

    void OnWaveClear()
    {
        Instantiate(upgradeShrinePrefab, spawnPoint.position, Quaternion.identity);
    }

    public void ResumeWave()
    {
        if (isRoomCleared) return;
        isWaitingForReward = false;

        if (currentWave < totalWaves) StartCoroutine(StartNextWave());
        else if (!isBossSpawned) StartCoroutine(SpawnBoss());
    }
}