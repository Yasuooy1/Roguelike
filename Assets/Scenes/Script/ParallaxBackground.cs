using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private float length;
    private float startpos;
    public GameObject cam;

    [Range(0f, 1f)]
    public float parallaxEffect; // ความช้า/เร็วตามใจชอบเลยครับ สมูทแน่นอน

    void Start()
    {
        startpos = transform.position.x;

        // ดึงความกว้างจริงของรูปต้นฉบับมาเป็นเกณฑ์วนลูป
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            length = (sr.sprite.rect.width / sr.sprite.pixelsPerUnit) * transform.localScale.x;
        }
        else
        {
            length = 20f;
        }

        if (cam == null)
        {
            cam = Camera.main.gameObject;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // 1. คำนวณการเคลื่อนที่ตาม Parallax แบบดั้งเดิมที่คุ้นเคย
        float dist = cam.transform.position.x * parallaxEffect;
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // 2. เช็กระยะห่างจากกล้องแบบเรียลไทม์ (ตัดปัญหาเรื่องโหลดช้า/หน่วง)
        float offset = cam.transform.position.x - transform.position.x;

        // พอกล้องขยับเลยระยะของภาพ จะทำการวนลูปตำแหน่งทันทีอย่างไร้รอยต่อ
        if (offset > length)
        {
            startpos += length;
        }
        else if (offset < -length)
        {
            startpos -= length;
        }
    }
}