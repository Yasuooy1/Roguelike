using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // 🌟 เพิ่มบรรทัดนี้เพื่อใช้ Text

public class PlayerInventory : MonoBehaviour
{
    [Header("UI กระเป๋า (Elden Ring Style)")]
    public Image itemSlotUI;
    public Sprite emptySlotSprite;
    public TextMeshProUGUI countText; // 🌟 ช่องสำหรับลาก Text ตัวเลขมาใส่
    public Image cooldownImage;       // 🌟 ช่องสำหรับลากแผ่นวงกลมสีดำมาใส่
    public Image itemIconImage;


    [Header("ของที่มีในกระเป๋า")]
    public List<ItemData> inventoryList = new List<ItemData>();
    private int currentIndex = 0;

    private float currentCooldown = 0f; // 🌟 เวลานับถอยหลังคูลดาวน์
    private float maxCooldown = 0f;     // 🌟 เวลาคูลดาวน์เต็มๆ ของไอเทมชิ้นนั้น

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        // 🔄 กดปุ่ม E เพื่อ "หมุนเปลี่ยนไอเทม"
        if (Input.GetKeyDown(KeyCode.R) && currentCooldown <= 0) // 🌟 หมุนได้เฉพาะตอนที่ไม่ได้ติดคูลดาวน์
        {
            CycleItem();
        }

        // 💥 กดปุ่ม F เพื่อ "ใช้งานไอเทม" (เปลี่ยนปุ่มตามชอบ)
        if (Input.GetKeyDown(KeyCode.F))
        {
            UseCurrentItem();
        }

        // ⏳ ระบบจัดการ Cooldown แถบวงกลม
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;

            if (cooldownImage != null)
            {
                cooldownImage.fillAmount = currentCooldown / maxCooldown;
            }
        }
        else if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0; // ล้างแถบดำออกเมื่อคูลดาวน์เสร็จ
        }
    }

    void CycleItem()
    {
        if (inventoryList.Count == 0) return;

        currentIndex++;

        if (currentIndex >= inventoryList.Count)
        {
            currentIndex = 0;
        }

        UpdateUI();
        Debug.Log("หมุนมาที่ไอเทม: " + inventoryList[currentIndex].itemName);
    }

    void UseCurrentItem()
    {
        if (inventoryList.Count == 0 || currentCooldown > 0) return; // 🌟 ถ้ากระเป๋าว่าง หรือติดคูลดาวน์อยู่ ห้ามใช้!

        ItemData itemToUse = inventoryList[currentIndex];

        // ----------------------------------------------------
        // 🩸 1. ยาเพิ่มเลือด
        if (itemToUse.itemName == "HealPotion")
        {
            PlayerHealth health = GetComponent<PlayerHealth>();

            if (health != null)
            {
                if (health.currentHealth >= health.maxHealth)
                {
                    Debug.Log("เลือดเต็มอยู่แล้ว!");
                    return;
                }

                health.Heal(1);
                Debug.Log("ซดยาเพิ่มเลือดเรียบร้อย!");
                StartCooldown(itemToUse.cooldownTime); // 🌟 เริ่มนับคูลดาวน์
            }
        }
        // 💧 2. ยาเพิ่มมานา
        else if (itemToUse.itemName == "ManaPotion")
        {
            PlayerMana mana = GetComponent<PlayerMana>();

            if (mana != null)
            {
                if (mana.currentMana >= mana.maxMana)
                {
                    Debug.Log("มานาเต็มอยู่แล้ว เก็บไว้ก่อน!");
                    return;
                }

                mana.AddMana(1); // 👈 เติมมานา 1 ดวง (ถ้าอยากให้ยาระดับสูงเติม 2 ดวงก็เปลี่ยนเลขได้)
                Debug.Log("เติมมานา 1 ดวง!");
                StartCooldown(itemToUse.cooldownTime);
            }
        }

        // 🌟 ลดจำนวนไอเทมลง 1 ชิ้น
        itemToUse.amount--;

        // 🌟 ถ้าใช้จนหมดเกลี้ยง (เหลือ 0) ค่อยลบออกจากกระเป๋า
        if (itemToUse.amount <= 0)
        {
            inventoryList.RemoveAt(currentIndex);

            if (currentIndex >= inventoryList.Count)
            {
                currentIndex = 0;
            }
        }

        UpdateUI();
    }

    // 🌟 ฟังก์ชันเปิดระบบคูลดาวน์
    void StartCooldown(float time)
    {
        maxCooldown = time;
        currentCooldown = time;
    }

    public void AddItem(ItemData newItem)
    {
        // 🌟 ระบบซ้อนไอเทม (Stacking)
        // เช็กก่อนว่ามีไอเทมชื่อนี้อยู่ในกระเป๋าหรือยัง
        ItemData existingItem = inventoryList.Find(x => x.itemName == newItem.itemName);

        if (existingItem != null)
        {
            // ถ้ามีแล้ว ให้บวกจำนวนเพิ่มเข้าไป
            existingItem.amount += newItem.amount;
        }
        else
        {
            // ถ้ายังไม่มี ค่อยสร้างช่องใหม่
            ItemData clonedItem = new ItemData
            {
                itemName = newItem.itemName,
                itemIcon = newItem.itemIcon,
                amount = newItem.amount,
                cooldownTime = newItem.cooldownTime
            };
            inventoryList.Add(clonedItem);
        }

        UpdateUI();
        Debug.Log("เก็บได้: " + newItem.itemName);
    }

    void UpdateUI()
    {
        if (itemSlotUI == null) return;

        if (inventoryList.Count > 0)
        {
            ItemData currentItem = inventoryList[currentIndex];

            // 🌟 สั่งเปิดรูปขวดยาและอัปเดตรูป
            if (itemIconImage != null)
            {
                itemIconImage.sprite = currentItem.itemIcon;
                itemIconImage.enabled = true;
            }

            if (countText != null)
            {
                countText.text = currentItem.amount.ToString();
                countText.enabled = true; // โชว์ตัวเลข
            }
        }
        else
        {
            // 🌟 กรณีไม่มีของ: ให้ "ปิด" รูปขวดยาและตัวเลขไปเลย (เหลือแต่กรอบดำเปล่าๆ)
            if (itemIconImage != null) itemIconImage.enabled = false;
            if (countText != null) countText.enabled = false;
        }
    }

    
}
