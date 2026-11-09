using UnityEngine;

public sealed class DroneRotorSpin : MonoBehaviour
{
    [SerializeField] float rotationsPerSecond = 85f;
    [SerializeField] bool reverse;
    public void SetReverse(bool value) => reverse = value;
    void Update() => transform.Rotate(0f, (reverse ? -1f : 1f) * rotationsPerSecond * 360f * Time.deltaTime, 0f, Space.Self);
}
