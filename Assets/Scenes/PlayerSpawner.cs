using UnityEngine;
using Cinemachine; // ต้องพิมพ์บรรทัดนี้เพิ่ม เพื่อให้โค้ดรู้จักกล้อง Cinemachine

public class PlayerSpawner : MonoBehaviour
{
    [Header("ใส่ Prefab ตัวละครผู้เล่นที่นี่")]
    public GameObject playerPrefab;

    void Start()
    {
        if (playerPrefab != null)
        {
            // 1. เสกตัวละครออกมา
            GameObject spawnedPlayer = Instantiate(playerPrefab, transform.position, Quaternion.identity);
            spawnedPlayer.name = "Player"; // ตั้งชื่อให้เป็น Player เผื่อสคริปต์อื่นเรียกหา

            // 2. ค้นหากล้อง Cinemachine ในฉาก แล้วสั่งให้ตามตัวละครใหม่ทันที!
            CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
            if (vcam != null)
            {
                vcam.Follow = spawnedPlayer.transform;
                // ถ้าเกมเป็นแนว Top-Down หรือต้องการให้กล้องหันตามด้วย ให้เอา // บรรทัดล่างออก
                // vcam.LookAt = spawnedPlayer.transform; 
            }
            else
            {
                Debug.LogWarning("หากล้อง CM vcam1 ไม่เจอครับ");
            }
        }
    }
}