using UnityEngine;
using static TraversalPro.Utility;

// TP mod - created this script
namespace TraversalPro
{
    [RequireComponent(typeof(CharacterMotor))]
    public class LedgeClimb : MonoBehaviour
    {
        [SerializeField] Transform view;
        [SerializeField, Min(0)] float minHeight = .15f;
        [SerializeField, Min(0)] float maxHeight = 1.8f;
        [SerializeField, Min(0)] float viewToleranceDegrees = 65f;
        [SerializeField, Min(0)] float inputToleranceDegrees = 30f;
        [SerializeField, Min(0)] float smoothTime = .2f;
        [SerializeField, Min(0)] float dampFactor = 1;
        const float distanceToleranceFactor = 1.5f;
        const float failureTimer = 1f;
        CharacterMotor motor;
        Rigidbody rb;
        Capsule capsule;
        Vector3 viewFlatForward;
        Vector3 flatInput;
        bool hasValidContact;
        Rigidbody ledgeRb;
        Vector3 localGoal;
        bool hasGoal;
        float nearestDistanceSoFar;
        float currentFailureTimer;
        readonly RaycastHit[] hits = new RaycastHit[16];

        void Awake()
        {
            motor = GetComponent<CharacterMotor>();
            rb = motor.Rigidbody;
        }

        void FixedUpdate()
        {
            capsule = new Capsule(motor.CapsuleCollider);
            viewFlatForward = view.forward;
            viewFlatForward.y = 0;
            viewFlatForward.Normalize();
            flatInput = new Vector3(motor.MoveInput.x, 0, motor.MoveInput.z);
            if (!hasGoal)
            {
                hasGoal = hasValidContact && TryFindLedgeGoal(out ledgeRb, out localGoal);
                nearestDistanceSoFar = float.PositiveInfinity;
                currentFailureTimer = failureTimer;
            }
            hasValidContact = false;
            if (hasGoal)
            {
                hasGoal = TryMoveTowardGoal();
            }
        }

        void OnCollisionEnter(Collision other)
        {
            if (HasValidContact(other))
            {
                hasValidContact = true;
            }
        }

        void OnCollisionStay(Collision other)
        {
            if (HasValidContact(other))
            {
                hasValidContact = true;
            }
        }

        bool TryFindLedgeGoal(out Rigidbody ledgeRb, out Vector3 localGoal)
        {
            Capsule cap = capsule;
            cap.radius *= .9f;
            RaycastHit upHit = Capsulecast(cap, Vector3.up, hits, maxHeight, motor.layerMask, motor.IgnoredColliderIds);
            if (!upHit.IsValid())
            {
                cap.Center += Vector3.up * maxHeight;
                Vector3 direction = flatInput.normalized;
                RaycastHit forwardHit = Capsulecast(cap, direction, hits, cap.radius * 1.5f, motor.layerMask, motor.IgnoredColliderIds);
                if (!forwardHit.IsValid())
                {
                    cap.Center += direction * cap.radius * 1.5f;
                    RaycastHit downHit = Capsulecast(cap, Vector3.down, hits, maxHeight - minHeight, motor.layerMask, motor.IgnoredColliderIds);
                    if (downHit.IsValid())
                    {
                        float slopeDegrees = Vector3.Angle(Vector3.up, downHit.normal);
                        if (slopeDegrees < motor.steepSlopeThresholdDegrees)
                        {
                            ledgeRb = downHit.rigidbody;
                            localGoal = ledgeRb 
                                ? Quaternion.Inverse(ledgeRb.rotation) * (downHit.point - ledgeRb.position) 
                                : downHit.point;
                            return true;
                        }
                    }
                }
            }
            ledgeRb = null;
            localGoal = default;
            return false;
        }

        bool HasValidContact(Collision other)
        {
            for (int i = 0; i < other.contactCount; i++)
            {
                Vector3 point = other.GetContact(i).point;
                Vector3 offset = point - capsule.Center;
                offset.y = 0;
                if (offset.magnitude > capsule.radius * .5f 
                    && Vector3.Angle(offset, viewFlatForward) < viewToleranceDegrees
                    && Vector3.Angle(offset, flatInput) < inputToleranceDegrees)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <returns>Whether body has made progress towards goal and goal is still valid.</returns>
        bool TryMoveTowardGoal()
        {
            Vector3 goal = GetGoal();
            Vector3 position = capsule.GetLowerTip();
            
            // check failure timer
            Vector3 offset = goal - position;
            float distance = offset.magnitude;
            if (distance > nearestDistanceSoFar)
            {
                currentFailureTimer -= Time.deltaTime;
                if (currentFailureTimer < 0)
                {
                    return false;
                }
            }
            else
            {
                currentFailureTimer += Time.deltaTime;
                currentFailureTimer = Mathf.Min(currentFailureTimer, failureTimer);
            }
            
            // check if distance is too far
            if (distance > nearestDistanceSoFar * distanceToleranceFactor)
            {
                return false;
            }
            nearestDistanceSoFar = Mathf.Min(nearestDistanceSoFar, distance);
            
            // check for success
            if (distance < capsule.radius * .5f)
            {
                return false;
            }
            
            // check input
            Vector3 flatOffset = new(offset.x, 0, offset.z);
            if (flatInput.sqrMagnitude < .01f)
            {
                return false;
            }
            if (Vector3.Angle(flatInput, flatOffset) > inputToleranceDegrees)
            {
                return false;
            }
            
            // check view
            if (Vector3.Angle(viewFlatForward, flatOffset) > viewToleranceDegrees)
            {
                return false;
            }
            
            // apply acceleration
            Spring spring = new(smoothTime, dampFactor, Time.deltaTime);
            Vector3 acceleration = spring.Acceleration(
                position, 
                rb.linearVelocity,
                goal, 
                default, 
                GetGravity(rb, motor.freeFall.Value));
            acceleration.y = Mathf.Max(acceleration.y, 0);
            rb.AddForce(acceleration, ForceMode.Acceleration);
            // motor.AccelerationGoal = 0;
            return true;
        }

        Vector3 GetGoal()
        {
            return ledgeRb 
                ? ledgeRb.rotation * localGoal + ledgeRb.position 
                : localGoal;
        }

        void Reset()
        {
            ViewControl viewControl = transform.root.GetComponentInChildren<ViewControl>();
            if (viewControl)
            {
                view = viewControl.transform;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (hasGoal)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(GetGoal(), .1f);
            }
            Gizmos.color = Color.green;
            Vector3 minPos = transform.position + Vector3.up * minHeight;
            Vector3 maxPos = transform.position + Vector3.up * maxHeight;
            Gizmos.DrawSphere(minPos, .1f);
            Gizmos.DrawSphere(maxPos, .1f);
            Gizmos.DrawLine(minPos, maxPos);
        }
    }
}