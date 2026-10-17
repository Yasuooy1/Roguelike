using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class BuffCard
{
    public string buffName;
    public Sprite cardSprite;
    [TextArea(2, 3)] public string description;
}

[System.Serializable]
public class ActiveBuff
{
    public string buffName;
    public int remainingWaves;
    public bool isPermanent;
    public Sprite buffIcon; // 🌟 เพิ่มตัวแปรเก็บรูปภาพไอคอน
}

public class BuffManager : MonoBehaviour
{
    public static BuffManager instance;

    [Header("UI หน้าต่างเลือกการ์ด")]
    public GameObject buffPanel;
    public Button[] buffButtons;
    public TextMeshProUGUI[] buffTexts;
    public TextMeshProUGUI[] buffDescTexts;

    [Header("UI โชว์สถานะบัฟใต้หลอดมานา (แบบไอคอน)")]
    public Transform buffIconContainer; // 🌟 กล่องจัดเรียงไอคอน
    public GameObject buffIconPrefab;   // 🌟 ชิ้นส่วนไอคอนต้นแบบ

    [Header("🎵 เสียงประกอบ")]
    public AudioSource uiAudioSource;
    public AudioClip popupSound;
    public AudioClip clickSound;

    [Header("🃏 กองการ์ดทั้งหมด")]
    public List<BuffCard> normalBuffCards;
    public List<BuffCard> bossBuffCards;

    private List<BuffCard> currentOptions = new List<BuffCard>();
    public List<ActiveBuff> currentActiveBuffs = new List<ActiveBuff>();

    private bool isSelectingBossBuff = false;

    void Awake()
    {
        instance = this;
        if (buffPanel != null) buffPanel.SetActive(false);
    }

    public void ShowBuffSelection(bool isBossWave = false)
    {
        isSelectingBossBuff = isBossWave;
        Time.timeScale = 0f;
        buffPanel.SetActive(true);
        currentOptions.Clear();

        if (uiAudioSource != null && popupSound != null) uiAudioSource.PlayOneShot(popupSound);

        List<BuffCard> tempCards = new List<BuffCard>(isBossWave ? bossBuffCards : normalBuffCards);

        for (int i = 0; i < 3; i++)
        {
            if (tempCards.Count == 0) break;
            int randIndex = Random.Range(0, tempCards.Count);
            BuffCard selectedCard = tempCards[randIndex];

            currentOptions.Add(selectedCard);

            if (buffTexts[i] != null) buffTexts[i].text = selectedCard.buffName;
            if (buffDescTexts[i] != null) buffDescTexts[i].text = selectedCard.description;

            if (buffButtons[i] != null && selectedCard.cardSprite != null)
            {
                Image btnImage = buffButtons[i].GetComponent<Image>();
                if (btnImage != null) btnImage.sprite = selectedCard.cardSprite;
            }

            tempCards.RemoveAt(randIndex);
        }

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

        ActiveBuff existingBuff = currentActiveBuffs.Find(b => b.buffName == chosenBuffName);
        if (existingBuff != null)
        {
            if (!existingBuff.isPermanent) existingBuff.remainingWaves = 2;
        }
        else
        {
            currentActiveBuffs.Add(new ActiveBuff
            {
                buffName = chosenBuffName,
                remainingWaves = isSelectingBossBuff ? 999 : 2,
                isPermanent = isSelectingBossBuff,
                buffIcon = currentOptions[buttonIndex].cardSprite // 🌟 จำรูปการ์ดมาโชว์เป็นไอคอน
            });
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
        PlayerController movement = player.GetComponent<PlayerController>();
        PlayerCombat combat = player.GetComponent<PlayerCombat>();

        if (buffName == "Wind Walker" && movement != null) movement.moveSpeed *= 1.25f;
        else if (buffName == "Fast Cast" && combat != null) combat.fireCooldown *= 0.70f;
        else if (buffName == "Time Weaver" && combat != null) combat.ultimateDuration += 1.5f;
        else if (buffName == "Mana Flow" && combat != null) combat.ultimateManaDiscount += 1;
        else if (buffName == "Void Blink" && movement != null) movement.teleportCooldown *= 2.5f;
    }

    private void RemoveBuffEffects(string buffName)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        PlayerController movement = player.GetComponent<PlayerController>();
        PlayerCombat combat = player.GetComponent<PlayerCombat>();

        if (buffName == "Wind Walker" && movement != null) movement.moveSpeed /= 1.25f;
        else if (buffName == "Fast Cast" && combat != null) combat.fireCooldown /= 0.70f;
        else if (buffName == "Time Weaver" && combat != null) combat.ultimateDuration -= 1.5f;
        else if (buffName == "Mana Flow" && combat != null) combat.ultimateManaDiscount -= 1;
        else if (buffName == "Void Blink" && movement != null) movement.teleportCooldown /= 2.5f;
    }

    public void OnWaveEnded()
    {
        for (int i = currentActiveBuffs.Count - 1; i >= 0; i--)
        {
            if (currentActiveBuffs[i].isPermanent) continue;

            currentActiveBuffs[i].remainingWaves--;
            if (currentActiveBuffs[i].remainingWaves <= 0)
            {
                RemoveBuffEffects(currentActiveBuffs[i].buffName);
                currentActiveBuffs.RemoveAt(i);
            }
        }
        UpdateBuffUI();
    }

    // 🌟 ฟังก์ชันอัปเดต UI ไอคอน
    // 🌟 ฟังก์ชันอัปเดต UI ไอคอน (แก้ใหม่ให้ค้นหาอัตโนมัติจาก Player)
    private void UpdateBuffUI()
    {
        // 1. ถ้าช่องมันว่างอยู่ ให้ควานหาจากตัว Player ที่อยู่ในฉาก
        if (buffIconContainer == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Transform[] allTransforms = player.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allTransforms)
                {
                    // ค้นหา Object ที่ชื่อ "BuffIconContainer"
                    if (t.name == "BuffIconContainer")
                    {
                        buffIconContainer = t;
                        break; // เจอแล้วหยุดหา
                    }
                }
            }
        }

        // ถ้ายังหาไม่เจออีก หรือลืมใส่ Prefab ไอคอน ให้หยุดทำงาน
        if (buffIconContainer == null || buffIconPrefab == null) return;

        // 2. ล้างไอคอนเก่าออกให้หมดก่อน
        foreach (Transform child in buffIconContainer)
        {
            Destroy(child.gameObject);
        }

        // 3. สร้างไอคอนใหม่ตามจำนวนบัฟที่มีอยู่
        foreach (var buff in currentActiveBuffs)
        {
            GameObject newIcon = Instantiate(buffIconPrefab, buffIconContainer);
            BuffIconUI iconUI = newIcon.GetComponent<BuffIconUI>();

            if (iconUI != null)
            {
                iconUI.Setup(buff); // ส่งข้อมูลบัฟไปให้สคริปต์จัดการ UI
            }
            else
            {
                // กันเหนียว กรณีลืมแปะสคริปต์
                Image img = newIcon.GetComponent<Image>();
                if (img != null) img.sprite = buff.buffIcon;
            }
        }
    }

    public bool HasBuff(string buffName)
    {
        return currentActiveBuffs.Exists(b => b.buffName == buffName);
    }
}