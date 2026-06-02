using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    void Start()
    {
        // เกิดมาปุ๊บ นับถอยหลัง 3 วินาทีแล้วตายเองแน่นอน
        Destroy(gameObject, 0.5f);
    }
}
