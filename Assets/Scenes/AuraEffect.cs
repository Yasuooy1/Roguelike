using UnityEngine;

public class AuraEffect : MonoBehaviour
{
    // ฟังก์ชันนี้จะถูกเรียกตอนออร่าเล่นจบ
    public void DisableAura()
    {
        gameObject.SetActive(false); // สั่งปิดตัวเอง
    }
}