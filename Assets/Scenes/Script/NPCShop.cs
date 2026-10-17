using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // คุม UI ด้วยจอยสติ๊ก/คีย์บอร์ด

public class NPCShop : MonoBehaviour
{
    [Header("Shop UI")]
    public GameObject shopCanvas;
    public Text messageText;
    public GameObject firstShopButton; // ลากปุ่มแรก (เช่น ซื้อยาเลือด) มาใส่ช่องนี้

    [Header("Item To Sell (ยาเลือด)")]
    public string potionName = "HealPotion";
    public Sprite potionIcon;
    public int potionPrice = 10;
    public float potionCooldown = 2f;

    [Header("Item To Sell (ยามานา)")]
    public string manaPotionName = "ManaPotion";
    public Sprite manaPotionIcon;
    public int manaPotionPrice = 15;
    public float manaPotionCooldown = 2f;

    private PlayerInventory playerInventory;

    [Header("Upgrade Skills")]
    public int fireUpgradePrice = 100;

    void Start()
    {
        if (shopCanvas != null) shopCanvas.SetActive(false);
    }

    // ฟังก์ชันเปิด/ปิดหน้าร้านค้า
    public void ToggleShop()
    {
        if (shopCanvas != null)
        {
            bool isActive = !shopCanvas.activeSelf;
            shopCanvas.SetActive(isActive);

            // หยุดเวลาตอนเปิดร้าน
            Time.timeScale = isActive ? 0f : 1f;

            if (isActive)
            {
                // ล็อกเป้าให้จอยสติ๊กไปที่ปุ่มแรก
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstShopButton);

                if (messageText != null)
                {
                    int currentSouls = PlayerPrefs.GetInt("Player_Souls", 0);
                    messageText.text = "ยินดีต้อนรับ! ตอนนี้คุณมี " + currentSouls + " วิญญาณ\nต้องการซื้ออะไรดี?";
                }
            }
        }
    }

    // ฟังก์ชันซื้อยาเลือด
    public void BuyPotion()
    {
        int currentSouls = PlayerPrefs.GetInt("Player_Souls", 0);
        if (currentSouls >= potionPrice && playerInventory != null)
        {
            PlayerPrefs.SetInt("Player_Souls", currentSouls - potionPrice);
            PlayerPrefs.Save();

            ItemData boughtItem = new ItemData();
            boughtItem.itemName = potionName;
            boughtItem.itemIcon = potionIcon;
            boughtItem.amount = 1;
            boughtItem.cooldownTime = potionCooldown;

            playerInventory.AddItem(boughtItem);

            int remainingSouls = PlayerPrefs.GetInt("Player_Souls", 0);
            if (messageText != null) messageText.text = "ซื้อยาเลือดสำเร็จ! เหลือเงิน: " + remainingSouls + " Souls";
        }
        else if (messageText != null)
        {
            messageText.text = "เงินไม่พอนะ!";
        }
    }

    // ฟังก์ชันซื้อยามานา
    public void BuyManaPotion()
    {
        int currentSouls = PlayerPrefs.GetInt("Player_Souls", 0);
        if (currentSouls >= manaPotionPrice && playerInventory != null)
        {
            PlayerPrefs.SetInt("Player_Souls", currentSouls - manaPotionPrice);
            PlayerPrefs.Save();

            ItemData boughtItem = new ItemData();
            boughtItem.itemName = manaPotionName;
            boughtItem.itemIcon = manaPotionIcon;
            boughtItem.amount = 1;
            boughtItem.cooldownTime = manaPotionCooldown;

            playerInventory.AddItem(boughtItem);

            int remainingSouls = PlayerPrefs.GetInt("Player_Souls", 0);
            if (messageText != null) messageText.text = "ซื้อยามานาสำเร็จ! เหลือเงิน: " + remainingSouls + " Souls";
        }
        else if (messageText != null)
        {
            messageText.text = "เงินไม่พอนะ!";
        }
    }

    // ฟังก์ชันปิดหน้าร้านค้า
    public void CloseShop()
    {
        if (shopCanvas != null) shopCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    // ตรวจจับผู้เล่นเดินเข้าใกล้
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInventory = collision.GetComponent<PlayerInventory>();
            if (playerInventory != null) playerInventory.currentNearbyShop = this;
        }
    }

    // ตรวจจับผู้เล่นเดินออกห่าง
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerInventory != null) playerInventory.currentNearbyShop = null;
            playerInventory = null;
            CloseShop(); // ปิดร้านค้าอัตโนมัติเมื่อเดินหนี
        }
    }
}