using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    public static ArrowIndicator instance;

    [Header("การตั้งค่า")]
    // ❌ ไม่ต้องลากใส่แล้วครับ! ปล่อยทิ้งไว้ได้เลยเดี๋ยวโค้ดหาเอง
    public Transform player;
    public float radius = 2f;

    [Header("กราฟิกลูกศร")]
    public GameObject arrowVisual;

    private Transform currentTarget;

    void Awake()
    {
        instance = this;
        if (arrowVisual != null) arrowVisual.SetActive(false);
    }

    void Update()
    {
        // 🌟 ถ้ายิงเรดาร์แล้วยังหาผู้เล่นไม่เจอ (อาจจะยังไม่ถูกเสก) ให้รอก่อน
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform; // หาเจอแล้ว! ล็อกเป้าผู้เล่นเลย
            }
            else
            {
                return; // ถ้ายังหาไม่เจอ ให้หยุดการทำงานรอบนี้ไปก่อน
            }
        }

        // ถ้าไม่มีเป้าหมาย หรือเป้าหมายตาย/ถูกปิดไปแล้ว ให้ซ่อนลูกศร
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            if (arrowVisual.activeSelf) arrowVisual.SetActive(false);
            return;
        }

        if (!arrowVisual.activeSelf) arrowVisual.SetActive(true);

        // หาเวกเตอร์ทิศทางจาก ผู้เล่น -> เป้าหมาย
        Vector3 direction = currentTarget.position - player.position;
        direction.z = 0;

        // หมุนหัวลูกศรไปทางเป้าหมาย
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // ขยับตัวลูกศรให้ไปอยู่รอบๆ ตัวผู้เล่นตามรัศมีที่ตั้งไว้
        transform.position = player.position + direction.normalized * radius;
    }

    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }
}