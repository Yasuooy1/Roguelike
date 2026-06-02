using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float fadeSpeed = 3f;
    public float destroyTime = 1f;

    private TextMeshPro textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    // ฟังก์ชันใหม่ที่ให้เราสั่งสีและขนาดตัวอักษรได้โดยตรง
    public void SetupCustom(int damageAmount, Color customColor, float customSize)
    {
        if (damageAmount == 0) textMesh.SetText("Block!"); // ถ้าดาเมจ 0 ให้ขึ้นคำว่า Block
        else textMesh.SetText(damageAmount.ToString());

        textMesh.color = customColor;
        textMesh.fontSize = customSize;
        textColor = customColor;

        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;
    }
}