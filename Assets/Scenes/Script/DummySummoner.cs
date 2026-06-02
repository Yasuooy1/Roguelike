using UnityEngine;

public class DummySummoner : MonoBehaviour
{
    [Header("มอนสเตอร์ที่จะเสก (Prefab)")]
    public GameObject dummyPrefab;

    [Header("จุดที่จะให้มอนสเตอร์โผล่")]
    public Transform spawnPoint;

    [Header("เอฟเฟกต์ตอนเสก (ถ้ามี)")]
    public GameObject spawnEffect;

    private GameObject currentDummy; // เก็บตัวแปรไว้เช็กว่ามีตัวเก่าอยู่ไหม

    void Update()
    {
        // (ตัวเลือก) ถ้าผู้เล่นเดินมาใกล้แท่น แล้วกดปุ่ม E เพื่อเรียก
        // if (Input.GetKeyDown(KeyCode.E) && isPlayerNear) { Summon(); }
    }

    // ฟังก์ชันสำหรับเรียกมอนสเตอร์
    public void Summon()
    {
        // 🌟 เช็กก่อนว่ามีตัวเก่าอยู่ไหม ถ้ามีให้ลบทิ้งก่อน (ป้องกันมอนสเตอร์ล้นจอ)
        if (currentDummy != null)
        {
            Destroy(currentDummy);
        }

        // เสกมอนสเตอร์ตัวใหม่
        currentDummy = Instantiate(dummyPrefab, spawnPoint.position, Quaternion.identity);
        if (HubRoomGuide.instance != null) HubRoomGuide.instance.PointToShop();

        // (ตัวเลือก) ปิดสคริปต์เดินของตัวละครนี้ เพื่อให้เป็นกระสอบทรายนิ่งๆ
        // currentDummy.GetComponent<EnemyAI>().enabled = false;

        // เล่นเอฟเฟกต์ควันหรือระเบิดเบาๆ
        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, spawnPoint.position, Quaternion.identity);
        }

        Debug.Log("Summoned a Training Dummy!");
    }

    // ถ้าอยากให้ใช้วิธี "ยิงใส่แท่น" เพื่อเรียกออกมา
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet")) // ถ้ากระสุนผู้เล่นมาโดน
        {
            Summon();
            // (ตัวเลือก) ทำอนิเมชันแท่นยุบลงไปตอนโดนยิง
        }
    }
}