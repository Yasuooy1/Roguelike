using UnityEngine;

public class MapWarp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.instance != null)
            {
                // เรียกใช้ฟังก์ชันสุ่มด่าน
                GameManager.instance.LoadNextRandomMap();
            }
        }
    }
}