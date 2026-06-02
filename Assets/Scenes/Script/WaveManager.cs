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
    public GameObject[] wave1Enemies;
    public GameObject[] wave2Enemies;
    public GameObject[] wave3Enemies;
    public GameObject[] normalEnemies;

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

    public bool gameStarted = false;
    public GameObject startTrigger;
    void Start()
    {
        if (ArrowIndicator.instance != null && startTrigger != null)
        {
            ArrowIndicator.instance.SetTarget(startTrigger.transform);
        }
    }

    public void BeginGame()
    {
        if (!gameStarted)
        {
            if (ArrowIndicator.instance != null) ArrowIndicator.instance.ClearTarget();
            gameStarted = true;
            isWaitingForReward = false;
            isSpawning = false;
            currentWave = 0;

            StopAllCoroutines();
            StartCoroutine(StartNextWave());
            Debug.Log("Game Started!");
        }
    }

    void Update()
    {
        if (isRoomCleared || isSpawning) return;

        aliveEnemies.RemoveAll(item => item == null);

        // 🌟 จังหวะที่ศัตรูตายหมดเวฟ!
        if (aliveEnemies.Count == 0 && currentWave > 0 && !isWaitingForReward)
        {
            isWaitingForReward = true;

            if (currentWave < totalWaves)
            {
                OnWaveClear();
            }
            else if (!isBossSpawned)
            {
                OnWaveClear();
            }
            else if (isBossSpawned)
            {
                isRoomCleared = true;

                // 🌟 ดึงบัฟออกตอนชนะบอสเคลียร์ด่าน
                if (BuffManager.instance != null)
                {
                    BuffManager.instance.OnWaveEnded();
                }

                if (warningDirectionText != null)
                {
                    warningDirectionText.text = "🎉 เคลียร์ด่านสำเร็จ!";
                    warningDirectionText.gameObject.SetActive(true);
                }
                if (EndGameManager.instance != null)
                {
                    EndGameManager.instance.ShowWinScreen();
                }
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

        Transform playerPos = GameObject.FindGameObjectWithTag("Player").transform;
        float centerX = (playerPos != null) ? playerPos.position.x : 0f;
        
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject randomEnemy = currentEnemyPool[Random.Range(0, currentEnemyPool.Length)];
            Transform chosenPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            if (currentWave == 1)
            {
                foreach (Transform pt in spawnPoints) { if (pt.position.x > centerX) { chosenPoint = pt; break; } }
            }
            else if (currentWave == 2)
            {
                foreach (Transform pt in spawnPoints) { if (pt.position.x < centerX) { chosenPoint = pt; break; } }

            }
            if (ArrowIndicator.instance != null)
            {
                ArrowIndicator.instance.SetTarget(chosenPoint);
            }


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
            if (ArrowIndicator.instance != null)
            {
                ArrowIndicator.instance.ClearTarget();
            }

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

    // ==========================================
    // 🌟 ฟังก์ชันจัดการเมื่อจบเวฟ
    // ==========================================
    // --- ส่วนที่แก้ไขใน OnWaveClear ---
    void OnWaveClear()
    {
        if (BuffManager.instance != null)
        {
            BuffManager.instance.OnWaveEnded();
        }

        // 🌟 เก็บค่า Object ที่เสกออกมาไว้ในตัวแปร shrine
        GameObject shrine = Instantiate(upgradeShrinePrefab, spawnPoint.position, Quaternion.identity);

        // 🌟 สั่งให้ลูกศรชี้ไปที่ shrine ตัวที่เพิ่งเสกออกมา (ไม่ใช่ตัว Prefab)
        if (ArrowIndicator.instance != null)
        {
            ArrowIndicator.instance.SetTarget(shrine.transform);
        }
    }

    // --- ส่วนที่แก้ไขใน ResumeWave ---
    public void ResumeWave()
    {
        if (isRoomCleared || !isWaitingForReward || isSpawning)
        {
            return;
        }

        // 🌟 เมื่อกดรับบัฟและเริ่มเวฟใหม่ ให้สั่งลบลูกศรทิ้งไปก่อนครับ
        if (ArrowIndicator.instance != null)
        {
            ArrowIndicator.instance.ClearTarget();
        }

        isWaitingForReward = false;

        if (currentWave < totalWaves)
        {
            StartCoroutine(StartNextWave());
        }
        else if (!isBossSpawned)
        {
            StartCoroutine(SpawnBoss());
        }
    }

    /*public void ResumeWave()
    {
        // 🌟 ใส่แม่กุญแจล็อคตรงนี้!
        // ถ้าเคลียร์ห้องแล้ว / ไม่ได้อยู่ในช่วงพักรับรางวัล / หรือกำลังเสกมอนสเตอร์อยู่ -> ห้ามรันคำสั่งซ้ำเด็ดขาด!
        if (isRoomCleared || !isWaitingForReward || isSpawning)
        {
            return;
        }

        isWaitingForReward = false; // ปลดล็อคว่ารับรางวัลเสร็จแล้ว

        if (currentWave < totalWaves)
        {
            StartCoroutine(StartNextWave());
        }
        else if (!isBossSpawned)
        {
            StartCoroutine(SpawnBoss());
        }
    }*/
}