using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private SpriteRenderer glowSprite;
    private Vector3 startScale;
    private Vector3 startPos;

    [Header("🌬️ ความจางของแสง (Alpha)")]
    public float minAlpha = 0.3f;
    public float maxAlpha = 0.8f;
    public float alphaSpeed = 3f; // ความเร็วลมพัดให้แสงวูบ

    [Header("🔥 การยืดหดของเปลวไฟ (Scale)")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float scaleSpeed = 2.5f;

    [Header("💨 การส่ายตามลม (Sway)")]
    public float swayAmount = 0.15f; // ระยะที่แสงส่ายซ้าย-ขวา
    public float swaySpeed = 2f;

    private float randomSeed;

    void Start()
    {
        glowSprite = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
        startPos = transform.localPosition;

        // สุ่ม Seed ป้องกันไม่ให้คบไฟทุกอันในฉากแกว่งพร้อมกันเป๊ะๆ (ถ้าก๊อปวางหลายอัน)
        randomSeed = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (glowSprite == null) return;

        // 1. แสงกระพริบพริ้วๆ สมูทๆ (เปลี่ยน Alpha)
        float noiseAlpha = Mathf.PerlinNoise(Time.time * alphaSpeed + randomSeed, 0f);
        Color c = glowSprite.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, noiseAlpha);
        glowSprite.color = c;

        // 2. ขนาดวูบวาบเหมือนไฟเต้นรำ (เปลี่ยน Scale)
        float noiseScale = Mathf.PerlinNoise(0f, Time.time * scaleSpeed + randomSeed);
        float currentScale = Mathf.Lerp(minScale, maxScale, noiseScale);
        transform.localScale = startScale * currentScale;

        // 3. แสงแกว่งซ้ายขวาตามลมพัด (เปลี่ยน Position X)
        float noisePos = Mathf.PerlinNoise(Time.time * swaySpeed + randomSeed, 100f);
        float sway = Mathf.Lerp(-swayAmount, swayAmount, noisePos);
        transform.localPosition = startPos + new Vector3(sway, 0f, 0f);
    }
}