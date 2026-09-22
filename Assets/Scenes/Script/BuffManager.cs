using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class BuffCard
{
    public string buffName; // ให้ตั้งชื่อเป็น "Fire Mastery", "Water Mastery", "Wind Mastery"
    public Sprite cardSprite;
}

public class BuffManager : MonoBehaviour
{
    public static BuffManager instance;

    [Header("UI ตั้งค่า")]
    public GameObject buffPanel;
    public Button[] buffButtons;
    public TextMeshProUGUI[] buffTexts;
    public TextMeshProUGUI currentBuffStatusText;

    [Header("🎵 เสียงประกอบ")]
    public AudioSource uiAudioSource;
    public AudioClip popupSound;
    public AudioClip clickSound;

    [Header("🃏 คลังการ์ดทั้งหมด (ตั้งค่าใน Inspector)")]
    public List<BuffCard> allAvailableCards;
    private List<BuffCard> currentOptions = new List<BuffCard>();

    // ==========================================
    // 🌟 ระบบเลเวลธาตุ (Elemental Mastery)
    // ==========================================
    [Header("เลเวลธาตุปัจจุบัน")]
    public int fireLevel = 0;
    public int waterLevel = 0;
    public int windLevel = 0;
    public int maxLevelCap = 5; // 🛑 เพดานสูงสุด

    void Awake()
    {
        instance = this;
        if (buffPanel != null) buffPanel.SetActive(false);
        UpdateBuffUI();

        for (int i = 0; i < buffButtons.Length; i++)
        {
            int index = i;
            if (buffButtons[i] != null)
            {
                buffButtons[i].onClick.RemoveAllListeners();
                buffButtons[i].onClick.AddListener(() => SelectBuff(index));
            }
        }
    }

    public void ShowBuffSelection()
    {
        Time.timeScale = 0f;
        buffPanel.SetActive(true);
        currentOptions.Clear();

        if (uiAudioSource != null && popupSound != null) uiAudioSource.PlayOneShot(popupSound);

        List<BuffCard> tempCards = new List<BuffCard>(allAvailableCards);
        for (int i = 0; i < 3; i++)
        {
            if (tempCards.Count == 0) break;
            int randIndex = Random.Range(0, tempCards.Count);
            BuffCard selectedCard = tempCards[randIndex];

            currentOptions.Add(selectedCard);
            if (buffTexts[i] != null) buffTexts[i].text = selectedCard.buffName;

            if (buffButtons[i] != null && selectedCard.cardSprite != null)
            {
                Image btnImage = buffButtons[i].GetComponent<Image>();
                if (btnImage != null) btnImage.sprite = selectedCard.cardSprite;
            }

            tempCards.RemoveAt(randIndex);
        }

        // บังคับจอยสติ๊กให้โฟกัสที่การ์ดใบแรก
        if (buffButtons.Length > 0 && buffButtons[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buffButtons[0].gameObject);
        }
    }

    public void SelectBuff(int buttonIndex)
    {
        if (uiAudioSource != null && clickSound != null) uiAudioSource.PlayOneShot(clickSound);

        string chosenBuffName = currentOptions[buttonIndex].buffName;

        // 🌟 เพิ่มเลเวลธาตุตามการ์ดที่เลือก (ติดเพดานที่กำหนด)
        if (chosenBuffName.Contains("Fire") && fireLevel < maxLevelCap) fireLevel++;
        else if (chosenBuffName.Contains("Water") && waterLevel < maxLevelCap) waterLevel++;
        else if (chosenBuffName.Contains("Wind") && windLevel < maxLevelCap) windLevel++;

        UpdateBuffUI();

        buffPanel.SetActive(false);
        Time.timeScale = 1f;

        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null) waveManager.ResumeWave();
    }

    private void UpdateBuffUI()
    {
        if (currentBuffStatusText == null) return;

        // 🌟 โชว์สถานะเลเวลธาตุบนหน้าจอ
        currentBuffStatusText.text = $"Elemental Mastery:\n" +
                                     $"🔥 Fire Lv. {fireLevel}/{maxLevelCap}\n" +
                                     $"💧 Water Lv. {waterLevel}/{maxLevelCap}\n" +
                                     $"🌪️ Wind Lv. {windLevel}/{maxLevelCap}";
    }

    // ฟังก์ชันนี้เรียกใช้ตอนผู้เล่นตาย หรือเริ่มรันใหม่ (สไตล์ Roguelite)
    public void ResetAllBuffs()
    {
        fireLevel = 0;
        waterLevel = 0;
        windLevel = 0;
        UpdateBuffUI();
    }

    // (เอาออก เพราะบัฟเราจะอยู่ถาวรตลอดการรัน ไม่ลบรายเวฟแล้ว)
    public void OnWaveEnded() { /* ปล่อยว่างไว้ให้ WaveManager เรียกเฉยๆ ไม่พังครับ */ }
}