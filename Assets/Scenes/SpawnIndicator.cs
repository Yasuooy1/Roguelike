using UnityEngine;
using System.Collections;

public class SpawnIndicator : MonoBehaviour
{
    [Header("ตั้งค่าเวลาเกิด")]
    public float spawnDelay = 1.5f; // เวลาหน่วง (วินาที) ให้ผู้เล่นตั้งตัว

    [Header("เอฟเฟกต์เตือน")]
    public GameObject warningGraphic; // ออบเจกต์รูปวงเวทกระพริบ หรือเครื่องหมายตกใจ (!)

    // ตัวแปรความจำ: เอาไว้จำว่าตัวเองต้องเสกมอนสเตอร์แบบไหนออกมา
    private GameObject enemyPrefabToSpawn;

    // 🌟 ฟังก์ชันนี้ WaveManager จะเป็นคนเรียกใช้ เพื่อฝากข้อมูลมอนสเตอร์มาให้
    public void Setup(GameObject enemyPrefab)
    {
        enemyPrefabToSpawn = enemyPrefab;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // 1. โชว์รูปเตือนภัยให้ผู้เล่นเห็น
        if (warningGraphic != null) warningGraphic.SetActive(true);

        // (💡 ถ้าคุณอาร์มมีไฟล์เสียง "วิ้งๆ" เตือนมอนเกิด สั่ง Play Sound ตรงนี้ได้เลย!)

        // 2. ⏳ รอเวลาให้ผู้เล่นเตรียมตัวหันหน้าไปหา
        yield return new WaitForSeconds(spawnDelay);

        // 3. ปิดรูปเตือน แล้วเสกมอนสเตอร์ตัวจริงออกมาตรงตำแหน่งนี้เลย!
        if (enemyPrefabToSpawn != null)
        {
            Instantiate(enemyPrefabToSpawn, transform.position, Quaternion.identity);
        }

        // 4. หมดหน้าที่แล้ว ลบวงเวทเตือนภัยนี้ทิ้งซะ
        Destroy(gameObject);
    }
}