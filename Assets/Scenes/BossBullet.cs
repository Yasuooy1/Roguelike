using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;

    [Header("ดรอปพลังงานตอนชนพื้น")]
    public GameObject energyOrbPrefab; // 🌟 ลาก Prefab EnergyOrb มาใส่ช่องนี้!

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = transform.up * speed;
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            PlayerHealth ph = hitInfo.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage); // โดนตัว = เสียเลือด
            Destroy(gameObject);
        }
        else if (hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // 🌟 ชนพื้น = ดรอปก้อนพลังงาน!
            if (energyOrbPrefab != null)
            {
                Instantiate(energyOrbPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}