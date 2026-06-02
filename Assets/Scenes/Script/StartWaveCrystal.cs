using UnityEngine;

public class StartWaveCrystal : MonoBehaviour
{
    [Header("ลาก WaveManager ในฉากมาใส่ช่องนี้")]
    public WaveManager waveManager;

    [Header("เอฟเฟกต์ตอนแท่นระเบิด (ไม่ใส่ก็ได้)")]
    public GameObject effectOnStart;

    // ฟังก์ชันนี้จะทำงานเมื่อมีอะไรบางอย่างบินมาชนกล่อง Collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 🌟 ใส่บรรทัดนี้เพื่อฟ้องว่า "มีอะไรบางอย่างมาชนแล้วนะ!"
        Debug.Log("โดนชนโดย: " + collision.gameObject.name + " (ป้ายชื่อ: " + collision.tag + ")");

        if (collision.CompareTag("Bullet"))
        {
            Debug.Log("กระสุนโดนเป้าแล้ว! กำลังสั่งเริ่มเวฟ..."); // 🌟 บอกว่าเงื่อนไขผ่าน
            if (waveManager != null)
            {
                waveManager.BeginGame();
            }
            gameObject.SetActive(false);
        }
    }
}