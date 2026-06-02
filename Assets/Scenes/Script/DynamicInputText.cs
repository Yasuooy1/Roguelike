using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // 🌟 ขาดไม่ได้เลยตัวนี้

public class DynamicInputText : MonoBehaviour
{
    [Header("องค์ประกอบ UI")]
    public TextMeshProUGUI guideText; // ลากตัวหนังสือ TextMeshPro มาใส่
    public PlayerInput playerInput;   // ลากตัวละคร Player (ที่มีคอมโพเนนต์ Player Input) มาใส่

    [Header("ข้อความตอนใช้คีย์บอร์ด")]
    [TextArea]
    public string keyboardText = "Press F to use Potion\nPress R to swap Potion\nPress E to Interact";

    [Header("ข้อความตอนใช้จอยสติ๊ก")]
    [TextArea]
    public string gamepadText = "Press D-Pad UP to use Potion\nPress D-Pad LEFT to swap Potion\nPress D-Pad DOWN to Interact";

    private string currentScheme = "";

    void Start()
    {
        // 🌟 ถ้าลืมลากใส่ ให้มันพยายามหาเอง
        if (guideText == null) guideText = GetComponent<TextMeshProUGUI>();
        if (playerInput == null) playerInput = FindObjectOfType<PlayerInput>();

        UpdateTextDisplay();
    }

    void Update()
    {
        if (playerInput == null) return;

        // 🌟 เช็กว่าตอนนี้ผู้เล่นกำลังใช้อุปกรณ์อะไร (Keyboard หรือ Gamepad)
        string newScheme = playerInput.currentControlScheme;

        // ถ้ามีการสลับอุปกรณ์ (เช่น วางเมาส์แล้วไปจับจอย) ให้เปลี่ยนข้อความทันที!
        if (newScheme != currentScheme)
        {
            currentScheme = newScheme;
            UpdateTextDisplay();
        }
    }

    private void UpdateTextDisplay()
    {
        if (guideText == null) return;

        // เช็กชื่อ Scheme (มักจะเป็น "Gamepad" หรือ "Keyboard&Mouse" ตามที่ตั้งในหน้าต่างดำ)
        if (currentScheme.Contains("Gamepad") || currentScheme.Contains("Joystick"))
        {
            guideText.text = gamepadText;
        }
        else
        {
            guideText.text = keyboardText;
        }
    }
}