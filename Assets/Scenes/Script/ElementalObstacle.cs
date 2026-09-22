using UnityEngine;

public class ElementalObstacle : MonoBehaviour
{
    [Header("ตั้งค่าการแพ้ธาตุ (สูตรเวทมนตร์)")]
    public string weaknessRecipe = "Fire";

    [Header("เอฟเฟกต์เมื่อถูกทำลาย")]
    public GameObject destroyEffectPrefab;
    public AudioClip burnSound;

    [Header("ระบบดาเมจ (หนามทิ่ม)")]
    public int damageToPlayer = 10;
    public float damageCooldown = 1f; // ระยะเวลาอมตะชั่วคราวก่อนโดนทิ่มซ้ำ
    private float lastDamageTime = 0f;

    // 🌟 ฟังก์ชันทำดาเมจใส่ผู้เล่นที่เดินมาชน
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // เช็กคูลดาวน์ เพื่อไม่ให้เลือดลดรัวๆ เป็นน้ำ
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                
                PlayerHealth playerHp = collision.GetComponent<PlayerHealth>();

                if (playerHp != null)
                {
                    playerHp.TakeDamage(damageToPlayer); // สั่งลดเลือด
                    lastDamageTime = Time.time;
                    Debug.Log("💥 ผู้เล่นโดนหนามทิ่ม!");
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Bullet bullet = collision.GetComponent<Bullet>();

            if (bullet != null && bullet.puzzleRecipe == weaknessRecipe)
            {
                DestroyObstacle();
                Destroy(collision.gameObject);
            }
        }
    }

    private void DestroyObstacle()
    {
        Debug.Log($"🔥 {gameObject.name} ถูกทำลายด้วยสูตร: {weaknessRecipe}!");

        if (burnSound != null) AudioSource.PlayClipAtPoint(burnSound, transform.position);
        if (destroyEffectPrefab != null) Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);

        gameObject.SetActive(false);
    }
}