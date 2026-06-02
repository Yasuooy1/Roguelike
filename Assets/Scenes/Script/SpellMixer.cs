using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellMixer : MonoBehaviour
{
    public enum Element { Fire, Water, Lightning }

    [Header("ลูกแก้วที่กำลังผสมอยู่")]
    public List<Element> currentOrbs = new List<Element>();

    [Header("ออบเจกต์ลูกแก้วในฉาก")]
    public SpriteRenderer[] orbRenderers;
    // 🌟 1. เพิ่มช่องใส่ Animator เพื่อบังคับให้ภาพมันขยับ
    public Animator[] orbAnimators;

    [Header("ไฟล์อนิเมชันของแต่ละธาตุ")]
    // 🌟 2. ใช้ RuntimeAnimatorController แทน Sprite ธรรมดา
    public RuntimeAnimatorController fireAnimController;
    public RuntimeAnimatorController waterAnimController;
    public RuntimeAnimatorController lightningAnimController;

    [Header("ตำแหน่งการลอยรอบตัว (แบบ Invoker)")]
    public Vector3[] orbOffsets = new Vector3[] {
        new Vector3(-1.2f, 1f, 0f),  // ลูกที่ 1: ไหล่ซ้าย
        new Vector3(0f, 1.8f, 0f),   // ลูกที่ 2: เหนือหัว
        new Vector3(1.2f, 1f, 0f)    // ลูกที่ 3: ไหล่ขวา
    };
    public float hoverSpeed = 3f;
    public float hoverHeight = 0.2f;

    void Start()
    {
        UpdateOrbSprites();
    }

    void Update()
    {
        AnimateOrbs();
    }

    public void AddOrb(Element newOrb)
    {
        if (currentOrbs.Count >= 3)
        {
            currentOrbs.RemoveAt(0);
        }

        currentOrbs.Add(newOrb);
        UpdateOrbSprites();

        // 🌟 3. สั่งเล่นเอฟเฟกต์ "เด้งป๊อปอัป" เฉพาะลูกแก้วที่เพิ่งเกิดใหม่
        int newOrbIndex = currentOrbs.Count - 1;
        StartCoroutine(PopSpawnAnimation(orbRenderers[newOrbIndex].transform));
    }

    void UpdateOrbSprites()
    {
        for (int i = 0; i < orbRenderers.Length; i++)
        {
            if (i < currentOrbs.Count)
            {
                orbRenderers[i].enabled = true;

                // 🌟 4. สลับไฟล์อนิเมชันตามธาตุ (ไฟจะลุก น้ำจะเดือดตามรูปที่ทำมาเลย!)
                if (orbAnimators.Length > i && orbAnimators[i] != null)
                {
                    if (currentOrbs[i] == Element.Fire) orbAnimators[i].runtimeAnimatorController = fireAnimController;
                    else if (currentOrbs[i] == Element.Water) orbAnimators[i].runtimeAnimatorController = waterAnimController;
                    else if (currentOrbs[i] == Element.Lightning) orbAnimators[i].runtimeAnimatorController = lightningAnimController;
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
        for (int i = 0; i < orbRenderers.Length; i++)
        {
            if (orbRenderers[i].enabled)
            {
                float floatOffset = Mathf.Sin(Time.time * hoverSpeed + (i * 1.5f)) * hoverHeight;
                Vector3 targetPos = transform.position + orbOffsets[i] + new Vector3(0, floatOffset, 0);
                orbRenderers[i].transform.position = Vector3.Lerp(orbRenderers[i].transform.position, targetPos, Time.deltaTime * 10f);
            }
        }
    }

    // ==========================================
    // 🌟 ฟังก์ชันทำเอฟเฟกต์ตอนกดลูกแก้ว (ขยายตัวเด้งดึ๋ง)
    // ==========================================
    IEnumerator PopSpawnAnimation(Transform orbTransform)
    {
        float timer = 0f;
        orbTransform.localScale = Vector3.zero; // เริ่มต้นที่ขนาด 0 (มองไม่เห็น)

        while (timer < 1f)
        {
            timer += Time.deltaTime * 15f; // ความเร็วในการเด้งโผล่ขึ้นมา

            // ขยายเกินขนาดจริงนิดนึง (1.2f) แล้วค่อยหดกลับมา 1f เพื่อให้ดูมีน้ำหนักกระแทก
            float scale = Mathf.Lerp(0f, 1.2f, timer);
            if (timer >= 1f) scale = 1f;

            orbTransform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }

    public void ClearOrbs()
    {
        currentOrbs.Clear();
        UpdateOrbSprites();
    }

    public string GetCurrentRecipe()
    {
        if (currentOrbs.Count < 3) return "";
        List<string> recipeList = new List<string>();
        foreach (Element orb in currentOrbs) recipeList.Add(orb.ToString());
        recipeList.Sort();
        return string.Join("", recipeList);
    }
}