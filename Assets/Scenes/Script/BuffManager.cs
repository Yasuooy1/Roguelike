using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 🌟 1. เพิ่มบรรทัดนี้เข้ามาครับ

[System.Serializable]
public class BuffCard
{
    public string buffName;
    public Sprite cardSprite;
}

[System.Serializable]
public class ActiveBuff
{
    public string buffName;
    public int remainingWaves;
}

public class BuffManager : MonoBehaviour
{
    public static BuffManager instance;

    [Header("UI ตั้งค่า")]
    public GameObject buffPanel;
    public Button[] buffButtons;
    public TextMeshProUGUI[] buffTexts;

    [Header("UI โชว์สถานะบัฟปัจจุบัน")]
    public TextMeshProUGUI currentBuffStatusText;

    [Header("🎵 เสียงประกอบ")]
    public AudioSource uiAudioSource;
    public AudioClip popupSound;
    public AudioClip clickSound;

    [Header("🃏 คลังการ์ดทั้งหมด")]
    public List<BuffCard> allAvailableCards;

    private List<BuffCard> currentOptions = new List<BuffCard>();

    public List<ActiveBuff> currentActiveBuffs = new List<ActiveBuff>();

    void Awake()
    {
        instance = this;
        if (buffPanel != null) buffPanel.SetActive(false);
        if (currentBuffStatusText != null) currentBuffStatusText.text = "";

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
                if (btnImage != null)
                {
                    btnImage.sprite = selectedCard.cardSprite;
                }
            }

            tempCards.RemoveAt(randIndex);
        }

        // ==========================================
        // 🌟 2. บังคับจอยสติ๊กให้โฟกัสที่การ์ดใบแรกสุด (ซ้ายมือ) ทันทีที่เปิดหน้าจอ
        // ==========================================
        if (buffButtons.Length > 0 && buffButtons[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // ล้างค่าเก่าทิ้งก่อน
            EventSystem.current.SetSelectedGameObject(buffButtons[0].gameObject); // ล็อกเป้าใบแรก
        }
    }

    public void SelectBuff(int buttonIndex)
    {
        if (uiAudioSource != null && clickSound != null) uiAudioSource.PlayOneShot(clickSound);

        string chosenBuffName = currentOptions[buttonIndex].buffName;

        ActiveBuff existingBuff = currentActiveBuffs.Find(b => b.buffName == chosenBuffName);
        if (existingBuff != null)
        {
            existingBuff.remainingWaves = 2;
        }
        else
        {
            currentActiveBuffs.Add(new ActiveBuff { buffName = chosenBuffName, remainingWaves = 2 });
            ApplyBuff(chosenBuffName);
        }

        UpdateBuffUI();

        buffPanel.SetActive(false);
        Time.timeScale = 1f;

        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null) waveManager.ResumeWave();
    }

    private void ApplyBuff(string buffName)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (buffName == "Wind Walker")
        {
            PlayerController movement = player.GetComponent<PlayerController>();
            if (movement != null) movement.moveSpeed *= 1.20f;
        }
        else if (buffName == "Giant Blast")
        {
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null) combat.bulletSizeMultiplier += 0.5f;
        }
        else if (buffName == "Fast Cast")
        {
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null) combat.fireCooldown *= 0.70f;
        }
        else if (buffName == "Prismatic Aegis")
        {
            Debug.Log("เปิดใช้งานโล่ป้องกัน!");
        }
    }

    private void RemoveBuffEffects(string buffName)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (buffName == "Wind Walker")
            {
                PlayerController movement = player.GetComponent<PlayerController>();
                if (movement != null) movement.moveSpeed /= 1.20f;
            }
            else if (buffName == "Giant Blast")
            {
                PlayerCombat combat = player.GetComponent<PlayerCombat>();
                if (combat != null) combat.bulletSizeMultiplier -= 0.5f;
            }
            else if (buffName == "Fast Cast")
            {
                PlayerCombat combat = player.GetComponent<PlayerCombat>();
                if (combat != null) combat.fireCooldown /= 0.70f;
            }
        }
    }

    public void OnWaveEnded()
    {
        for (int i = currentActiveBuffs.Count - 1; i >= 0; i--)
        {
            currentActiveBuffs[i].remainingWaves--;

            if (currentActiveBuffs[i].remainingWaves <= 0)
            {
                RemoveBuffEffects(currentActiveBuffs[i].buffName);
                currentActiveBuffs.RemoveAt(i);
            }
        }

        UpdateBuffUI();
    }

    private void UpdateBuffUI()
    {
        if (currentBuffStatusText == null) return;

        if (currentActiveBuffs.Count == 0)
        {
            currentBuffStatusText.text = "";
            return;
        }

        string uiText = "Active Auras:\n";
        foreach (var buff in currentActiveBuffs)
        {
            uiText += $"- {buff.buffName} ({buff.remainingWaves} Waves)\n";
        }

        currentBuffStatusText.text = uiText;
    }
}