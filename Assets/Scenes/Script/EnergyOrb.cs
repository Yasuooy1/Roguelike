/*using UnityEngine;

public class EnergyOrb : MonoBehaviour
{
    void Start()
    {
        // ถ้าผู้เล่นไม่ยอมมาเก็บใน 5 วิ ให้มันสลายไปเอง (จะได้ไม่รกจอ)
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            PlayerCombat player = hitInfo.GetComponent<PlayerCombat>();
            if (player != null)
            {
                player.CollectEnergy(); // ส่งพลังงานเข้าตัวผู้เล่น
                Destroy(gameObject);    // เก็บแล้วหายไป
            }
        }
    }
}*/