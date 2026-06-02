using UnityEngine;

public class HubRoomGuide : MonoBehaviour
{
    public static HubRoomGuide instance; // 🌟 ทำให้เรียกใช้จากสคริปต์อื่นง่ายๆ

    [Header("จุดสอนเล่น")]
    public Transform[] tutorialPoints;

    [Header("เป้าหมายอื่นๆ")]
    public Transform dummySpawner;
    public Transform shopNPC;
    public Transform exitDoor;

    void Awake() { instance = this; } // ตั้งค่าตัวตนสคริปต์

    void Start() { PointToSpecificStep(0); }

    public void PointToSpecificStep(int stepIndex)
    {
        if (ArrowIndicator.instance != null && stepIndex < tutorialPoints.Length)
            ArrowIndicator.instance.SetTarget(tutorialPoints[stepIndex]);
    }

    public void PointToDummy()
    {
        if (ArrowIndicator.instance != null && dummySpawner != null)
            ArrowIndicator.instance.SetTarget(dummySpawner);
    }

    public void PointToShop()
    {
        if (ArrowIndicator.instance != null && shopNPC != null)
            ArrowIndicator.instance.SetTarget(shopNPC);
    }

    public void PointToExit()
    {
        if (ArrowIndicator.instance != null && exitDoor != null)
            ArrowIndicator.instance.SetTarget(exitDoor);
    }

    public void TurnOffArrow()
    {
        if (ArrowIndicator.instance != null)
            ArrowIndicator.instance.ClearTarget();
    }
}