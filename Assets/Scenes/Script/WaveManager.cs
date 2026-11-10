using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    [Header("UI แสดงผลบนหน้าจอ")]
    public TextMeshProUGUI waveCountText;
    public TextMeshProUGUI warningDirectionText;

    [Header("ตั้งค่าเวฟ (Wave Settings)")]
    public int totalWaves = 1; // ตั้งไว้ 1 สำหรับเทสต์
    private int currentWave = 0;

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
    private bool hasClearedFirstBoss = false; // 🌟 เช็กว่าผ่านบอสตัวแรกไปหรือยัง

    [Header("ระบบแท่นอัปเกรด & แจ้งเตือน")]
    public GameObject upgradeShrinePrefab;
    public Transform spawnPoint;
    public GameObject warningPrefab;

    [Header("🌟 ระบบร้านค้าหลังตีบอส")]
    public GameObject npcShopPrefab;
    public Transform shopSpawnPoint;
    private GameObject currentShopInstance;

    [Header("🌟 ประตูมิติไปเวฟต่อไป")]
    public GameObject portalPrefab;
    public Transform portalSpawnPoint;
    private GameObject currentPortalInstance;

    public bool gameStarted = false;
    public GameObject startTrigger;

    [Header("สถานะปัจจุบัน (ห้ามแก้)")]
   
    public int loopCount = 0; // 🌟 เพิ่มตัวแปรนับรอบ Loop


    void Awake()
    {
        instance = this;
    }

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
            hasClearedFirstBoss = false; // เริ่มเกมใหม่ รีเซ็ตสถานะบอส
            loopCount = 0; //
            aliveEnemies.Clear();

            StopAllCoroutines();
            StartCoroutine(StartNextWave());
        }
    }

    void Update()
    {
        if (isRoomCleared || isSpawning) return;

        aliveEnemies.RemoveAll(item => item == null);

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
            else
            {
                if (BuffManager.instance != null) BuffManager.instance.OnWaveEnded();

                GameObject shrine = Instantiate(upgradeShrinePrefab, spawnPoint.position, Quaternion.identity);
                UpgradeShrine shrineScript = shrine.GetComponent<UpgradeShrine>();
                if (shrineScript != null) shrineScript.isBossShrine = true;

                if (npcShopPrefab != null && shopSpawnPoint != null)
                {
                    currentShopInstance = Instantiate(npcShopPrefab, shopSpawnPoint.position, Quaternion.identity);
                }
                if (portalPrefab != null && portalSpawnPoint != null)
                {
                    currentPortalInstance = Instantiate(portalPrefab, portalSpawnPoint.position, Quaternion.identity);
                }

                if (ArrowIndicator.instance != null) ArrowIndicator.instance.SetTarget(shrine.transform);
            }
        }
    }

    void UpdateWaveUI()
    {
        if (waveCountText != null)
        {
            if (hasClearedFirstBoss)
            {
                // ถ้าเข้า Endless โหมดแล้ว โชว์แค่นี้พอ เช่น "Wave: 4", "Wave: 5"
                waveCountText.text = "Wave: " + currentWave;
            }
            else
            {
                // ช่วงแรกยังโชว์ปกติ เช่น "Wave: 1 / 3"
                waveCountText.text = "Wave: " + currentWave + " / " + totalWaves;
            }
        }
    }

    IEnumerator StartNextWave()
    {
        isSpawning = true;
        currentWave++;
        if (ScoreManager.instance != null) ScoreManager.instance.UpdateWave(currentWave);
        UpdateWaveUI();

        yield return new WaitForSeconds(2f);

        int enemiesToSpawn = currentWave + 2;
        GameObject[] currentEnemyPool;

        // 🌟 ถ้ารอบแรกยังไม่เจอ Boss ให้ใช้ระบบแยกเวฟ 1, 2, 3 ตามปกติ
        // แต่ถ้าผ่านบอสตัวแรกมาแล้ว (Endless Loop) จะดึงจาก normalEnemies มาคละกันทุกเวฟยาวๆ
        if (hasClearedFirstBoss)
        {
            currentEnemyPool = normalEnemies;
        }
        else
        {
            if (currentWave == 1) currentEnemyPool = wave1Enemies;
            else if (currentWave == 2) currentEnemyPool = wave2Enemies;
            else if (currentWave == 3) currentEnemyPool = wave3Enemies;
            else currentEnemyPool = normalEnemies;
        }

        if (currentEnemyPool.Length == 0) currentEnemyPool = normalEnemies;

        Transform playerPos = GameObject.FindGameObjectWithTag("Player").transform;
        float centerX = (playerPos != null) ? playerPos.position.x : 0f;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject randomEnemy = currentEnemyPool[Random.Range(0, currentEnemyPool.Length)];
            Transform chosenPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            if (!hasClearedFirstBoss && currentWave == 1)
            {
                foreach (Transform pt in spawnPoints) { if (pt.position.x > centerX) { chosenPoint = pt; break; } }
            }
            else if (!hasClearedFirstBoss && currentWave == 2)
            {
                foreach (Transform pt in spawnPoints) { if (pt.position.x < centerX) { chosenPoint = pt; break; } }
            }

            if (ArrowIndicator.instance != null) ArrowIndicator.instance.SetTarget(chosenPoint);

            if (warningDirectionText != null)
            {
                string direction = (chosenPoint.position.x < centerX) ? "ซ้าย" : "ขวา";
                warningDirectionText.text = "ระวัง ศัตรูมาทาง" + direction;
                warningDirectionText.gameObject.SetActive(true);
            }

            GameObject warning = null;
            if (warningPrefab != null) warning = Instantiate(warningPrefab, chosenPoint.position, Quaternion.identity);

            yield return new WaitForSeconds(1.5f);

            if (warningDirectionText != null) warningDirectionText.gameObject.SetActive(false);
            if (warning != null) Destroy(warning);

            GameObject spawnedEnemy = Instantiate(randomEnemy, chosenPoint.position, Quaternion.identity);
            if (ArrowIndicator.instance != null) ArrowIndicator.instance.ClearTarget();
            // 🌟 เช็กมอนสเตอร์เดินพื้น
            Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.SetTutorialWave(currentWave, i);
                enemyScript.SetLoopDifficulty(hasClearedFirstBoss, loopCount, currentWave);
            }
            FlyingEnemy flyingEnemyScript = spawnedEnemy.GetComponent<FlyingEnemy>();
            if (flyingEnemyScript != null)
            {
                flyingEnemyScript.SetLoopDifficulty(hasClearedFirstBoss, loopCount, currentWave);
            }
            

            
            if (enemyScript != null) enemyScript.SetTutorialWave(currentWave, i);

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
            warningDirectionText.text = "คำเตือน บอสปรากฏตัว";
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
        Boss bossScript = spawnedBoss.GetComponent<Boss>();
        if (bossScript != null)
        {
            bossScript.SetLoopDifficulty(hasClearedFirstBoss, loopCount);
        }

        BossCutscene cutsceneManager = FindObjectOfType<BossCutscene>();
        if (cutsceneManager != null) cutsceneManager.PlayCutscene(spawnedBoss.transform);

        isSpawning = false;
    }

    void OnWaveClear()
    {
        if (BuffManager.instance != null) BuffManager.instance.OnWaveEnded();
        GameObject shrine = Instantiate(upgradeShrinePrefab, spawnPoint.position, Quaternion.identity);
        if (ArrowIndicator.instance != null) ArrowIndicator.instance.SetTarget(shrine.transform);
    }

    public void ResumeWave()
    {
        if (isRoomCleared || !isWaitingForReward || isSpawning) return;
        if (ArrowIndicator.instance != null) ArrowIndicator.instance.ClearTarget();

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

    // 🌟 พอผ่านบอสตัวแรกแล้วเข้าประตู จะเริ่มลูป Endless โดยบังคับใช้ Normal Enemies คละกันยาวๆ
    // 🌟 พอผ่านบอสตัวแรกแล้วเข้าประตู จะเริ่มลูป Endless
    public void StartEndlessLoop()
    {
        if (currentShopInstance != null) Destroy(currentShopInstance);
        if (currentPortalInstance != null) Destroy(currentPortalInstance);

        
        // ✅ ให้เป้าหมายเวฟของบอสตัวถัดไป ขยับออกไปอีก 3 เวฟ
        totalWaves = currentWave + 3;

        isBossSpawned = false;
        isWaitingForReward = false;
        hasClearedFirstBoss = true;
        loopCount++;

        StartCoroutine(StartNextWave());
    }
}