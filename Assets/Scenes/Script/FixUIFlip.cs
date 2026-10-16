using UnityEngine;

public class FixUIFlip : MonoBehaviour
{
    private Vector3 initialScale;

    void Start()
    {
        // จำขนาดดั้งเดิมของมันไว้ตอนเริ่มเกม
        initialScale = transform.localScale;
    }

    void LateUpdate()
    {
        // 1. ล็อกการหมุน (กันบั๊กมอนสเตอร์หันด้วยการหมุนแกน Y 180 องศา)
        transform.rotation = Quaternion.identity;

        // 2. ล็อกการสเกล (กันบั๊กมอนสเตอร์หันด้วยการปรับ Scale X เป็น -1)
        if (transform.parent != null)
        {
            // เช็กว่าค้างคาวตัวแม่สเกลติดลบอยู่ไหม
            float parentScaleX = transform.parent.localScale.x;

            // สั่งให้ UI เอาค่ามาคูณแก้ทาง (ลบเจอลบเป็นบวก) ทำให้ภาพพัซเซิลตั้งตรงและเรียงจากซ้ายไปขวาเสมอ!
            transform.localScale = new Vector3(initialScale.x * Mathf.Sign(parentScaleX), initialScale.y, initialScale.z);
        }
    }
}