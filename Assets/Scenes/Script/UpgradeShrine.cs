using UnityEngine;

public class UpgradeShrine : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        // ต้องมั่นใจว่าตัวละครผู้เล่น ตั้ง Tag เป็นคำว่า "Player" เป๊ะๆ นะครับ!
        if (collision.CompareTag("Player"))
        {
            Debug.Log("แตะเสาแล้ว! สุ่มบัฟเลย!");

            // เรียกคำสั่งสุ่มการ์ดขึ้นมาโชว์ทันที
            if (BuffManager.instance != null)
            {
                BuffManager.instance.ShowBuffSelection();
            }

            // เปิดหน้าต่างเสร็จ ก็ทำลายเสาทิ้งไปเลย
            Destroy(gameObject);
        }
    }
}