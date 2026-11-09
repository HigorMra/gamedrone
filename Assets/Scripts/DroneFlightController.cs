using UnityEngine;

/// <summary>Fast, velocity-aware FPV flight with mouse waypoint steering.</summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class DroneFlightController : MonoBehaviour
{
    [Header("Flight feel")]
    [SerializeField] float acceleration = 34f;
    [SerializeField] float lateralAcceleration = 42f;
    [SerializeField] float maxSpeed = 38f;
    [SerializeField] float boostMultiplier = 1.55f;
    [SerializeField] float airDrag = 0.22f;
    [SerializeField] float brakeDrag = 4.8f;
    [SerializeField] float waypointRadius = 2.2f;
    [SerializeField] float cruiseHeight = 8f;
    [SerializeField] float verticalSpeed = 13f;
    [Header("References")]
    [SerializeField] Camera flightCamera;
    [SerializeField] LayerMask groundMask = 1 << 8;
    [SerializeField] LayerMask obstacleMask = ~(1 << 2);

    Rigidbody body;
    Vector3 velocity;
    Vector3? waypoint;
    float targetHeight;
    float yaw;
    public float Speed => velocity.magnitude;
    public float Altitude => transform.position.y;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        targetHeight = cruiseHeight;
        yaw = transform.eulerAngles.y;
        if (!flightCamera) flightCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ResetFlight();
        if (Input.GetMouseButtonDown(0)) SetWaypointFromPointer();
        targetHeight = Mathf.Clamp(targetHeight +
            ((Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f)) * verticalSpeed * Time.deltaTime, 3f, 55f);
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        float forward = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) -
                        (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
        float strafe = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) -
                       (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
        float boost = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? boostMultiplier : 1f;
        Vector3 planarAcceleration = Vector3.zero;

        if (waypoint.HasValue)
        {
            Vector3 delta = waypoint.Value - transform.position;
            delta.y = 0f;
            float distance = delta.magnitude;
            if (distance <= waypointRadius) waypoint = null;
            else
            {
                // Brake early using stopping distance; a new click immediately replaces this vector.
                float desiredSpeed = Mathf.Min(maxSpeed * boost, Mathf.Sqrt(2f * acceleration * Mathf.Max(0f, distance - waypointRadius)));
                Vector3 desiredVelocity = delta.normalized * desiredSpeed;
                planarAcceleration = Vector3.ClampMagnitude((desiredVelocity - Horizontal(velocity)) * 3.4f, lateralAcceleration * boost);
            }
        }
        else
        {
            Vector3 forwardAxis = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            Vector3 rightAxis = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
            planarAcceleration = forwardAxis * forward * acceleration + rightAxis * strafe * lateralAcceleration;
            if (planarAcceleration.sqrMagnitude > acceleration * acceleration)
                planarAcceleration = planarAcceleration.normalized * acceleration;
        }

        velocity += planarAcceleration * dt;
        float drag = planarAcceleration.sqrMagnitude > 0.01f ? airDrag : brakeDrag;
        velocity *= Mathf.Exp(-drag * dt);
        Vector3 planar = Horizontal(velocity);
        planar = Vector3.ClampMagnitude(planar, maxSpeed * boost);
        velocity.x = planar.x; velocity.z = planar.z;

        float verticalError = targetHeight - transform.position.y;
        float verticalVelocity = Mathf.Clamp(verticalError * 2.2f, -verticalSpeed, verticalSpeed);
        velocity.y = Mathf.Lerp(velocity.y, verticalVelocity, 1f - Mathf.Exp(-4f * dt));
        Vector3 displacement = velocity * dt;
        if (displacement.sqrMagnitude > 0.0001f && Physics.SphereCast(body.position, .32f, displacement.normalized, out RaycastHit obstacle, displacement.magnitude, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            body.MovePosition(body.position + displacement.normalized * Mathf.Max(0f, obstacle.distance - .36f));
            velocity = Vector3.ProjectOnPlane(velocity, obstacle.normal) * .18f;
            waypoint = null;
        }
        else body.MovePosition(body.position + displacement);

        Vector3 facing = waypoint.HasValue ? Horizontal(waypoint.Value - transform.position) : planar;
        if (facing.sqrMagnitude > 0.16f)
        {
            float targetYaw = Quaternion.LookRotation(facing, Vector3.up).eulerAngles.y;
            yaw = Mathf.LerpAngle(yaw, targetYaw, 1f - Mathf.Exp(-8f * dt));
        }
        float bank = Mathf.Clamp(-strafe * 19f - Vector3.Dot(planar.normalized, transform.right) * Mathf.Min(Speed, 15f), -32f, 32f);
        Quaternion attitude = Quaternion.Euler(Mathf.Clamp(-forward * 9f, -12f, 12f), yaw, bank);
        body.MoveRotation(Quaternion.Slerp(body.rotation, attitude, 1f - Mathf.Exp(-8f * dt)));
    }

    void SetWaypointFromPointer()
    {
        if (!flightCamera) return;
        Ray ray = flightCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask, QueryTriggerInteraction.Ignore))
        {
            waypoint = hit.point;
            waypoint = new Vector3(waypoint.Value.x, targetHeight, waypoint.Value.z);
        }
        else
        {
            Plane plane = new Plane(Vector3.up, new Vector3(0f, targetHeight, 0f));
            if (plane.Raycast(ray, out float enter)) waypoint = ray.GetPoint(enter);
        }
    }

    static Vector3 Horizontal(Vector3 v) => new Vector3(v.x, 0f, v.z);

    void ResetFlight()
    {
        velocity = Vector3.zero;
        waypoint = null;
        targetHeight = cruiseHeight;
        yaw = 0f;
        body.position = new Vector3(0f, cruiseHeight, -20f);
        body.rotation = Quaternion.identity;
    }
}
