using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Header("UI กรอบกระเป๋า")]
    public Image itemSlotUI;       // ลากกรอบ UI ที่สร้างบนหน้าจอมาใส่ช่องนี้
    public Sprite emptySlotSprite; // รูปรองพื้นเวลาที่กระเป๋าว่างเปล่า (ไม่มีของ)

    [Header("ของที่มีในกระเป๋า")]
    public List<ItemData> inventoryList = new List<ItemData>();
    private int currentIndex = 0; // ตัวบอกว่าตอนนี้กำลังเลือกไอเทมชิ้นไหนอยู่

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        // 🔄 กดปุ่ม E เพื่อ "หมุนเปลี่ยนไอเทม" (แบบ Elden Ring)
        if (Input.GetKeyDown(KeyCode.E))
        {
            CycleItem();
        }

        // 💥 กดปุ่ม Q เพื่อ "ใช้งานไอเทม" ที่เลือกอยู่
        if (Input.GetKeyDown(KeyCode.F))
        {
            UseCurrentItem();
        }
    }

    // ฟังก์ชันสำหรับหมุนไอเทม
    void CycleItem()
    {
        if (inventoryList.Count == 0) return; // ถ้ากระเป๋าว่าง ไม่ต้องสลับ

        currentIndex++; // เลื่อนไปชิ้นถัดไป

        // ถ้าเลื่อนจนสุดรายการแล้ว ให้วนกลับมาที่ชิ้นแรกใหม่ (0)
        if (currentIndex >= inventoryList.Count)
        {
            currentIndex = 0;
        }

        UpdateUI(); // อัปเดตหน้าจอ
        Debug.Log("หมุนมาที่ไอเทม: " + inventoryList[currentIndex].itemName);
    }

    // ฟังก์ชันสำหรับใช้ไอเทม
    void UseCurrentItem()
    {
        if (inventoryList.Count == 0) return; // ไม่มีของให้ใช้

        ItemData itemToUse = inventoryList[currentIndex];

        // ----------------------------------------------------
        // 🧪 เช็กว่าของที่กดใช้คืออะไร (พิมพ์ชื่อให้ตรงกับที่ตั้งไว้ใน Inspector นะครับ)
        if (itemToUse.itemName == "HealPotion")
        {
            PlayerHealth health = GetComponent<PlayerHealth>(); // ดึงสคริปต์เลือดมา

            if (health != null)
            {
                // เช็กก่อนว่าเลือดเต็มไหม ถ้าเต็มไม่ให้ใช้ (จะได้ไม่เสียของฟรี)
                if (health.currentHealth >= health.maxHealth)
                {
                    Debug.Log("เลือดเต็มอยู่แล้ว เก็บยาไว้ใช้ตอนเจ็บดีกว่า!");
                    return; // สั่งหยุดการทำงานตรงนี้เลย จะได้ไม่โดนหักยา
                }

                health.Heal(1); // สั่งเพิ่มเลือด 1 ดวง (เปลี่ยนตัวเลขได้)
                Debug.Log("ซดยาเพิ่มเลือดเรียบร้อย!");
            }
        }
        // ----------------------------------------------------

        // ใช้เสร็จแล้ว ลบของชิ้นนั้นออกจากกระเป๋า
        inventoryList.RemoveAt(currentIndex);

        // ปรับลำดับหน้าจอให้ถูกต้อง
        if (currentIndex >= inventoryList.Count)
        {
            currentIndex = 0;
        }

        UpdateUI();
    }

    // ฟังก์ชันสำหรับเสกของเข้ากระเป๋าเวลาเดินชน (เดี๋ยวเราค่อยเรียกใช้)
    public void AddItem(ItemData newItem)
    {
        inventoryList.Add(newItem);
        UpdateUI();
        Debug.Log("เก็บได้: " + newItem.itemName);
    }

    // ฟังก์ชันวาดรูปไอเทมบนหน้าจอ
    void UpdateUI()
    {
        if (itemSlotUI == null) return;

        if (inventoryList.Count > 0)
        {
            // ถ้ามีของ ให้เอารูปไอเทมชิ้นนั้นมาแสดง
            itemSlotUI.sprite = inventoryList[currentIndex].itemIcon;
            itemSlotUI.color = Color.white; // ทำให้สีสว่างชัดเจน
        }
        else
        {
            // ถ้ากระเป๋าว่างเปล่า ให้โชว์กรอบเปล่าๆ
            itemSlotUI.sprite = emptySlotSprite;
            // ถ้าไม่มีรูปกรอบเปล่า ให้ปรับให้มันโปร่งใสแทน
            if (emptySlotSprite == null) itemSlotUI.color = new Color(1, 1, 1, 0.2f);
        }
    }
}

// โครงสร้างข้อมูลไอเทม (เพื่อให้เราสร้างไอเทมได้หลากหลาย)
[System.Serializable]
public class ItemData
{
    public string itemName;  // ชื่อไอเทม
    public Sprite itemIcon;  // รูปไอคอนของไอเทม
}