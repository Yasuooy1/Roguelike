using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections; // 🌟 เพิ่มเนมสเปซนี้เพื่อให้ใช้งาน Coroutine ได้

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
    public Sprite buffIcon;
}

public class BuffManager : MonoBehaviour
{
    public static BuffManager instance;

    [Header("UI หน้าต่างเลือกการ์ด")]
    public GameObject buffPanel;
    public Button[] buffButtons;
    public Image[] buffIcons;
    public TextMeshProUGUI[] buffTexts;
    public TextMeshProUGUI[] buffDescTexts;

    [Header("UI โชว์สถานะบัฟใต้หลอดมานา (แบบไอคอน)")]
    public Transform buffIconContainer;
    public GameObject buffIconPrefab;

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

            if (buffTexts.Length > i && buffTexts[i] != null) buffTexts[i].text = selectedCard.buffName;
            if (buffDescTexts.Length > i && buffDescTexts[i] != null) buffDescTexts[i].text = selectedCard.description;

            if (buffIcons.Length > i && buffIcons[i] != null && selectedCard.cardSprite != null)
            {
                buffIcons[i].sprite = selectedCard.cardSprite;
            }

            tempCards.RemoveAt(randIndex);
        }

        if (buffButtons.Length > 0 && buffButtons[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buffButtons[0].gameObject);
        }
    }

    // ==========================================
    // 🌟 1. ปรับแก้ SelectBuff ให้ส่งต่อผ่าน Coroutine (แก้ปัญหาโค้ดตีกัน)
    // ==========================================
    public void SelectBuff(int buttonIndex)
    {
        StartCoroutine(SelectBuffRoutine(buttonIndex));
    }

    private IEnumerator SelectBuffRoutine(int buttonIndex)
    {
        // เล่นเสียงคลิกตอนกดเลือก
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }

        // 🌟 หน่วงเวลา 0.15 วินาที เพื่อให้ ButtonJuice เล่นแอนิเมชันปุ่มยุบตัวให้เสร็จก่อนปิดจอ
        // ใช้ WaitForSecondsRealtime เพราะเวลาในเกมถูกหยุดด้วย Time.timeScale = 0
        yield return new WaitForSecondsRealtime(0.15f);

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
                buffIcon = currentOptions[buttonIndex].cardSprite
            });
            ApplyBuff(chosenBuffName);
        }

        UpdateBuffUI();

        // ปิดหน้าต่างการ์ด และคืนเวลาเกมตามปกติ
        buffPanel.SetActive(false);
        Time.timeScale = 1f;

        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            
                // 🌟 เช็กว่าถ้าเป็นเวฟปกติ ให้ลุยเวฟต่อไป
                // 🌟 แต่ถ้าเป็นการ์ดบอส จะไม่สั่งเริ่มเวฟ ปล่อยให้ผู้เล่นเดินไปเข้าประตูเอง
                if (!isSelectingBossBuff)
                {
                    waveManager.ResumeWave();
                }
            
        }    
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

    private void UpdateBuffUI()
    {
        if (buffIconContainer == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Transform[] allTransforms = player.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allTransforms)
                {
                    if (t.name == "BuffIconContainer")
                    {
                        buffIconContainer = t;
                        break;
                    }
                }
            }
        }

        if (buffIconContainer == null || buffIconPrefab == null) return;

        foreach (Transform child in buffIconContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var buff in currentActiveBuffs)
        {
            GameObject newIcon = Instantiate(buffIconPrefab, buffIconContainer);
            BuffIconUI iconUI = newIcon.GetComponent<BuffIconUI>();

            if (iconUI != null)
            {
                iconUI.Setup(buff);
            }
            else
            {
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