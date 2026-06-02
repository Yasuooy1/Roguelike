using UnityEngine;

public class Soul : MonoBehaviour
{
    public int soulValue = 1;
    public float attractSpeed = 5f;
    private Transform player;
    private bool isFollowing = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        GetComponent<Rigidbody2D>()?.AddForce(Random.insideUnitCircle * 5f, ForceMode2D.Impulse);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < 3f) isFollowing = true;

        if (isFollowing)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, attractSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 🌟 แก้ชื่อคีย์เป็น Player_Souls ให้ตรงกับหน้า UpgradeManager
            int currentSouls = PlayerPrefs.GetInt("Player_Souls", 0);
            PlayerPrefs.SetInt("Player_Souls", currentSouls + soulValue);
            PlayerPrefs.Save(); // บังคับเซฟลงเครื่อง

            Debug.Log("เก็บ Soul ได้! ตอนนี้มีทั้งหมด: " + PlayerPrefs.GetInt("Player_Souls"));

            Destroy(gameObject);
        }
    }
}