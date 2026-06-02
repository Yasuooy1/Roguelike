using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private float length, startpos;
    public GameObject cam;
    public float parallaxEffect; // ค่าความช้า/เร็วของการเลื่อนตามกล้อง

    void Start()
    {
        startpos = transform.position.x;
        // หาความกว้างของรูปภาพ เพื่อเอาไว้วนลูป
        length = GetComponent<SpriteRenderer>().bounds.size.x;

        // ถ้าลืมลากกล้องใส่ มันจะหา Main Camera ให้เองอัตโนมัติ
        if (cam == null)
        {
            cam = Camera.main.gameObject;
        }
    }

    // ใช้ LateUpdate เพื่อให้มันขยับ "หลัง" จากที่กล้องหลัก (Cinemachine) ขยับเสร็จแล้ว ภาพจะได้ไม่กระตุก
    void LateUpdate()
    {
        // คำนวณระยะที่กล้องเดินไปแล้วเทียบกับฉาก
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        // ขยับฉากหลังตามค่าที่ตั้งไว้
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // --- ระบบวนลูปอัตโนมัติ (Infinite Loop) ---
        if (temp > startpos + length)
        {
            startpos += length; // ถอยรูปมาต่อด้านหน้า
        }
        else if (temp < startpos - length)
        {
            startpos -= length; // ถอยรูปไปต่อด้านหลัง
        }
    }
}