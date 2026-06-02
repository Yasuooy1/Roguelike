using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("ตั้งค่าป้ายสอนเล่น")]
    public GameObject tutorialText;
    public bool isLastTutorial = false; // 🌟 ติ๊กถูกเฉพาะกล่องสุดท้าย (Shoot)

    [Header("ตั้งค่าลูกศรชี้ถัดไป")]
    public int nextStepIndex; // 🌟 ใส่เลขลำดับถัดไป (เช่น กล่องแรกใส่ 1, กล่องสองใส่ 2)

    private bool arrowMoved = false; // กุญแจล็อก (ให้ลูกศรขยับแค่รอบเดียว)

    void Start()
    {
        // ซ่อนป้ายไว้ก่อนตอนเริ่มฉาก
        if (tutorialText != null) tutorialText.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. โชว์ป้ายข้อความ (โค้ดเดิมของคุณอาร์ม)
            if (tutorialText != null) tutorialText.SetActive(true);

            // 2. 🌟 ระบบเลื่อนลูกศร (จะทำงานแค่ "ครั้งแรก" ที่เดินมาเหยียบเท่านั้น)
            if (!arrowMoved)
            {
                arrowMoved = true; // ล็อกกุญแจทันที! ป้องกันลูกศรรันเบิ้ล

                HubRoomGuide guide = FindObjectOfType<HubRoomGuide>();
                if (guide != null)
                {
                    if (isLastTutorial)
                    {
                        // ถ้าติ๊กกล่องนี้เป็นอันสุดท้าย ให้ชี้ไป "หุ่นซ้อม (Dummy)"
                        guide.PointToDummy();
                    }
                    else
                    {
                        // ถ้ายังไม่จบ ให้ชี้ไปจุดที่ระบุไว้
                        guide.PointToSpecificStep(nextStepIndex);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // ซ่อนป้ายตอนเดินออก (โค้ดเดิม)
            if (tutorialText != null) tutorialText.SetActive(false);
        }
    }
}