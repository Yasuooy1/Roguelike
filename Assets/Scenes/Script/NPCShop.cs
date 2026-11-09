using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NPCShop : MonoBehaviour
{
    [Header("Shop UI")]
    public GameObject shopCanvas;
    public Text messageText;
    public GameObject firstShopButton;

    [Header("ปุ่ม F ลอยบนหัว (Interact UI)")]
    public GameObject interactUI;

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
    private bool canInteract = false;
    private bool isShopOpen = false;

    void Start()
    {
        // 🌟 ค้นหา Canvas อัตโนมัติถ้าช่องยังว่าง
        if (shopCanvas == null)
        {
            Transform canvasTrans = transform.Find("Shop Canvas");
            if (canvasTrans == null) canvasTrans = transform.Find("Canvas");
            if (canvasTrans == null) canvasTrans = transform.Find("ShopPanel");

            if (canvasTrans != null) shopCanvas = canvasTrans.gameObject;
            else
            {
                Canvas foundCanvas = GetComponentInChildren<Canvas>(true);
                if (foundCanvas != null) shopCanvas = foundCanvas.gameObject;
            }
        }

        if (shopCanvas != null) shopCanvas.SetActive(false);
        if (interactUI != null) interactUI.SetActive(false);
    }

    void Update()
    {
        // ถ้าผู้เล่นอยู่ในระยะ และกดปุ่ม F
        if (canInteract && Input.GetKeyDown(KeyCode.F))
        {
            ToggleShop();
        }
    }

    // ฟังก์ชันเปิด/ปิดหน้าร้านค้า
    public void ToggleShop()
    {
        if (shopCanvas != null)
        {
            isShopOpen = !shopCanvas.activeSelf;
            shopCanvas.SetActive(isShopOpen);

            Time.timeScale = isShopOpen ? 0f : 1f;

            if (isShopOpen)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstShopButton);

                if (messageText != null)
                {
                    int currentSouls = PlayerPrefs.GetInt("Player_Souls", 0);
                    messageText.text = "ยินดีต้อนรับ! ตอนนี้คุณมี " + currentSouls + " วิญญาณ\nต้องการซื้ออะไรดี?";
                }

                if (interactUI != null) interactUI.SetActive(false); // ซ่อนปุ่ม F ตอนเปิดร้าน
            }
            else
            {
                if (interactUI != null && canInteract) interactUI.SetActive(true); // โชว์ปุ่ม F กลับมาถ้ายังยืนอยู่ใกล้
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
            if (messageText != null) messageText.text = "ซื้อยาเลือดสำเร็จ! เหลือเงิน: " + remainingSouls + " Souls";
        }
        else if (messageText != null)
        {
            messageText.text = "เงินไม่พอนะ!";
        }
    }

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

    public void CloseShop()
    {
        if (shopCanvas != null) shopCanvas.SetActive(false);
        Time.timeScale = 1f;
        isShopOpen = false;
        if (interactUI != null && canInteract) interactUI.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canInteract = true;
            playerInventory = collision.GetComponent<PlayerInventory>();
            if (playerInventory != null) playerInventory.currentNearbyShop = this;

            if (interactUI != null && !isShopOpen) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canInteract = false;
            if (interactUI != null) interactUI.SetActive(false);

            if (playerInventory != null) playerInventory.currentNearbyShop = null;
            playerInventory = null;
            CloseShop();
        }
    }
}