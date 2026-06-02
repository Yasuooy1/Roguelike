using UnityEngine;

public class WarpPortal : MonoBehaviour
{
    private bool hasWarped = false; // 🌟 ตัวล็อก ป้องกันการเดินชนซ้ำแล้วโหลดฉากเบิ้ล

    // ฟังก์ชันนี้จะทำงานทันทีที่มีคนเดินเข้ามาในกรอบ (Trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ถ้าเคยวาร์ปไปแล้ว ให้หยุดทำงานเลย (กันบั๊ก)
        if (hasWarped) return;

        // เช็กว่าสิ่งที่เดินชนคือผู้เล่นใช่ไหม
        if (collision.CompareTag("Player"))
        {
            hasWarped = true; // 🌟 ล็อกประตูทันที!
            Debug.Log("🌀 ผู้เล่นเดินชนประตูวาร์ป! กำลังไปห้องถัดไป...");
            if (HubRoomGuide.instance != null) HubRoomGuide.instance.TurnOffArrow();

            // 🌟 เรียกใช้ GameManager เพื่อโหลดด่านสุ่มถัดไป
            if (GameManager.instance != null)
            {
                GameManager.instance.LoadNextRandomMap();
            }
            else
            {
                Debug.LogError("🚨 หา GameManager ไม่เจอ! อย่าลืมวาง GameManager ไว้ในฉากด้วยนะครับ");
            }
        }
    }
}