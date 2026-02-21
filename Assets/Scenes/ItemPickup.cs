using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("ข้อมูลของชิ้นนี้ (ตั้งค่าให้ตรงกับรูปร่าง)")]
    // ตัวนี้จะดึงโครงสร้าง ItemData จากที่เราเขียนไว้ในสคริปต์ PlayerInventory มาใช้ครับ
    public ItemData itemInfo;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // เช็กว่าคนที่เดินมาชน มี Tag ว่า "Player" หรือไม่
        if (other.CompareTag("Player"))
        {
            // พยายามค้นหากระเป๋า (PlayerInventory) ในตัวผู้เล่น
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();

            if (inventory != null)
            {
                // 1. ส่งข้อมูลไอเทมชิ้นนี้ เข้าไปเก็บในกระเป๋า
                inventory.AddItem(itemInfo);

                // 2. ทำลายตัวเองทิ้ง (เสมือนว่าถูกเก็บไปแล้ว)
                Destroy(gameObject);
            }
        }
    }
}