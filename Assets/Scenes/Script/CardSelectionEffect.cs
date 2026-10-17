using UnityEngine;
using UnityEngine.EventSystems; // จำเป็นสำหรับระบบเลือก UI ด้วยคีย์บอร์ด/จอย
using UnityEngine.UI;
using System.Collections;

public class CardSelectionEffect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Image cardImage;
    private Vector3 originalScale;

    // ตั้งค่าความมืด (ยิ่งตัวเลขน้อย ยิ่งมืด 0 = ดำสนิท, 1 = สว่างปกติ)
    private Color dimColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    void Awake()
    {
        cardImage = GetComponent<Image>();
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        // 🌟 ตอนเปิดหน้าต่างขึ้นมา บังคับให้การ์ดทุกใบ "มืดและขนาดปกติ" ไว้ก่อน
        if (cardImage != null) cardImage.color = dimColor;
        transform.localScale = originalScale;
    }

    // 🎮 เมื่อกดลูกศรคีย์บอร์ด/จอย เลื่อนมาที่การ์ดใบนี้ (ถูกเลือก)
    public void OnSelect(BaseEventData eventData)
    {
        if (cardImage != null) cardImage.color = Color.white; // สว่างขึ้น
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale * 1.15f)); // ขยายขนาดขึ้น 15% ให้เด้งออกมาชัดๆ
    }

    // 🎮 เมื่อกดลูกศรคีย์บอร์ด/จอย เลื่อนหนีไปการ์ดใบอื่น (เลิกเลือก)
    public void OnDeselect(BaseEventData eventData)
    {
        if (cardImage != null) cardImage.color = dimColor; // มืดลง
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale)); // หดกลับขนาดเดิม
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        float t = 0;
        Vector3 startScale = transform.localScale;
        while (t < 1f)
        {
            // ใช้ unscaledDeltaTime เพราะเกมถูกหยุดเวลาอยู่ (Time.timeScale = 0)
            t += Time.unscaledDeltaTime * 15f;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}