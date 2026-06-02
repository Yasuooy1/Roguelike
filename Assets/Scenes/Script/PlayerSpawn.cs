using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    void Start()
    {
        // ตามหาวัตถุที่มี Tag ว่า "SpawnPoint"
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");

        if (spawnPoint != null)
        {
            // ดึงตำแหน่ง X, Y จากจุดเกิด แต่บังคับให้แกน Z เป็น 0 เพื่อไม่ให้ตัวละครจมฉาก
            Vector3 newPosition = spawnPoint.transform.position;
            newPosition.z = 0f;

            transform.position = newPosition;
            Debug.Log("จัดตัวละครลงจุดเกิดเรียบร้อย!");
        }
        else
        {
            Debug.LogWarning("หาจุดเกิดไม่เจอ! อย่าลืมตั้ง Tag เป็น 'SpawnPoint' นะครับ");
        }
    }
}