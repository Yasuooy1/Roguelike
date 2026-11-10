using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffIconUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI stackText;

    public void Setup(ActiveBuff buff)
    {
        // 🌟 1. ระบบหา Component อัตโนมัติ (ไม่ต้องลากใส่ Inspector เองแล้ว!)
        if (iconImage == null)
            iconImage = GetComponent<Image>();

        if (stackText == null)
            stackText = GetComponentInChildren<TextMeshProUGUI>(true);

        // 🌟 2. เซ็ตภาพการ์ด
        if (iconImage != null)
        {
            iconImage.sprite = buff.buffIcon;
        }

        // 🌟 3. เซ็ตตัวเลข Stack (x2, x3)
        if (stackText != null)
        {
            if (buff.stackCount > 1)
            {
                stackText.gameObject.SetActive(true);
                stackText.text = "x" + buff.stackCount;
            }
            else
            {
                stackText.gameObject.SetActive(false); // มีอันเดียวซ่อนเลขไว้
            }
        }
    }
}