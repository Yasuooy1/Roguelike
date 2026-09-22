using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellMixer : MonoBehaviour
{
    // 🌟 1. ใช้ธาตุ 3 ชนิดให้ตรงกับพัซเซิลมอนสเตอร์
    public enum Element { Fire, Water, Wind }

    [Header("ลูกแก้วที่กำลังผสมอยู่")]
    public List<Element> currentOrbs = new List<Element>();

    [Header("ออบเจกต์ลูกแก้วในฉาก")]
    public SpriteRenderer[] orbRenderers;
    public Animator[] orbAnimators;

    [Header("ไฟล์อนิเมชันของแต่ละธาตุ")]
    public RuntimeAnimatorController fireAnimController;
    public RuntimeAnimatorController waterAnimController;
    public RuntimeAnimatorController windAnimController;

    [Header("ตั้งค่าการลอยวนรอบตัว (Orbit)")]
    public float orbitRadius = 1.2f;
    public float orbitSpeed = 150f;
    public float hoverSpeed = 3f;
    public float hoverHeight = 0.2f;
    public Vector3 centerOffset = new Vector3(0, 1f, 0);

    private float currentAngle = 0f;

    void Start()
    {
        UpdateOrbSprites();
    }

    void Update()
    {
        AnimateOrbs();
    }

    public void AddOrb(Element element)
    {
        // ถ้ายัดลูกแก้วเกิน 3 ลูก ให้ลบลูกเก่าสุดทิ้ง
        if (currentOrbs.Count >= 3)
        {
            currentOrbs.RemoveAt(0);
        }

        currentOrbs.Add(element);
        UpdateOrbSprites();

        // เล่นแอนิเมชันลูกแก้วเด้งขึ้นมา
        if (currentOrbs.Count > 0 && currentOrbs.Count <= orbRenderers.Length)
        {
            StartCoroutine(PopSpawnAnimation(orbRenderers[currentOrbs.Count - 1].transform));
        }
    }

    void UpdateOrbSprites()
    {
        for (int i = 0; i < orbRenderers.Length; i++)
        {
            if (i < currentOrbs.Count)
            {
                orbRenderers[i].enabled = true;

                if (orbAnimators.Length > i && orbAnimators[i] != null)
                {
                    if (currentOrbs[i] == Element.Fire) orbAnimators[i].runtimeAnimatorController = fireAnimController;
                    else if (currentOrbs[i] == Element.Water) orbAnimators[i].runtimeAnimatorController = waterAnimController;
                    else if (currentOrbs[i] == Element.Wind) orbAnimators[i].runtimeAnimatorController = windAnimController;
                }
            }
            else
            {
                orbRenderers[i].enabled = false;
            }
        }
    }

    void AnimateOrbs()
    {
        currentAngle += orbitSpeed * Time.deltaTime;

        int activeCount = currentOrbs.Count;
        if (activeCount == 0) return;

        for (int i = 0; i < orbRenderers.Length; i++)
        {
            if (orbRenderers[i].enabled)
            {
                float angleOffset = (360f / activeCount) * i;
                float finalAngle = currentAngle + angleOffset;

                float rad = finalAngle * Mathf.Deg2Rad;
                float xPos = Mathf.Cos(rad) * orbitRadius;
                float yPos = Mathf.Sin(rad) * orbitRadius;

                float floatOffset = Mathf.Sin(Time.time * hoverSpeed + (i * 1.5f)) * hoverHeight;

                Vector3 targetPos = transform.position + centerOffset + new Vector3(xPos, yPos + floatOffset, 0);

                orbRenderers[i].transform.position = Vector3.Lerp(orbRenderers[i].transform.position, targetPos, Time.deltaTime * 10f);
            }
        }
    }

    IEnumerator PopSpawnAnimation(Transform orbTransform)
    {
        float timer = 0f;
        orbTransform.localScale = Vector3.zero;

        while (timer < 1f)
        {
            timer += Time.deltaTime * 15f;
            float scale = Mathf.Lerp(0f, 1.2f, timer);
            if (timer >= 1f) scale = 1f;

            orbTransform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }

    // เอาไว้เรียกใช้ตอนยิงเวทออกไปแล้ว เพื่อล้างลูกแก้วรอบตัวทิ้ง
    public void ClearOrbs()
    {
        currentOrbs.Clear();
        UpdateOrbSprites();
    }

    // ==========================================
    // 🌟 ฟังก์ชันส่งรหัสผ่านไปเจาะเกราะมอนสเตอร์!
    // ==========================================
    public string GetCurrentRecipe()
    {
        // ถ้ายังกดไม่ครบ 3 ลูก จะยิงรหัสผ่านไม่ได้ (return string เปล่า)
        if (currentOrbs.Count < 3) return "";

        List<string> recipeList = new List<string>();

        // แปลงลูกแก้วแต่ละลูกเป็นคำ เช่น "Fire", "Water", "Wind"
        foreach (Element orb in currentOrbs)
        {
            recipeList.Add(orb.ToString());
        }

        // เรียงตัวอักษร A-Z (Fire > Water > Wind) เพื่อให้ตรงกับโค้ดฝั่งมอนสเตอร์เป๊ะๆ
        recipeList.Sort();

        // จับมัดรวมกันเป็นก้อนเดียว เช่น "FireWaterWind"
        return string.Join("", recipeList);
    }
}