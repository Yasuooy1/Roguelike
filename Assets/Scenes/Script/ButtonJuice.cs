using UnityEngine;
using UnityEngine.EventSystems; // จำเป็นสำหรับคีย์บอร์ดและจอย
using UnityEngine.UI;
using System.Collections; // สำหรับทำ Coroutine ปุ่มเด้ง

public class ButtonJuice : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    private Vector3 originalScale;
    public AudioSource audioSource;

    [Header("Sounds")]
    public AudioClip hoverSound; // เสียงตอนเลื่อนไปทับ/เลือกปุ่ม
    public AudioClip clickSound; // เสียงตอนกดปุ่ม (Enter/Space)

    [Header("Visual Effects")]
    public GameObject sparkleEffect; // เอฟเฟกต์รูปวงเวทจักรราศี ที่จะให้โชว์

    void Start()
    {
        originalScale = transform.localScale;

        // ปิดเอฟเฟกต์วงเวทไว้ก่อนตอนเริ่ม
        if (sparkleEffect != null) sparkleEffect.SetActive(false);
    }

    // ==========================================
    // 1. ตอนกดปุ่ม (คีย์บอร์ดกด Enter, Spacebar หรือ จอยสติ๊กกด A)
    // ==========================================
    public void OnSubmit(BaseEventData eventData)
    {
        ExecuteClickFeedback();
    }

    private void ExecuteClickFeedback()
    {
        // เล่นเสียงกด
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ButtonBounceRoutine());
        }
    }

    private IEnumerator ButtonBounceRoutine()
    {
        // ย่อขนาด 10%
        transform.localScale = originalScale * 0.9f;

        // รอแป๊บนึง (ใช้ WaitForSecondsRealtime เผื่อเกมถูก Pause อยู่เวลาตาย)
        yield return new WaitForSecondsRealtime(0.1f);

        // เด้งกลับขนาดเดิม
        transform.localScale = originalScale;
    }

    // ==========================================
    // 2. ตอนคีย์บอร์ดลูกศร / จอยสติ๊ก เลื่อนมาทับปุ่มนี้
    // ==========================================
    public void OnSelect(BaseEventData eventData)
    {
        ExecuteHoverFeedback();
    }

    private void ExecuteHoverFeedback()
    {
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }

        if (sparkleEffect != null)
        {
            // เปิดเอฟเฟกต์วงเวทให้แสดงขึ้นมา!
            sparkleEffect.SetActive(true);
        }
    }

    // ==========================================
    // 3. ตอนคีย์บอร์ดลูกศร / จอยสติ๊ก เลื่อนหนีออกจากปุ่มนี้
    // ==========================================
    public void OnDeselect(BaseEventData eventData)
    {
        ExecuteDeselectFeedback();
    }

    private void ExecuteDeselectFeedback()
    {
        if (sparkleEffect != null)
        {
            // ปิดวงเวททิ้งไป
            sparkleEffect.SetActive(false);
        }

        // เผื่อมีบั๊กกดค้างแล้วเลื่อนหนี ให้มันคืนขนาดเดิมเสมอ
        transform.localScale = originalScale;
    }
}