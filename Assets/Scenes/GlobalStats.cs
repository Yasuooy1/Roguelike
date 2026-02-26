using UnityEngine;

public static class GlobalStats
{
    // สเตตัสเริ่มต้น (คุณอาร์มปรับเลขได้ตามใจชอบ)
    public static int BonusMaxHealth = 0; // เลือดที่อัปเพิ่มจากเดิม
    public static int BonusMaxMana = 0;   // มานาที่อัปเพิ่มจากเดิม
    public static float DamageMultiplier = 1f; // ตัวคูณความแรง (เช่น 1.2 คือแรงขึ้น 20%)

    public static int PlayerCoins = 0; // เงินที่เอาไว้ใช้อัปสเตตัส
}