using UnityEngine;
using UnityEngine.UI;

public class TutorialTrigger : MonoBehaviour
{
    [Header("หน้าต่าง UI หลัก (ตัวพื้นหลัง)")]
    public GameObject tutorialCanvas;

    [Header("หน้ากระดาษสอนเล่น (ลาก Page มาใส่เรียงตามลำดับ)")]
    public GameObject[] tutorialPages;
    private int currentPage = 0;

    [Header("ตั้งค่าลูกศรชี้ถัดไป")]
    public bool isLastTutorial = false; // ติ๊กถูกเฉพาะกล่องสุดท้าย
    public int nextStepIndex; // ใส่เลขลำดับถัดไป

    [Header("🌟 รหัสจำการดู (ตั้งชื่อห้ามซ้ำกันในแต่ละจุด)")]
    public string tutorialKey = "HasSeenTrigger_01";

    private bool arrowMoved = false;

    void Start()
    {
        // ปิดหน้าต่างไว้ก่อนตอนเริ่มฉาก
        if (tutorialCanvas != null) tutorialCanvas.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 🛑 1. เช็กความจำ: ถ้าค่ายังเป็น 0 (แปลว่ายังไม่เคยเหยียบจุดนี้)
            if (PlayerPrefs.GetInt(tutorialKey, 0) == 0)
            {
                ShowTutorial(); // เรียกฟังก์ชันโชว์ป๊อปอัปและหยุดเวลา
            }

            // 2. 🌟 ระบบเลื่อนลูกศร (จะทำงานแค่ "ครั้งแรก" ที่เดินมาเหยียบเท่านั้น)
            if (!arrowMoved)
            {
                arrowMoved = true; // ล็อกกุญแจทันที! ป้องกันลูกศรรันเบิ้ล

                HubRoomGuide guide = FindObjectOfType<HubRoomGuide>();
                if (guide != null)
                {
                    if (isLastTutorial) guide.PointToDummy();
                    else guide.PointToSpecificStep(nextStepIndex);
                }
            }
        }
    }

    public void ShowTutorial()
    {
        if (tutorialCanvas != null) tutorialCanvas.SetActive(true);
        Time.timeScale = 0f; // 🌟 หยุดเวลาเกมให้ผู้เล่นอ่าน
        currentPage = 0;
        UpdatePageDisplay();
    }

    // 🌟 เอาฟังก์ชันนี้ไปผูกกับปุ่ม "หน้าต่อไป (Next)" บนหน้า UI
    public void NextPage()
    {
        currentPage++;

        // ถ้ากดจนหมดทุกหน้าแล้ว ให้ปิดป๊อปอัปและเดินเวลาต่อ
        if (currentPage >= tutorialPages.Length)
        {
            CloseTutorial();
        }
        else
        {
            UpdatePageDisplay(); // ถ้ายังไม่หมด ก็เปลี่ยนไปหน้าถัดไป
        }
    }

    void UpdatePageDisplay()
    {
        // สั่งปิดทุกหน้า แล้วเปิดแค่หน้าที่เรากำลังดูอยู่
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            if (tutorialPages[i] != null)
            {
                tutorialPages[i].SetActive(i == currentPage);
            }
        }
    }

    public void CloseTutorial()
    {
        if (tutorialCanvas != null) tutorialCanvas.SetActive(false);
        Time.timeScale = 1f; // 🟢 กลับมาเดินเวลาต่อ! (ผู้เล่นขยับได้แล้ว)

        // 💾 บันทึกว่าผู้เล่นดูจบแล้ว รอบหน้าเดินมาเหยียบอีกจะได้ไม่เด้ง
        PlayerPrefs.SetInt(tutorialKey, 1);
        PlayerPrefs.Save();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // ป้องกันไว้เผื่อเดินออก
        if (collision.CompareTag("Player"))
        {
            if (tutorialCanvas != null) tutorialCanvas.SetActive(false);
        }
    }

    // 🛠️ ฟังก์ชันพิเศษสำหรับ Dev: เอาไว้กดล้างความจำตอนเทสต์เกม
    [ContextMenu("Reset Tutorial Memory")]
    public void ResetTutorialMemory()
    {
        PlayerPrefs.SetInt(tutorialKey, 0);
        PlayerPrefs.Save();
        Debug.Log("ล้างความจำจุด " + tutorialKey + " แล้ว! เดินไปเหยียบรอบหน้าจะเด้งใหม่");
    }
}