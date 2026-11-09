using UnityEngine;

public sealed class DroneFollowCamera : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 5.5f, -12f);
    [SerializeField] float positionSharpness = 5f;
    [SerializeField] float lookAhead = 5f;
    [SerializeField, Range(0f,1f)] float fpvRoll = .82f;
    [SerializeField] float speedFovBoost = 15f;
    DroneFlightController flight;
    Camera cameraComponent;

    public void SetTarget(Transform value) { target = value; flight = value ? value.GetComponent<DroneFlightController>() : null; }

    void Awake() => cameraComponent = GetComponent<Camera>();

    void LateUpdate()
    {
        if (!target) return;
        Vector3 forward = target.forward;
        Vector3 desired = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-positionSharpness * Time.deltaTime));
        Vector3 lookAt = target.position + forward * lookAhead + Vector3.up * 1.2f;
        Quaternion rotation = Quaternion.LookRotation(lookAt - transform.position, Vector3.up) * Quaternion.Euler(0f,0f,target.eulerAngles.z*fpvRoll);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1f - Mathf.Exp(-positionSharpness * Time.deltaTime));
        if (cameraComponent && flight) cameraComponent.fieldOfView = Mathf.Lerp(76f,76f+speedFovBoost,Mathf.Clamp01(flight.Speed/38f));
    }
}
