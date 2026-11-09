using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopButtonHandler : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerDownHandler
{
    private Graphic buttonGraphic;
    private Color originalColor;

    [Header("Highlight Color (สีตอนเลือกปุ่มร้านค้า)")]
    public Color hoverColor = new Color(1f, 1f, 1f, 1f);

    [Header("Audio Settings (เสียงปุ่มร้านค้า)")]
    public AudioSource audioSource;
    public AudioClip hoverSound; // เสียงตอนเลื่อนมาเลือก/ชี้
    public AudioClip clickSound; // เสียงตอนกดซื้อ / กดปุ่ม

    void Start()
    {
        buttonGraphic = GetComponent<Graphic>();
        if (buttonGraphic != null)
        {
            originalColor = buttonGraphic.color;
        }

        // ถ้าไม่ได้ลาก AudioSource มา ให้ลองหาจากตัวปุ่มหรือเรฟกลางอัตโนมัติ
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void OnEnable()
    {
        if (buttonGraphic != null)
        {
            buttonGraphic.color = originalColor;
        }
    }

    // เมื่อคีย์บอร์ดหรือจอยเลื่อนมาเลือกปุ่มนี้ (รวมถึงตอนเปิดร้านแล้วปุ่มแรกถูกเลือก)
    public void OnSelect(BaseEventData eventData)
    {
        if (buttonGraphic != null)
        {
            buttonGraphic.color = hoverColor;
        }

        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    // เมื่อเลื่อนหนีออกจากปุ่มนี้
    public void OnDeselect(BaseEventData eventData)
    {
        if (buttonGraphic != null)
        {
            buttonGraphic.color = originalColor;
        }
    }

    // เมื่อกดปุ่มด้วยคีย์บอร์ด / จอย (Enter / Spacebar)
    public void OnSubmit(BaseEventData eventData)
    {
        PlayClickSound();
    }

    // เมื่อคลิกเมาส์ที่ปุ่มนี้
    public void OnPointerDown(PointerEventData eventData)
    {
        PlayClickSound();
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}