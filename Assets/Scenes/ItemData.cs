using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public Sprite itemIcon;
    public int amount = 1;
    public float cooldownTime = 2f;
}