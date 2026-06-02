using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem; // 🌟 1. อย่าลืมบรรทัดนี้เด็ดขาด!

public class PlayerInventory : MonoBehaviour
{
    [Header("UI กระเป๋า (Elden Ring Style)")]
    public Image itemSlotUI;
    public Sprite emptySlotSprite;
    public TextMeshProUGUI countText;
    public Image cooldownImage;
    public Image itemIconImage;

    [Header("ของที่มีในกระเป๋า")]
    public List<ItemData> inventoryList = new List<ItemData>();
    private int currentIndex = 0;

    private float currentCooldown = 0f;
    private float maxCooldown = 0f;

    [Header("Item Sprites (สำหรับระบบ Load)")]
    public Sprite healPotionIcon;
    public Sprite manaPotionIcon;
    [Header("🎵 ระบบเสียงไอเทม")]
    public AudioSource audioSource;   // ลาก Player หรือกล่องเสียงมาใส่
    public AudioClip drinkPotionSound;

    void Start()
    {
        LoadInventory();
        UpdateUI();
    }

    void Update()
    {
        // ❌ เอา Input.GetKeyDown(KeyCode.R) และ F ออกไปแล้วครับ
        // ให้เหลือแค่ตัวนับเวลา Cooldown UI พอ
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            if (cooldownImage != null) cooldownImage.fillAmount = currentCooldown / maxCooldown;
        }
        else if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0;
        }
    }

    // ==========================================
    // 🌟 2. รับค่าจากจอยสติ๊ก (D-pad) และคีย์บอร์ด (F, R)
    // ==========================================

    // กดยา (ปุ่ม F หรือ D-Pad บน)
    public void OnUseItem(InputValue value)
    {
        if (value.isPressed)
        {
            UseCurrentItem();
        }
    }

    // สลับยา (ปุ่ม R หรือ D-Pad ซ้าย )
    public void OnSwitchItem(InputValue value)
    {
        if (value.isPressed && currentCooldown <= 0)
        {
            CycleItem();
        }
    }

    // ==========================================

    void CycleItem()
    {
        if (inventoryList.Count == 0) return;
        currentIndex++;
        if (currentIndex >= inventoryList.Count) currentIndex = 0;
        UpdateUI();
    }

    void UseCurrentItem()
    {
        if (inventoryList.Count == 0 || currentCooldown > 0) return;

        ItemData itemToUse = inventoryList[currentIndex];

        if (itemToUse.itemName == "HealPotion")
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null)
            {
                if (health.currentHealth >= health.maxHealth) return;
                health.Heal(1);
                StartCooldown(itemToUse.cooldownTime);
                // 🌟 แทรกเสียงกินยาตรงนี้! (หลังจากฮีลสำเร็จ)
                if (audioSource != null && drinkPotionSound != null)
                {
                    audioSource.PlayOneShot(drinkPotionSound);
                }
            }
        }
        else if (itemToUse.itemName == "ManaPotion")
        {
            PlayerMana mana = GetComponent<PlayerMana>();
            if (mana != null)
            {
                if (mana.currentMana >= mana.maxMana) return;
                mana.AddMana(1);
                StartCooldown(itemToUse.cooldownTime);
                
                if (audioSource != null && drinkPotionSound != null)
                {
                    audioSource.PlayOneShot(drinkPotionSound);
                }
            }
        }

        itemToUse.amount--;
        SaveInventory();

        if (itemToUse.amount <= 0)
        {
            inventoryList.RemoveAt(currentIndex);
            if (currentIndex >= inventoryList.Count) currentIndex = 0;
        }

        UpdateUI();
    }

    void StartCooldown(float time)
    {
        maxCooldown = time;
        currentCooldown = time;
    }

    public void AddItem(ItemData newItem)
    {
        ItemData existingItem = inventoryList.Find(x => x.itemName == newItem.itemName);

        if (existingItem != null)
        {
            existingItem.amount += newItem.amount;
        }
        else
        {
            ItemData clonedItem = new ItemData
            {
                itemName = newItem.itemName,
                itemIcon = newItem.itemIcon,
                amount = newItem.amount,
                cooldownTime = newItem.cooldownTime
            };
            inventoryList.Add(clonedItem);
        }

        SaveInventory();
        UpdateUI();
    }

    void UpdateUI()
    {
        if (itemSlotUI == null) return;

        if (inventoryList.Count > 0)
        {
            ItemData currentItem = inventoryList[currentIndex];
            if (itemIconImage != null)
            {
                itemIconImage.sprite = currentItem.itemIcon;
                itemIconImage.enabled = true;
            }
            if (countText != null)
            {
                countText.text = currentItem.amount.ToString();
                countText.enabled = true;
            }
        }
        else
        {
            if (itemIconImage != null) itemIconImage.enabled = false;
            if (countText != null) countText.enabled = false;
        }
    }

    public void SaveInventory()
    {
        ItemData heal = inventoryList.Find(x => x.itemName == "HealPotion");
        ItemData mana = inventoryList.Find(x => x.itemName == "ManaPotion");

        PlayerPrefs.SetInt("Inv_HealPotion", heal != null ? heal.amount : 0);
        PlayerPrefs.SetInt("Inv_ManaPotion", mana != null ? mana.amount : 0);

        PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        inventoryList.Clear();

        int healAmount = PlayerPrefs.GetInt("Inv_HealPotion", 0);
        int manaAmount = PlayerPrefs.GetInt("Inv_ManaPotion", 0);

        if (healAmount > 0)
        {
            inventoryList.Add(new ItemData
            {
                itemName = "HealPotion",
                itemIcon = healPotionIcon,
                amount = healAmount,
                cooldownTime = 2f
            });
        }

        if (manaAmount > 0)
        {
            inventoryList.Add(new ItemData
            {
                itemName = "ManaPotion",
                itemIcon = manaPotionIcon,
                amount = manaAmount,
                cooldownTime = 2f
            });
        }
    }
    // 🌟 เพิ่มตัวแปรนี้ไว้ด้านบนสุดของคลาส PlayerInventory
    public NPCShop currentNearbyShop;

    // 🌟 แก้ไขฟังก์ชัน OnInteract (เอาไปวางต่อจาก OnSwitchItem)
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            // ถ้าอยู่ใกล้ร้านค้า ให้สั่งเปิดร้านค้านั้นๆ
            if (currentNearbyShop != null)
            {
                currentNearbyShop.ToggleShop();
            }
            else
            {
                Debug.Log("กด Interact แต่ไม่มีอะไรให้คุยแถวนี้");
            }
        }
    }
}