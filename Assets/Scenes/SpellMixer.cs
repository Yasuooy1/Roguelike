using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellMixer : MonoBehaviour
{
    // กำหนดธาตุที่มีในเกม (ไฟ, น้ำ, สายฟ้า)
    public enum Element { Fire, Water, Lightning }

    [Header("ลูกแก้วที่กำลังผสมอยู่")]
    public List<Element> currentOrbs = new List<Element>();

    [Header("UI โชว์ลูกแก้ว")]
    public Image[] orbSlots; // ใส่ช่อง UI 3 ช่อง
    public Sprite fireSprite;      // รูปวงกลมสีแดง
    public Sprite waterSprite;     // รูปวงกลมสีฟ้า
    public Sprite lightningSprite; // รูปวงกลมสีเหลือง

    void Start()
    {
        // เคลียร์ UI ให้ว่างเปล่าตอนเริ่มเกม
        UpdateOrbUI();
    }

    void Update()
    {
        // 🌟 กดคีย์บอร์ดเพื่อเรียกลูกแก้วธาตุ (แค่นี้ก็ถือว่าเก็บค่าแล้ว!)
        if (Input.GetKeyDown(KeyCode.Q)) AddOrb(Element.Fire);
        if (Input.GetKeyDown(KeyCode.W)) AddOrb(Element.Water);
        if (Input.GetKeyDown(KeyCode.E)) AddOrb(Element.Lightning);

        // ❌ ลบปุ่ม Spacebar ทิ้งไปแล้ว เพราะเราไปใช้คลิกขวาใน PlayerCombat แทน!
    }

    // ฟังก์ชันเพิ่มลูกแก้วลงในหลอด
    void AddOrb(Element newOrb)
    {
        // ถ้าลูกแก้วเต็ม 3 ลูกแล้ว ให้เตะลูกเก่าสุดทิ้ง
        if (currentOrbs.Count >= 3)
        {
            currentOrbs.RemoveAt(0);
        }

        // ยัดลูกใหม่เข้าไปต่อท้าย
        currentOrbs.Add(newOrb);
        UpdateOrbUI(); // อัปเดตภาพบนจอ
    }

    // ❌ ลบฟังก์ชัน CastSpell() ทิ้งไปแล้ว!

    // ฟังก์ชันวาดรูป UI
    void UpdateOrbUI()
    {
        for (int i = 0; i < orbSlots.Length; i++)
        {
            if (i < currentOrbs.Count)
            {
                orbSlots[i].enabled = true; // เปิดรูป
                if (currentOrbs[i] == Element.Fire) orbSlots[i].sprite = fireSprite;
                else if (currentOrbs[i] == Element.Water) orbSlots[i].sprite = waterSprite;
                else if (currentOrbs[i] == Element.Lightning) orbSlots[i].sprite = lightningSprite;
            }
            else
            {
                orbSlots[i].enabled = false; // ซ่อนรูปถ้าช่องนั้นยังว่าง
            }
        }
    }

    // ฟังก์ชันสำหรับให้ PlayerCombat สั่งลบลูกแก้วหลังยิงเจาะเกราะเสร็จ
    public void ClearOrbs()
    {
        currentOrbs.Clear();
        UpdateOrbUI();
    }
}