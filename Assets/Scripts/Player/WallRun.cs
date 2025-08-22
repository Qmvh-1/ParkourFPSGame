using TraversalPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(AirControl))]
public class WallRun : MonoBehaviour
{
    [Header("General")]
    [SerializeField] Transform checkPos;
    [SerializeField, Range(0f, 1f)] float gravityPercentage = .3f;
    [SerializeField] float raycastDistance = 1f;
    [SerializeField] LayerMask layerMask = 1;

    [Header("Movement")]
    [SerializeField] float speed = 10f;
    [SerializeField] float acceleration = 50f;
    [SerializeField] float maxVerticalSpeed = 10f;
    [SerializeField] float wallJumpDistance = 8f;
    [SerializeField] float wallJumpDegrees = 35f;
    [SerializeField] float wallJumpDuration = .5f;

    [Header("Starting")]
    [SerializeField] float startUpVelocityBoost = 5f;
    [SerializeField] float minStartSpeed = 5f;

    [Header("Camera")]
    [SerializeField] Transform camRoll;
    [SerializeField] float camRollSmoothTime = .3f;
    [SerializeField] float camRollDegrees = 15f;

    AirControl airControl;
    CharacterMotor motor;

    float camRollValue;
    float camRollVelocity;
    
    float defaultInAirSpeed;
    float defaultInAirMaxControlSpeed;
    float defaultInAirAcceleration;

    bool touchingLeftWall;
    bool touchingRightWall;
    bool isWallRunning;
    bool wallJumpInput;

    Rigidbody rb => motor.Rigidbody;

    void Awake()
    {
        motor = GetComponent<CharacterMotor>();
        airControl = GetComponent<AirControl>();

        defaultInAirSpeed = airControl.speed;
        defaultInAirMaxControlSpeed = airControl.maxControllableSpeed;
        defaultInAirAcceleration = airControl.acceleration;
    }

    void FixedUpdate()
    {
        touchingLeftWall = Physics.Raycast(checkPos.position, -checkPos.right, out RaycastHit leftHit, layerMask);
        touchingRightWall = Physics.Raycast(checkPos.position, checkPos.right, out RaycastHit rightHit, layerMask);

        RaycastHit wallHit = touchingLeftWall ? leftHit : rightHit;
        bool wasWallRunning = isWallRunning;
        isWallRunning = (touchingLeftWall || touchingRightWall) && !motor.IsGrounded;

        if (isWallRunning && !wasWallRunning)
        {
            isWallRunning = motor.LocalVelocity.XZ().magnitude >= minStartSpeed;
        }

        if (wallJumpInput)
        {
            wallJumpInput = false;
            if (isWallRunning)
            {
                WallJump(wallHit);
            }
        }

        if (!isWallRunning)
        {
            if (wasWallRunning)
            {
                airControl.speed = defaultInAirSpeed;
                airControl.maxControllableSpeed = defaultInAirMaxControlSpeed;
                airControl.acceleration = defaultInAirAcceleration;
            }
            return;
        }

        if (!wasWallRunning)
        {
            StartWallRun();
        }

        Vector3 counterGravity = -Physics.gravity * (1 - gravityPercentage);

        float yDrag = Utility.DragAcceleration(
            rb.linearVelocity.y,
            maxVerticalSpeed,
            Physics.gravity.y + counterGravity.y);

        Vector3 drag = new(0, yDrag * -Mathf.Sign(rb.linearVelocity.y), 0);
        rb.AddForce(counterGravity + drag, ForceMode.Acceleration);

    }

    void StartWallRun()
    {
        airControl.speed = speed;
        airControl.maxControllableSpeed = speed * 1.5f;
        airControl.acceleration = acceleration;
        rb.AddForce(Vector3.up * startUpVelocityBoost, ForceMode.VelocityChange);
    }

    void LateUpdate()
    {
        float camRollValueGoal = isWallRunning ? (touchingLeftWall ? -camRollDegrees : camRollDegrees) : 0;
        camRollValue = Mathf.SmoothDamp(camRollValue, camRollValueGoal, ref camRollVelocity, camRollSmoothTime);
        camRoll.localEulerAngles = new Vector3(0, 0, camRollValue);
    }

    void WallJump(RaycastHit wallHit)
    {
        Vector3 crossAxis = Vector3.Cross(Vector3.up, wallHit.normal).normalized;
        Vector3 flatJumpDir = -Vector3.Cross(Vector3.up, crossAxis).normalized;
        Vector3 jumpDir = Quaternion.AngleAxis(-wallJumpDegrees, crossAxis) * flatJumpDir;
        Vector3 goalOffset = jumpDir * wallJumpDistance;
        Vector3 initialVelocity = -.5f * wallJumpDuration * Physics.gravity + goalOffset / wallJumpDuration;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0;
        velocity += initialVelocity;
        rb.linearVelocity = velocity;
    }

    public void WallJumpInput(InputAction.CallbackContext context)
    {
        wallJumpInput = context.performed;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(
            checkPos.position - checkPos.right * raycastDistance,
            checkPos.position + checkPos.right * raycastDistance);
    }
}
