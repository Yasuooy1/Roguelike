using UnityEngine;

public class MerchantInteract : MonoBehaviour
{
    [Header("ปุ่ม F ลอยบนหัว (Interact UI)")]
    public GameObject interactUI;

    private bool canInteract = false;
    private NPCShop shop; // 🌟 1. ประกาศตัวแปรเพื่อดึงสคริปต์ NPCShop มาใช้

    void Start()
    {
        if (interactUI != null) interactUI.SetActive(false);
        shop = GetComponent<NPCShop>(); // 🌟 2. ดึงสคริปต์ NPCShop ที่แปะอยู่บนพ่อค้าตัวเดียวกันมาเตรียมไว้
    }

    void Update()
    {
        // ถ้าผู้เล่นอยู่ในระยะ และกดปุ่ม F
        if (canInteract && Input.GetKeyDown(KeyCode.F))
        {
            OpenShop();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            if (interactUI != null) interactUI.SetActive(true); // โชว์ปุ่ม F
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            if (interactUI != null) interactUI.SetActive(false); // ซ่อนปุ่ม F

            // 🌟 3. ถ้าเดินหนีออกจากระยะวงกลม ให้สั่งปิดหน้าร้านค้าอัตโนมัติ
            if (shop != null) shop.CloseShop();
        }
    }

    void OpenShop()
    {
        if (shop != null)
        {
            shop.ToggleShop(); // 🌟 4. เรียกฟังก์ชันเปิดหน้า UI ของสคริปต์ NPCShop

            // ซ่อนปุ่ม F ไว้ชั่วคราวตอนที่หน้าต่างร้านค้าเด้งขึ้นมา
            if (interactUI != null && shop.shopCanvas.activeSelf)
            {
                interactUI.SetActive(false);
            }
        }
    }
}