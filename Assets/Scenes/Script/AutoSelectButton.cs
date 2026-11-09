using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoSelectButton : MonoBehaviour
{
    [Header("ลากปุ่มแรกที่ต้องการให้เลือกมาใส่ตรงนี้")]
    public GameObject firstButton;

    private bool isFocused = false;

    void OnEnable()
    {
        // บังคับเลือกปุ่มทันทีเมื่อเปิดหน้านี้ขึ้นมา
        SelectFirstButton();
    }

    void Update()
    {
        // ถ้าเมาส์คลิกพื้นที่ว่างหรือกดปุ่มแล้วหลุดโฟกัส ให้ดึงโฟกัสกลับมาที่ปุ่มแรกเสมอ
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            SelectFirstButton();
        }
    }

    public void SelectFirstButton()
    {
        if (firstButton != null && firstButton.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }
    }
}