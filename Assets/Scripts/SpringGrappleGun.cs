using UnityEngine;
using UnityEngine.InputSystem;

public class SpringGrappleGun : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform aim;
    [SerializeField] Transform center;
    [SerializeField, Min(0)] float maxDistance = 35f;
    [SerializeField, Min(0)] float retractAcceleration = 25f;
    [SerializeField, Min(0)] float retractSpeed = 15f;
    [SerializeField, Min(0)] float stopDistance = .5f;
    [SerializeField, Min(0)] float aimAssistDegrees = 5f;
    [SerializeField, Min(1)] float aimAssistSegmentLength = 10f;
    [SerializeField] float stretchSmoothTime = .2f;
    [SerializeField] float stretchDampFactor = 1f;
    [SerializeField] float maxStretchAcceleration = 100;
    [SerializeField] LayerMask layerMask = 1;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform nozzle;

    Rigidbody hitRb;
    Vector3 localAnchor;
    Vector3 tetherAcceleration;
    float fireInput;
    float retractInput;
    float currentRetractSpeed;
    float ropeLength;
    float anchorDistance;

    public bool isUsing { get; private set; }

    void FixedUpdate()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0) return;
        if (!isUsing && fireInput != 0)
        {
            if (TryRaycast(out RaycastHit hit))
            {
                isUsing = true;
                hitRb = hit.rigidbody;
                localAnchor = hitRb ? Quaternion.Inverse(hitRb.rotation) * (hit.point - hitRb.position) : hit.point;
                ropeLength = Vector3.Distance(center.position, hit.point) * 1.01f;
            }
        }
        if (fireInput == 0)
        {
            isUsing = false;
            ropeLength = 0;
            currentRetractSpeed = 0;
        }
        if (isUsing)
        {
            float retractSpeedGoal = Mathf.Lerp(0, retractSpeed, retractInput);
            currentRetractSpeed = Mathf.MoveTowards(currentRetractSpeed, retractSpeedGoal, retractAcceleration * deltaTime);
            ropeLength = Mathf.MoveTowards(ropeLength, stopDistance, currentRetractSpeed * deltaTime);
            Vector3 anchor = GetAnchorPoint();
            Vector3 position = rb.position;
            Vector3 velocity = rb.linearVelocity;
            Vector3 anchorOffset = anchor - position;
            anchorDistance = anchorOffset.magnitude;
            if (anchorDistance > ropeLength)
            {
                Vector3 anchorDirection = anchorDistance > 0 ? anchorOffset / anchorDistance : default;
                Spring spring = new(stretchSmoothTime, stretchDampFactor, deltaTime);
                float stretchSpeed = Vector3.Dot(velocity, anchorDirection);
                float accelerationMag = spring.Acceleration(-anchorDistance, stretchSpeed, -ropeLength, default);
                accelerationMag = Mathf.Clamp(accelerationMag, 0, maxStretchAcceleration);
                Vector3 acceleration = accelerationMag * anchorDirection;
                rb.AddForce(acceleration, ForceMode.Acceleration);
                if (hitRb)
                {
                    Vector3 force = -acceleration * rb.mass;
                    hitRb.AddForceAtPosition(force, anchor, ForceMode.Force);
                }
            }
        }
    }

    void LateUpdate()
    {
        lineRenderer.enabled = isUsing;
        lineRenderer.SetPosition(0, nozzle.position);
        lineRenderer.SetPosition(1, GetAnchorPoint());
    }

    public Vector3 GetAnchorPoint()
    {
        return hitRb ? hitRb.position + hitRb.rotation * localAnchor : localAnchor;
    }

    bool TryRaycast(out RaycastHit hit)
    {
        Ray ray = new(aim.position, aim.forward);
        float startDistance = 0f;
        while (startDistance < maxDistance)
        {
            float sweetSpotDistance = startDistance + aimAssistSegmentLength / 2;
            float radius = Mathf.Tan(aimAssistDegrees / 2 * Mathf.Deg2Rad) * sweetSpotDistance;
            Vector3 origin = ray.origin + ray.direction * startDistance;
            if (Physics.SphereCast(origin, radius, ray.direction, out hit, maxDistance, layerMask))
            {
                hit.distance = Vector3.Distance(hit.point, aim.position);
                return true;
            }
            startDistance += aimAssistSegmentLength;
        }
        hit = default;
        return false;
    }

    public void FireInput(InputAction.CallbackContext context)
    {
        fireInput = context.performed ? context.ReadValue<float>() : default;
    }

    public void RetractInput(InputAction.CallbackContext context)
    {
        retractInput = context.performed ? context.ReadValue<float>() : default;
    }
    
    void Reset()
    {
        lineRenderer = transform.root.GetComponentInChildren<LineRenderer>();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position, transform.position + tetherAcceleration);
    }
}
