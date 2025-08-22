using UnityEngine;

/// <summary>
/// Moves an object with a spring-like motion. This can be used for procedural animations or for physics
/// simulations. 
/// </summary>
public readonly struct Spring
{
    public readonly float stiffness;
    public readonly float damping;

    /// <summary> The delta time per iteration. </summary>
    public readonly float miniDeltaTime;

    /// <summary> The number of times this spring should update during the application's update. </summary>
    public readonly int iterations;

    /// <summary>
    /// </summary>
    /// <param name="frequency">How quickly the spring will return to rest position. This is loosely the number of
    /// oscillations per second, though it depends largely on dampFactor. This value will be clamped to be
    /// greater than or equal to 0.</param>
    /// <param name="dampFactor">How much damping is applied to the spring. This value will be clamped to be greater than or equal to 0.<br/>
    /// Undamped: A value of 0 will not apply any damping and allow the spring to oscillate forever.<br/>
    /// Under Damped: Values between 0 and 1 allow overshooting and oscillation.<br/>
    /// Critically Damped: A value of 1 makes the spring reach the rest position as fast as possible without overshooting. <br/>
    /// Over Damped: Values greater than 1 make the spring move slower and will not allow overshoot.<br/>
    /// </param>
    /// <param name="deltaTime">The duration of time since the application's last update. This value will be clamped to be
    /// greater than 0.</param>
    /// <param name="maxIterations">The maximum value that can be assigned to the iterations field.
    /// frequency and dampFactor will be adjusted to ensure the spring is stable with the given maxIterations.</param>
    public Spring(float smoothTime, float dampFactor, float deltaTime, int maxIterations = 1)
    {
        smoothTime = Mathf.Max(smoothTime, .00001f);
        float frequency = 2f / smoothTime;
        dampFactor = Mathf.Max(dampFactor, 0);
        deltaTime = Mathf.Max(deltaTime, .00001f);
        maxIterations = Mathf.Max(maxIterations, 1);
        float maxDeltaTime = frequency == 0 ? 1000 : 1f / Mathf.Max(frequency * dampFactor * 2, frequency);
        iterations = (int)Mathf.Ceil(deltaTime / maxDeltaTime);
        iterations = Mathf.Min(iterations, maxIterations);
        miniDeltaTime = deltaTime / iterations;
        stiffness = Mathf.Clamp(frequency * frequency, 0, 1f / (miniDeltaTime * miniDeltaTime));
        damping = Mathf.Clamp(frequency * dampFactor * 2, 0, 1f / miniDeltaTime);
    }

    public float Acceleration(
        float position,
        float velocity,
        float positionGoal,
        float velocityGoal,
        float gravity = default)
    {
        positionGoal += stiffness == 0 ? default : -gravity / stiffness;
        return -stiffness * (position - positionGoal) - damping * (velocity - velocityGoal);
    }

    public Vector2 Acceleration(
        Vector2 position,
        Vector2 velocity,
        Vector2 positionGoal,
        Vector2 velocityGoal,
        Vector2 gravity = default)
    {
        positionGoal += stiffness == 0 ? default : -gravity / stiffness;
        return -stiffness * (position - positionGoal) - damping * (velocity - velocityGoal);
    }

    public Vector3 Acceleration(
        Vector3 position,
        Vector3 velocity,
        Vector3 positionGoal,
        Vector3 velocityGoal,
        Vector3 gravity = default)
    {
        positionGoal += stiffness == 0 ? default : -gravity / stiffness;
        return -stiffness * (position - positionGoal) - damping * (velocity - velocityGoal);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="orientation"></param>
    /// <param name="angularVelocity">Angle axis in radians.</param>
    /// <param name="orientationGoal"></param>
    /// <param name="angularVelocityGoal">Angle axis in radians.</param>
    /// <returns></returns>
    public Vector3 Acceleration(
        Quaternion orientation,
        Vector3 angularVelocity,
        Quaternion orientationGoal,
        Vector3 angularVelocityGoal)
    {
        Vector3 angleAxisOffset = ScaledAngleAxis(SmallestDifference(orientation, orientationGoal));
        return -stiffness * angleAxisOffset - damping * (angularVelocity - angularVelocityGoal);
    }

    public void Update(
        ref float position,
        ref float velocity,
        float positionGoal,
        float velocityGoal,
        float gravity = default)
    {
        positionGoal += stiffness == 0 ? default : -gravity / stiffness;
        for (int i = 0; i < iterations; i++)
        {
            float acceleration = -stiffness * (position - positionGoal) - damping * (velocity - velocityGoal);
            velocity += acceleration * miniDeltaTime;
            position += velocity * miniDeltaTime;
        }
    }

    public void Update(
        ref Vector2 position,
        ref Vector2 velocity,
        Vector2 positionGoal,
        Vector2 velocityGoal,
        Vector2 gravity = default)
    {
        positionGoal += stiffness == 0 ? default : -gravity / stiffness;
        for (int i = 0; i < iterations; i++)
        {
            Vector2 acceleration = -stiffness * (position - positionGoal) - damping * (velocity - velocityGoal);
            velocity += acceleration * miniDeltaTime;
            position += velocity * miniDeltaTime;
        }
    }

    public void Update(
        ref Vector3 position,
        ref Vector3 velocity,
        Vector3 positionGoal,
        Vector3 velocityGoal,
        Vector3 gravity = default)
    {
        positionGoal += stiffness == 0 ? default : -gravity / stiffness;
        for (int i = 0; i < iterations; i++)
        {
            Vector3 acceleration = -stiffness * (position - positionGoal) - damping * (velocity - velocityGoal);
            velocity += acceleration * miniDeltaTime;
            position += velocity * miniDeltaTime;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="orientation"></param>
    /// <param name="angularVelocity">Angle axis in radians.</param>
    /// <param name="orientationGoal"></param>
    /// <param name="angularVelocityGoal">Angle axis in radians.</param>
    public void Update(
        ref Quaternion orientation,
        ref Vector3 angularVelocity,
        Quaternion orientationGoal,
        Vector3 angularVelocityGoal)
    {
        for (int i = 0; i < iterations; i++)
        {
            Quaternion quaternionOffset = SmallestDifference(orientation, orientationGoal);
            Vector3 angleAxisOffset = ScaledAngleAxis(quaternionOffset);
            Vector3 acceleration = -stiffness * angleAxisOffset - damping * (angularVelocity - angularVelocityGoal);
            angularVelocity += acceleration * miniDeltaTime;
            Vector3 angularOffset = angularVelocity * Mathf.Rad2Deg * miniDeltaTime;
            float angle = angularOffset.magnitude;
            orientation = Quaternion.AngleAxis(angle, angularOffset) * orientation;
        }
    }

    static Quaternion SmallestDifference(Quaternion a, Quaternion b)
    {
        if (Quaternion.Dot(a, b) < 0) b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
        return a * Quaternion.Inverse(b);
    }

    static Vector3 ScaledAngleAxis(Quaternion orientation)
    {
        orientation.ToAngleAxis(out float angle, out Vector3 axis);
        return axis * (angle * Mathf.Deg2Rad);
    }
}

// smoothTime = 2f / frequency
// frequency = 2f / smoothTime