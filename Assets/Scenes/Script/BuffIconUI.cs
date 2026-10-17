using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffIconUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI waveText; // ตัวเลขบอกจำนวนเวฟ

    private bool isBlinking = false;
    void Awake()
    {
        // สั่งให้มันหา Image ในตัวมันเองอัตโนมัติ
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }

        // สั่งให้มันหา Text (TMP) ที่ซ่อนอยู่ข้างในอัตโนมัติ
        if (waveText == null)
        {
            waveText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void Setup(ActiveBuff buff)
    {
        iconImage.sprite = buff.buffIcon;

        if (buff.isPermanent)
        {
            waveText.text = "∞"; // ถ้าเป็นบัฟถาวร (บอส) ให้โชว์เครื่องหมายอนันต์
            isBlinking = false;
        }
        else
        {
            waveText.text = buff.remainingWaves.ToString();

            // ถ้าเหลือ 1 เวฟสุดท้าย ให้เริ่มกระพริบ
            isBlinking = (buff.remainingWaves <= 1);
        }
    }

    void Update()
    {
        if (isBlinking)
        {
            // สลับความโปร่งใส (Alpha) ไปมาระหว่าง 0.3 กับ 1.0 เพื่อให้ดูกระพริบ
            float alpha = Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.time * 5f, 1f));
            iconImage.color = new Color(1f, 1f, 1f, alpha);
            waveText.color = new Color(waveText.color.r, waveText.color.g, waveText.color.b, alpha);
        }
    }
}