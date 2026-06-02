using UnityEngine;

public class StartTrigger : MonoBehaviour
{
    void Start()
    {
        // พอเริ่มเกมปุ๊บ สั่งให้ GameManager สุ่มด่านแรกทันที
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadNextRandomMap();
        }
    }
}