using UnityEngine;

public class UIFollowPlayer : MonoBehaviour
{
    private Transform player; // เปลี่ยนเป็น private เพราะเราจะหาเองในโค้ด

    [Header("ระยะความสูงเหนือหัว")]
    public Vector3 offset = new Vector3(0f, 2f, 0f);

    void Update()
    {
        // 1. ถ้ายังไม่มีตัวผู้เล่น ให้พยายามหาจาก Tag "Player"
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }

        // 2. ถ้าเจอตัวผู้เล่นแล้ว ให้ลอยตามทันที
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }
}