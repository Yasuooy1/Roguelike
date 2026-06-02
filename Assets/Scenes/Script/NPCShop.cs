using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 🌟 1. เพิ่มบรรทัดนี้เพื่อคุม UI ด้วยจอย

public class NPCShop : MonoBehaviour
{
    [Header("Shop UI")]
    public GameObject shopCanvas;
    public Text messageText;
    public GameObject firstShopButton; // 🌟 2. ลากปุ่ม "ซื้อยา" อันแรกมาใส่ช่องนี้

    [Header("Item To Sell")]
    public string potionName = "HealPotion";
    public Sprite potionIcon;
    public int potionPrice = 10;
    public float potionCooldown = 2f;

    private PlayerInventory playerInventory;
    [Header("Upgrade Skills")]
    public int fireUpgradePrice = 100;

    void Start()
    {
        if (shopCanvas != null) shopCanvas.SetActive(false);
    }

    // 🌟 3. ลบ Update() ทิ้งไปเลยครับ เพราะเราจะเรียก ToggleShop จากตัวผู้เล่นแทน

    // 🌟 4. เปลี่ยนเป็น public เพื่อให้ Player เรียกใช้ได้
    public void ToggleShop()
    {
        if (shopCanvas != null)
        {
            bool isActive = !shopCanvas.activeSelf;
            shopCanvas.SetActive(isActive);

            // ถ้าเปิดร้านค้า ให้หยุดเวลา และล็อกเป้าจอยไปที่ปุ่มแรก
            Time.timeScale = isActive ? 0f : 1f;

            if (isActive)
            {
                // บังคับจอยสติ๊กให้โฟกัสที่ปุ่มซื้อยาอันแรกทันที
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstShopButton);

                if (messageText != null)
                {
                    int currentSouls = PlayerPrefs.GetInt("Player_Souls", 0);
                    messageText.text = "ยินดีต้อนรับ! ตอนนี้คุณมี " + currentSouls + " วิญญาณ\nต้องการซื้อยาแดง (ราคา " + potionPrice + " Souls) ไหม?";
                }
            }
        }
    }

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
            if (messageText != null) messageText.text = "ซื้อสำเร็จ! เหลือเงิน: " + remainingSouls + " วิญญาณ";
        }
        else if (messageText != null)
        {
            messageText.text = "เงินไม่พอนะ!";
        }
    }

    public void CloseShop()
    {
        if (shopCanvas != null) shopCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    // 🌟 5. เมื่อเดินเข้าใกล้ NPC จะส่งตัวเราเองไปให้ Player รู้จัก
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInventory = collision.GetComponent<PlayerInventory>();
            if (playerInventory != null) playerInventory.currentNearbyShop = this; // ส่งร้านนี้ไปให้กระเป๋า
            Debug.Log("กด D-pad ล่าง เพื่อเปิดร้านค้า");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerInventory != null) playerInventory.currentNearbyShop = null; // เดินออกก็ลบค่าทิ้ง
            playerInventory = null;
            CloseShop();
        }
    }
}