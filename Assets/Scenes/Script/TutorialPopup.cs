using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    [Header("หน้าต่าง UI หลัก (ตัวพื้นหลัง)")]
    public GameObject tutorialCanvas; // ลากตัวแผง UI หลักมาใส่ตรงนี้

    [Header("หน้ากระดาษสอนเล่น (ลากมาใส่เรียงตามลำดับ)")]
    public GameObject[] tutorialPages; // ลาก Page 1, Page 2, Page 3 มาใส่
    private int currentPage = 0;

    void Start()
    {
        // เริ่มฉากมาปุ๊บ สั่งเปิด Tutorial และหยุดเวลาทันที!
        if (tutorialPages.Length > 0)
        {
            ShowTutorial();
        }
    }

    public void ShowTutorial()
    {
        tutorialCanvas.SetActive(true);
        Time.timeScale = 0f; // 🌟 ท่าไม้ตาย: หยุดเวลาในเกม! (มอนสเตอร์จะหยุดเกิดชั่วคราว)
        currentPage = 0;
        UpdatePageDisplay();
    }

    // ฟังก์ชันนี้เอาไว้ผูกกับปุ่ม "หน้าต่อไป" (Next Button)
    public void NextPage()
    {
        currentPage++;

        // ถ้ากดจนหมดทุกหน้าแล้ว ให้ปิดป๊อปอัปและเริ่มเกม
        if (currentPage >= tutorialPages.Length)
        {
            CloseTutorial();
        }
        else
        {
            UpdatePageDisplay(); // ถ้ายังไม่หมด ก็โชว์หน้าต่อไป
        }
    }

    void UpdatePageDisplay()
    {
        // สั่งปิดทุกหน้า แล้วเปิดแค่หน้าที่เรากำลังดูอยู่
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            tutorialPages[i].SetActive(i == currentPage);
        }
    }

    public void CloseTutorial()
    {
        tutorialCanvas.SetActive(false); // ปิดหน้าต่าง
        Time.timeScale = 1f; // 🌟 ท่าไม้ตาย: เดินเวลาต่อ! (มอนสเตอร์จะเริ่มเกิดแล้ว)
    }
}