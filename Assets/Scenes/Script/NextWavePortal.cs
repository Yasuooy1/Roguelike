using UnityEngine;

public class NextWavePortal : MonoBehaviour
{
    public AudioClip warpSound; // เสียงตอนเดินเข้าวาร์ป (ถ้ามี)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (warpSound != null) AudioSource.PlayClipAtPoint(warpSound, transform.position);

            if (WaveManager.instance != null) WaveManager.instance.StartEndlessLoop();

            Destroy(gameObject);
        }
    }
}