using UnityEngine;
using System.Collections;

public class GameFeelManager : MonoBehaviour
{
    public static GameFeelManager instance;

    private Vector3 cameraOriginalPos;

    void Awake()
    {
        instance = this;
    }

    // 🌟 1. ฟังก์ชันสั่งจอสั่น (ความแรง, ระยะเวลา)
    public void ScreenShake(float magnitude, float duration)
    {
        StartCoroutine(ShakeRoutine(magnitude, duration));
    }

    IEnumerator ShakeRoutine(float magnitude, float duration)
    {
        cameraOriginalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // สุ่มตำแหน่งกล้องให้สั่นไปมา
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(cameraOriginalPos.x + x, cameraOriginalPos.y + y, cameraOriginalPos.z);

            // ใช้ unscaledDeltaTime เพื่อให้จอสั่นได้แม้จะติด Hit Stop (เวลาหยุด)
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = cameraOriginalPos; // ดึงกล้องกลับที่เดิม
    }

    // 🌟 2. ฟังก์ชันสั่งหยุดเวลาชั่วคราว (เพิ่มความสะใจตอนตีโดน)
    public void HitStop(float duration)
    {
        StartCoroutine(HitStopRoutine(duration));
    }

    IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.1f; // สโลว์โมชันหน่วงเวลาให้ช้าลงสุดๆ
        yield return new WaitForSecondsRealtime(duration); // รอด้วยเวลาจริง
        Time.timeScale = 1f;   // คืนค่าเวลาให้กลับมาปกติ
    }
}