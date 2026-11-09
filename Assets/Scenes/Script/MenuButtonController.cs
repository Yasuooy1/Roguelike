using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MenuButtonController : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerDownHandler
{
    private Vector3 originalScale;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Focus Protection (กันหลุดโฟกัส)")]
    public static GameObject lastSelectedButton;

    void Start()
    {
        originalScale = transform.localScale;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // ถ้าเมาส์คลิกพื้นที่ว่างแล้วหลุดโฟกัส ให้ดึงกลับมาที่ปุ่มนี้ถ้ามันเคยถูกเลือกอยู่
        if (EventSystem.current.currentSelectedGameObject == null && lastSelectedButton != null && lastSelectedButton.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedButton);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        lastSelectedButton = gameObject;

        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        transform.localScale = originalScale;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        ExecuteClick();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastSelectedButton = gameObject;
        EventSystem.current.SetSelectedGameObject(gameObject);
        ExecuteClick();
    }

    private void ExecuteClick()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        if (gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(BounceRoutine());
        }
    }

    private IEnumerator BounceRoutine()
    {
        transform.localScale = originalScale * 0.9f;
        yield return new WaitForSecondsRealtime(0.1f);
        transform.localScale = originalScale;
    }
}