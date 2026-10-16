using UnityEngine;
using UnityEngine.EventSystems; // 🌟 ต้องมีบรรทัดนี้ถึงจะสั่งปุ่มได้

public class AutoSelectUI : MonoBehaviour
{
    [Header("ลากปุ่มที่จะให้เลือกเป็นปุ่มแรกมาใส่")]
    public GameObject firstSelectedButton;

    // ทันทีที่หน้าต่างนี้ถูกเปิดขึ้นมา (SetActive(true))
    void OnEnable()
    {
        if (firstSelectedButton != null)
        {
            // ล้างการเลือกเก่าทิ้งก่อน (กันบั๊ก)
            EventSystem.current.SetSelectedGameObject(null);
            // บังคับไฮไลต์เป้าหมายไปที่ปุ่มที่เรากำหนดทันที
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }
}