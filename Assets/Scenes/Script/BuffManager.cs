using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

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
    public int stackCount;
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
    private bool isProcessingChoice = false; // ตัวล็อกกันคลิกเบิ้ล

    void Awake()
    {
        instance = this;
        if (buffPanel != null) buffPanel.SetActive(false);
    }

    public void ShowBuffSelection(bool isBossWave = false)
    {
        isProcessingChoice = false;
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
            string cardNameLower = selectedCard.buffName.Trim().ToLower();

            // 🌟 เช็กเงื่อนไข: ถ้าเป็นการ์ดห้ามซ้ำ (Time Weaver, Mana Flow หรือเปิดจากกองบอส) และผู้เล่นมีอยู่แล้ว จะไม่สุ่มขึ้นมาให้เลือกซ้ำ
            bool isUniqueCard = (cardNameLower == "time weaver" || cardNameLower == "mana flow" || isBossWave);
            if (isUniqueCard && currentActiveBuffs.Exists(b => b.buffName.Trim().ToLower() == cardNameLower))
            {
                tempCards.RemoveAt(randIndex);
                i--;
                continue;
            }

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

    public void SelectBuff(int buttonIndex)
    {
        StartCoroutine(SelectBuffRoutine(buttonIndex));
    }

    private IEnumerator SelectBuffRoutine(int buttonIndex)
    {
        if (isProcessingChoice) yield break;
        isProcessingChoice = true;

        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }

        yield return new WaitForSecondsRealtime(0.15f);

        string chosenBuffName = currentOptions[buttonIndex].buffName;
        string cardNameLower = chosenBuffName.Trim().ToLower();

        // เช็กว่าเป็นกลุ่มการ์ดห้ามซ้ำเด็ดขาดไหม (Time Weaver, Mana Flow, หรือเปิดจากกองบอส)
        bool isUniqueCard = (cardNameLower == "time weaver" || cardNameLower == "mana flow" || isSelectingBossBuff);

        ActiveBuff existingBuff = currentActiveBuffs.Find(b => b.buffName.Trim().ToLower() == cardNameLower);

        if (existingBuff != null)
        {
            if (isUniqueCard)
            {
                Debug.Log("การ์ดนี้จำกัดให้มีได้แค่ 1 ใบเท่านั้น!");
            }
            else
            {
                // 🌟 การ์ดทั่วไป อนุญาตให้สแตกได้ แต่จำกัดเพดานสูงสุดไม่เกิน 5 ครั้ง
                if (existingBuff.stackCount < 5)
                {
                    existingBuff.stackCount++;
                    ApplyBuff(chosenBuffName);
                }
                else
                {
                    Debug.Log("การ์ดนี้สะสมครบเพดานสูงสุด 5 ครั้งแล้ว!");
                }
            }
        }
        else
        {
            currentActiveBuffs.Add(new ActiveBuff
            {
                buffName = chosenBuffName,
                stackCount = 1,
                isPermanent = true,
                buffIcon = currentOptions[buttonIndex].cardSprite
            });
            ApplyBuff(chosenBuffName);
        }

        UpdateBuffUI();

        buffPanel.SetActive(false);
        Time.timeScale = 1f;

        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
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
        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (buffName == "Wind Walker" && movement != null)
        {
            movement.moveSpeed *= 1.15f;
            if (movement.moveSpeed > 12f) movement.moveSpeed = 12f;
        }
        else if (buffName == "Fast Cast" && combat != null)
        {
            combat.fireCooldown *= 0.85f;
            if (combat.fireCooldown < 0.15f) combat.fireCooldown = 0.15f;
        }
        else if (buffName == "Time Weaver" && combat != null)
        {
            combat.ultimateDuration += 1.5f;
            if (combat.ultimateDuration > 10f) combat.ultimateDuration = 10f;
        }
        else if (buffName == "Mana Flow" && combat != null)
        {
            combat.ultimateManaDiscount += 1;
            if (combat.ultimateManaDiscount > 2) combat.ultimateManaDiscount = 2;
        }
        else if (buffName == "Void Blink" && movement != null)
        {
            movement.teleportCooldown *= 0.8f;
        }
        else if (buffName == "Damage Up" && combat != null)
        {
            combat.bonusDamage += 3; // สะสมได้สูงสุด 5 ครั้ง
        }
        else if (buffName == "Health Up" && health != null)
        {
            if (health.maxHealth < 25)
            {
                health.maxHealth += 1;
                health.currentHealth += 1;
                health.RefreshHealthStat();
            }
        }
    }

    public void OnWaveEnded() { }

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