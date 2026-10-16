using UnityEngine;

public class UIPopup : MonoBehaviour
{
    [Header("ความเร็วในการเด้ง")]
    public float popSpeed = 12f;

    // 🌟 เพิ่มตัวแปรมาจำขนาดที่แท้จริง
    private Vector3 targetScale;

    void Awake()
    {
        // ให้มันจำขนาดที่เราดึงยืดไว้ในหน้าต่าง Unity ตอนเริ่มเกม
        targetScale = transform.localScale;
    }

    void OnEnable()
    {
        // บังคับให้ขนาดหดเหลือ 0 ทันทีที่เปิดหน้าต่าง
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        // ค่อยๆ ขยายร่างจาก 0 ไปหาขนาดจริง (targetScale) แทนที่จะเป็น 1 (Vector3.one)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * popSpeed);
    }
}