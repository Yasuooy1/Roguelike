using UnityEngine;
using Cinemachine;
using System.Collections;

public class BossCutscene : MonoBehaviour
{
    [Header("ใส่กล้อง 2 ตัว")]
    public CinemachineVirtualCamera playerCam; // กล้องผู้เล่น (Priority 10)
    public CinemachineVirtualCamera bossCam;   // กล้องบอส (Priority 9)

    [Header("เวลาแช่กล้องที่บอส")]
    public float timeToLookAtBoss = 3f;

    public void PlayCutscene(Transform targetBoss)
    {
        StartCoroutine(CutsceneRoutine(targetBoss));
    }

    IEnumerator CutsceneRoutine(Transform targetBoss)
    {
        // 1. เปิดสวิตช์กล้องบอส และตั้งเป้าหมาย
        bossCam.gameObject.SetActive(true);
        bossCam.Follow = targetBoss;

        // 2. สั่งกล้องบอสแย่งซีน (Priority 20)
        bossCam.Priority = 20;

        yield return new WaitForSeconds(timeToLookAtBoss);

        // 3. ลด Priority กลับ
        bossCam.Priority = 0;
        bossCam.Follow = null;

        // 4. ปิดสวิตช์กล้องบอสทิ้งไปเลย! (จบปัญหาแย่งซีน 100%)
        bossCam.gameObject.SetActive(false);
    }
}