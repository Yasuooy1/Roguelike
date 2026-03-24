using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 🌟 สร้างกล่องเก็บข้อมูลการ์ด (ชื่อ + รูปภาพ)
[System.Serializable]
public class BuffCard
{
    public string buffName;
    public Sprite cardSprite; // ช่องสำหรับลากรูปการ์ดมาใส่
}

public class BuffManager : MonoBehaviour
{
    public static BuffManager instance;

    [Header("UI ตั้งค่า")]
    public GameObject buffPanel;
    public Button[] buffButtons;
    public TextMeshProUGUI[] buffTexts;
    public Image[] buffImages; // 🌟 เพิ่มช่องสำหรับเปลี่ยนรูปการ์ดบน UI

    [Header("🎵 เสียงประกอบ")]
    public AudioSource uiAudioSource;
    public AudioClip popupSound;
    public AudioClip clickSound;

    [Header("🃏 คลังการ์ดทั้งหมด (ตั้งค่าใน Inspector)")]
    public List<BuffCard> allAvailableCards; // ใส่ข้อมูลการ์ดทั้ง 4 ใบตรงนี้

    private List<BuffCard> currentOptions = new List<BuffCard>();

    void Awake()
    {
        instance = this;
        if (buffPanel != null) buffPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ShowBuffSelection();
        }
    }

    public void ShowBuffSelection()
    {
        Time.timeScale = 0f;
        buffPanel.SetActive(true);
        currentOptions.Clear();

        if (uiAudioSource != null && popupSound != null)
        {
            uiAudioSource.PlayOneShot(popupSound);
        }

        // 🌟 ระบบสุ่มการ์ด (สุ่มมา 3 ใบแบบไม่ซ้ำ พร้อมรูปภาพ)
        List<BuffCard> tempCards = new List<BuffCard>(allAvailableCards);
        for (int i = 0; i < 3; i++)
        {
            if (tempCards.Count == 0) break; // กันบั๊กกรณีการ์ดไม่พอ

            int randIndex = Random.Range(0, tempCards.Count);
            BuffCard selectedCard = tempCards[randIndex];

            currentOptions.Add(selectedCard);
            buffTexts[i].text = selectedCard.buffName;

            // 🌟 สั่งเปลี่ยนรูปภาพบนหน้า UI ให้ตรงกับการ์ดที่สุ่มได้!
            if (buffImages[i] != null && selectedCard.cardSprite != null)
            {
                buffImages[i].sprite = selectedCard.cardSprite;
            }

            tempCards.RemoveAt(randIndex);
        }
    }

    public void SelectBuff(int buttonIndex)
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }

        string chosenBuffName = currentOptions[buttonIndex].buffName;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            if (chosenBuffName == "Fast Cast (ลดคูลดาวน์ 15%)")
            {
                Debug.Log("อัปเกรด: Fast Cast!");
                // โค้ดลดคูลดาวน์
            }
            else if (chosenBuffName == "Wind Walker (วิ่งเร็วขึ้น 10%)")
            {
                PlayerController movement = player.GetComponent<PlayerController>();
                if (movement != null) movement.moveSpeed *= 1.10f;
            }
            else if (chosenBuffName == "Giant Blast (กระสุนใหญ่ขึ้น 20%)")
            {
                PlayerCombat combat = player.GetComponent<PlayerCombat>();
                if (combat != null) combat.bulletSizeMultiplier += 0.2f;
            }
            else if (chosenBuffName == "Multi-Shot (กระสุน +1 นัด)")
            {
                Debug.Log("อัปเกรด: Multi-Shot!");
                // โค้ดเพิ่มกระสุน
            }
        }

        buffPanel.SetActive(false);
        Time.timeScale = 1f;

        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.ResumeWave();
        }
    }
}