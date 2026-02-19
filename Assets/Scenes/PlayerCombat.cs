using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Shooting Setup")]
    public Transform firePoint;     // จุดยิง (ลาก FirePoint มาใส่)
    public GameObject bulletPrefab;  
    private SpriteRenderer playerSprite; // กระสุน (ลาก Prefab Bullet มาใส่)

    [Header("Element System")]
    public Element currentElement = Element.Red; // เริ่มต้นที่สีแดง

    // สร้างรายชื่อธาตุทั้ง 3 สี
    public enum Element { Red, Green, Blue }


    void Update()
    {
        // 1. กดยิง (คลิกซ้าย หรือ ปุ่ม J)
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }

        // 2. กดปุ่มเพื่อสลับธาตุ (เช่น กด Q เพื่อสลับ)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchElement();
        }
    }

    void Shoot()
    {
        // 1. สร้างกระสุน
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. ส่งค่า "ธาตุปัจจุบัน" ไปให้กระสุน
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.bulletElement = currentElement;

        // 3. เปลี่ยนสีกระสุน
        SpriteRenderer bulletSprite = bullet.GetComponent<SpriteRenderer>();
        switch (currentElement)
        {
            case Element.Red: bulletSprite.color = Color.red; break;
            case Element.Green: bulletSprite.color = Color.green; break;
            case Element.Blue: bulletSprite.color = Color.blue; break;
        }
    }

    void SwitchElement()
    {
        // สลับธาตุวนลูป แดง -> เขียว -> ฟ้า -> แดง
        if (currentElement == Element.Red) currentElement = Element.Green;
        else if (currentElement == Element.Green) currentElement = Element.Blue;
        else if (currentElement == Element.Blue) currentElement = Element.Red;

        Debug.Log("Current Element: " + currentElement); // พิมพ์บอกใน Console
        switch (currentElement)
        {
            case Element.Red: playerSprite.color = Color.red; break;
            case Element.Green: playerSprite.color = Color.green; break;
            case Element.Blue: playerSprite.color = Color.blue; break;
        }
    }
    // เพิ่มตัวแปรนี้ไว้ด้านบน
  

    void Start()
    {
        playerSprite = GetComponent<SpriteRenderer>();
    }

    // ในฟังก์ชันที่ใช้สลับธาตุ (ที่กดปุ่ม Q) ให้เพิ่มส่วนนี้:
    void ChangeElement()
    {
        // ... โค้ดสลับ Element เดิมของคุณ ...

        // อัปเดตสีที่ตัวละคร
        switch (currentElement)
        {
            case Element.Red: playerSprite.color = Color.red; break;
            case Element.Green: playerSprite.color = Color.green; break;
            case Element.Blue: playerSprite.color = Color.blue; break;
        }
    }
}