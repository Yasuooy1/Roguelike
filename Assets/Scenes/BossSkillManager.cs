using System.Collections;
using UnityEngine;

public class BossSkillManager : MonoBehaviour
{
    [Header("อนิเมเตอร์บอส")]
    public Animator bossAnim;

    [Header("แบล็คโฮลด้านหลัง (ลากออบเจกต์ลูกมาใส่)")]
    public GameObject backBlackhole;

    [Header("กระสุนแบล็คโฮล (ลาก Prefab มาใส่)")]
    public GameObject blackholeProjectilePrefab;

    [Header("จุดปล่อยกระสุน (Transform)")]
    public Transform firePoint;

    [Header("เวลาร่ายเวท (วินาที)")]
    public float castTime = 1.5f; // ปรับให้ตรงกับความยาวของอนิเมชันบอส

    void Start()
    {
        // เริ่มเกมมา ซ่อนแบล็คโฮลด้านหลังไว้ก่อน
        if (backBlackhole != null) backBlackhole.SetActive(false);
    }

    void Update()
    {
        // 🌟 เทสต์ระบบ: กดปุ่ม K เพื่อสั่งบอสปล่อยพลัง!
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(CastBlackholeSkill());
        }
    }

    // ระบบทำงานตามคิว (ร่าย -> โชว์รูข้างหลัง -> รอ -> ยิงกระสุน)
    IEnumerator CastBlackholeSkill()
    {
        Debug.Log("1. บอสเริ่มร่ายเวท!");
        bossAnim.SetTrigger("Cast");

        // หน่วงเวลาแป๊บนึง (0.2 วิ) ให้บอสยกมือก่อน แล้วค่อยเปิดแบล็คโฮลข้างหลัง
        yield return new WaitForSeconds(0.2f);

        Debug.Log("2. แบล็คโฮลด้านหลังค่อยๆ ปรากฏ!");
        if (backBlackhole != null) backBlackhole.SetActive(true);

        // ⏳ รอเวลาให้บอสร่ายจนจบ (ตามเวลา castTime ที่ตั้งไว้)
        yield return new WaitForSeconds(castTime - 0.2f);

        Debug.Log("3. ยิงลูกบอลแบล็คโฮลออกไป!");
        Instantiate(blackholeProjectilePrefab, firePoint.position, Quaternion.identity);

        // 4. เก็บแบล็คโฮลด้านหลัง หรือจะปล่อยทิ้งไว้อีกแป๊บแล้วค่อยปิดก็ได้
        if (backBlackhole != null) backBlackhole.SetActive(false);
    }
}