using TraversalPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterRun))]
[RequireComponent(typeof(Jump))]
[RequireComponent(typeof(CharacterMotor))]
public class Crouch : MonoBehaviour
{
    [Header("General")]
    [SerializeField] Transform view;
    [SerializeField] Transform mesh;
    [SerializeField] float crouchHeight = 1f;
    [SerializeField] float smoothTime = .1f;
    [SerializeField] bool isToggle = false;

    [Header("Movement")]
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] float runSpeed = 4f;
    [SerializeField] bool canCrouchInAir = false;

    CharacterRun run;
    Jump jump;
    CharacterMotor motor;

    float capsuleStartHeight;
    float startRunSpeed;
    float startJumpHeight;
    float startViewHeight;

    float heightVelocity;
    float heightGoal;

    bool isCrouching;
    bool crouchInput;

    void Start()
    {
        motor = GetComponent<CharacterMotor>();
        run = GetComponent<CharacterRun>();
        jump = GetComponent<Jump>();
        capsuleStartHeight = motor.CapsuleCollider.height;
        startRunSpeed = run.runSpeed;
        startJumpHeight = jump.height;
        heightGoal = capsuleStartHeight;
        startViewHeight = view.position.y;
    }

    void FixedUpdate()
    {
        CapsuleCollider player = motor.CapsuleCollider;

        Capsule capsule = new(player);
        bool shouldCrouch = crouchInput && (motor.IsGrounded || canCrouchInAir) && !run.IsSprinting;
        if (!isCrouching && shouldCrouch)
        {
            CrouchDown();
        }

        float currentHeight = capsule.GetUpperTip().y - capsule.GetLowerTip().y;
        //float heightDiff = Mathf.Abs(heightGoal - currentHeight);

        //if (heightDiff > .01f) return;

        if (!shouldCrouch)
        {
            Ray ray = new(capsule.upper, Vector3.up);

            bool canStandUp = !Physics.SphereCast(
                ray,
                capsule.radius - .01f,
                //out RaycastHit hit,
                capsuleStartHeight - currentHeight + .01f,
                motor.layerMask,
                QueryTriggerInteraction.Ignore);

            if (canStandUp)
            {
                StandUp();
            }
            else
            {
                run.IsSprinting = false;
            }
        }

        player.height = Mathf.SmoothDamp(currentHeight, heightGoal, ref heightVelocity, smoothTime);
        Vector3 center = player.center;
        center.y = player.height / 2;
        player.center = center;
    }

    void LateUpdate()
    {
        Vector3 localPosition = view.localPosition;

        localPosition.y = startViewHeight
            / capsuleStartHeight
            * motor.CapsuleCollider.height;

        view.localPosition = localPosition;

        mesh.localScale = new Vector3(mesh.localScale.x, motor.CapsuleCollider.height / 2, mesh.localScale.z);
        localPosition = mesh.localPosition;
        localPosition.y = motor.CapsuleCollider.center.y;
        mesh.localPosition = localPosition;
    }

    void CrouchDown()
    {
        isCrouching = true;
        run.runSpeed = runSpeed;
        jump.height = jumpHeight;
        heightGoal = crouchHeight;
    }

    void StandUp()
    {
        isCrouching = false;
        run.runSpeed = startRunSpeed;
        jump.height = startJumpHeight;
        heightGoal = capsuleStartHeight;
    }

    public void CrouchInput(InputAction.CallbackContext context)
    {
        crouchInput = isToggle
            ? (context.performed ? !crouchInput : crouchInput)
            : context.performed;
    }
}
