using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpringConstraint : MonoBehaviour
{
    [SerializeField] Rigidbody goal;
    [SerializeField] Vector3 positionLocalOffset;
    [Min(0)] public float positionSmoothTime = .5f;
    [Min(0)] public float positionDampFactor = .5f;
    [SerializeField] Vector3 rotationLocalOffset;
    [Min(0)] public float rotationSmoothTime = .5f;
    [Min(0)] public float rotationDampFactor = .5f;
    [SerializeField] float gizmoRadius = .05f;
    Rigidbody rb;

    public Vector3 positionGoal
    {
        get => goal ? goal.rotation * positionLocalOffset + goal.position : positionLocalOffset;
    }

    public Quaternion rotationGoal
    {
        get => Quaternion.Euler(rotationLocalOffset) * (goal ? goal.rotation : Quaternion.identity);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Spring posSpring = new(positionSmoothTime, positionDampFactor, Time.deltaTime);
        Vector3 pos = rb.position;
        Vector3 vel = rb.linearVelocity;
        Vector3 acc = posSpring.Acceleration(pos, vel, positionGoal, default, Physics.gravity);
        rb.AddForce(acc, ForceMode.Acceleration);

        Spring rotSpring = new(rotationSmoothTime, rotationDampFactor, Time.deltaTime, 8);
        Quaternion rot = rb.rotation;
        Vector3 angVel = rb.angularVelocity;
        Vector3 rotAcc = rotSpring.Acceleration(rot, angVel, rotationGoal, default);
        rb.AddTorque(rotAcc, ForceMode.Acceleration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 posGoal = goal ? goal.rotation * positionLocalOffset + goal.position : positionLocalOffset;
        Quaternion rotGoal = Quaternion.Euler(rotationLocalOffset) * (goal ? goal.rotation : Quaternion.identity);
        Quaternion orientationRight = rotGoal * Quaternion.LookRotation(Vector3.right);
        DrawCircleGizmo(posGoal, orientationRight, gizmoRadius);
        Quaternion orientationUp = rotGoal * Quaternion.LookRotation(Vector3.up);
        DrawCircleGizmo(posGoal, orientationUp, gizmoRadius);
        Quaternion orientationForward = rotGoal * Quaternion.LookRotation(Vector3.forward);
        DrawCircleGizmo(posGoal, orientationForward, gizmoRadius);
    }

    static void DrawCircleGizmo(Vector3 center, Quaternion orientation, float radius, int segmentCount = 32)
    {
        segmentCount = Mathf.Clamp(segmentCount, 2, 256);
        Span<Vector3> points = stackalloc Vector3[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            float radians = t * Mathf.PI * 2;
            points[i] = center + orientation * new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * radius;
        }
        Gizmos.DrawLineStrip(points, looped: true);
    }
}