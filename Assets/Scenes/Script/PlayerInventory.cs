using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Header("UI กระเป๋า")]
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

    public Sprite healPotionIcon;
    public Sprite manaPotionIcon;

    [Header("🎵 ระบบเสียงไอเทม")]
    public AudioSource audioSource;
    public AudioClip drinkPotionSound;

    [Header("✨ ระบบแอนิเมชัน & ออร่า")]
    public Animator playerAnimator;
    public GameObject healthAura;
    public GameObject manaAura;

    public NPCShop currentNearbyShop;

    void Start()
    {
        LoadInventory();
        UpdateUI();
    }

    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            if (cooldownImage != null) cooldownImage.fillAmount = currentCooldown / maxCooldown;
        }
        else if (cooldownImage != null) cooldownImage.fillAmount = 0;
    }

    public void OnUseItem(InputValue value)
    {
        if (value.isPressed)
        {
            // 🌟 1. เช็กก่อนเลยว่ายืนอยู่ใกล้พ่อค้าไหม?
            if (currentNearbyShop != null)
            {
                // ถ้าอยู่ใกล้ ให้สั่ง "เปิดร้านค้า" แล้วยกเลิกการกินยาทันที!
                currentNearbyShop.ToggleShop();
                return;
            }

            // 🌟 2. ถ้าไม่ได้อยู่ใกล้พ่อค้า ให้เช็กว่าเกมหยุดอยู่ไหม (เปิดเมนู/ร้านค้าอยู่)
            if (Time.timeScale == 0f)
            {
                return; // ห้ามกินยาตอนเปิดเมนู
            }

            // 🌟 3. ถ้าไม่ได้เปิดเมนู และไม่ได้อยู่ใกล้พ่อค้า ก็กินยาตามปกติ!
            UseCurrentItem();
        }
    }

    public void OnSwitchItem(InputValue value)
    {
        if (currentNearbyShop != null || Time.timeScale == 0f)
        {
            return;
        }
        if (value.isPressed && currentCooldown <= 0) CycleItem();
    }

    // 🌟 ระบบคุยกับ NPC 
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
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

        PlayerController controller = FindObjectOfType<PlayerController>();

        if (itemToUse.itemName == "HealPotion")
        {

            if (BuffManager.instance != null && BuffManager.instance.HasBuff("Bloodthirst"))
            {
                Debug.Log("คำสาป Bloodthirst: คุณไม่สามารถใช้ยาฮีลได้อีกต่อไป!");
                return;
            }
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null && health.currentHealth < health.maxHealth)
            {
                if (playerAnimator != null) playerAnimator.SetTrigger("DrinkHealth");
                if (controller != null) controller.LockMovement(0.8f);
                StartCooldown(itemToUse.cooldownTime);
                ConsumeItem(itemToUse);
            }
        }
        else if (itemToUse.itemName == "ManaPotion")
        {
            PlayerMana mana = GetComponent<PlayerMana>();
            if (mana != null && mana.currentMana < mana.maxMana)
            {
                if (playerAnimator != null) playerAnimator.SetTrigger("DrinkMana");
                if (controller != null) controller.LockMovement(0.8f);
                StartCooldown(itemToUse.cooldownTime);
                ConsumeItem(itemToUse);
            }
        }
    }

    void ConsumeItem(ItemData itemToUse)
    {
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

    public void DrinkHealthPotionEvent()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null) health.Heal(1);

        if (audioSource != null && drinkPotionSound != null)
            audioSource.PlayOneShot(drinkPotionSound);

        if (healthAura != null)
        {
            healthAura.SetActive(false);
            healthAura.SetActive(true);
        }
    }

    public void DrinkManaPotionEvent()
    {
        PlayerMana mana = GetComponent<PlayerMana>();
        if (mana != null) mana.AddMana(1);

        if (audioSource != null && drinkPotionSound != null)
            audioSource.PlayOneShot(drinkPotionSound);

        if (manaAura != null)
        {
            manaAura.SetActive(false);
            manaAura.SetActive(true);
        }
    }

    // 🌟 ระบบเพิ่มไอเทม (ที่ทำให้เกิด Error เมื่อกี้ นำกลับมาใส่ให้แล้วครับ)
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
        if (healAmount > 0) inventoryList.Add(new ItemData { itemName = "HealPotion", itemIcon = healPotionIcon, amount = healAmount, cooldownTime = 2f });
        if (manaAmount > 0) inventoryList.Add(new ItemData { itemName = "ManaPotion", itemIcon = manaPotionIcon, amount = manaAmount, cooldownTime = 2f });
    }

    void UpdateUI()
    {
        if (itemSlotUI == null) return;
        if (inventoryList.Count > 0)
        {
            ItemData currentItem = inventoryList[currentIndex];
            if (itemIconImage != null) { itemIconImage.sprite = currentItem.itemIcon; itemIconImage.enabled = true; }
            if (countText != null) { countText.text = currentItem.amount.ToString(); countText.enabled = true; }
        }
        else
        {
            if (itemIconImage != null) itemIconImage.enabled = false;
            if (countText != null) countText.enabled = false;
        }
    }
}