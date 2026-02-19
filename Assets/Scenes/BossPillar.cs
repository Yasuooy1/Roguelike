using UnityEngine;

public class BossPillar : MonoBehaviour
{
    public PlayerCombat.Element pillarElement;
    public int health = 30;

    private SpriteRenderer spriteRenderer;
    private Boss mainBoss; // อ้างอิงถึงบอสหลัก

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    // ให้บอสส่งข้อมูลตัวเองมาให้เสาตอนที่เสกเสาขึ้นมา
    public void SetupPillar(Boss boss, PlayerCombat.Element element)
    {
        mainBoss = boss;
        pillarElement = element;
        UpdateColor();
    }

    void UpdateColor()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        switch (pillarElement)
        {
            case PlayerCombat.Element.Red: spriteRenderer.color = Color.red; break;
            case PlayerCombat.Element.Green: spriteRenderer.color = Color.green; break;
            case PlayerCombat.Element.Blue: spriteRenderer.color = Color.blue; break;
        }
    }

    // ฟังก์ชันรับดาเมจจากกระสุนผู้เล่น (กระสุนต้องเรียกใช้ TakeDamage เหมือนตอนยิงมอนสเตอร์ปกติ)
    public void TakeDamage(int damage, PlayerCombat.Element hitElement)
    {
        bool isWeakness = false;

        // เช็กชนะทาง (แดงชนะเขียว, เขียวชนะฟ้า, ฟ้าชนะแดง)
        if (hitElement == PlayerCombat.Element.Red && pillarElement == PlayerCombat.Element.Green) isWeakness = true;
        else if (hitElement == PlayerCombat.Element.Green && pillarElement == PlayerCombat.Element.Blue) isWeakness = true;
        else if (hitElement == PlayerCombat.Element.Blue && pillarElement == PlayerCombat.Element.Red) isWeakness = true;

        if (isWeakness)
        {
            health -= damage;
            Debug.Log("เสาโดนโจมตี! เลือดเสาเหลือ: " + health);

            if (health <= 0)
            {
                // แจ้งบอสว่าเสานี้พังแล้ว!
                if (mainBoss != null) mainBoss.OnPillarDestroyed();
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("ยิงเสาผิดสี!");
        }
    }
}