using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;

    [HideInInspector]
    public PlayerCombat.Element bulletElement; // เก็บสีของกระสุน

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // บังคับให้พุ่งไปด้านหน้าของตัวเอง (ตามมุมที่ถูกหมุนมา)
        rb.velocity = transform.up * speed;

        Destroy(gameObject, 5f); // กระสุนจะหายไปเองใน 5 วิ เพื่อไม่ให้รกจอ
    }

    public void Setup(PlayerCombat.Element element)
    {
        bulletElement = element;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // ย้อมสีกระสุนตามธาตุของบอส
        switch (element)
        {
            case PlayerCombat.Element.Red: sr.color = Color.red; break;
            case PlayerCombat.Element.Green: sr.color = Color.green; break;
            case PlayerCombat.Element.Blue: sr.color = Color.blue; break;
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // ถ้าชนผู้เล่น
        if (hitInfo.CompareTag("Player"))
        {
            PlayerCombat playerCombat = hitInfo.GetComponent<PlayerCombat>();
            PlayerHealth playerHealth = hitInfo.GetComponent<PlayerHealth>();

            if (playerCombat != null && playerHealth != null)
            {
                // 🌟 กิมมิค Outland: ถ้าสีอาวุธผู้เล่น "ตรงกับ" สีกระสุน = ปลอดภัย!
                if (playerCombat.currentElement == bulletElement)
                {
                    Debug.Log("<color=green>ดูดซับกระสุน! (ป้องกันสำเร็จเพราะสีเดียวกัน)</color>");
                    Destroy(gameObject);
                }
                else
                {
                    // ถ้าสีไม่ตรงกัน โดนดาเมจเต็มๆ!
                    Debug.Log("<color=red>โดนกระสุน! (สีไม่ตรงกัน)</color>");
                    playerHealth.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }
        // ถ้าชนกำแพงหรือพื้น ก็ให้กระสุนหายไป
        else if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}