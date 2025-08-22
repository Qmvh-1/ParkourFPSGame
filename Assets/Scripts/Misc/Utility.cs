using Unity.Mathematics;
using UnityEngine;

public static class Utility
{
    public static float DragAcceleration(float speed, float terminalSpeed, float gravity)
    {
        // terminalVelocity = sqrt((2 * mass * gravity) / (dragCoefficient * airDensity * referenceArea))
        // dragForce = (dragCoefficient * airDensity * referenceArea * fluidSpeed * fluidSpeed) / 2
        terminalSpeed = Mathf.Max(terminalSpeed, .001f);
        double terminalSpeedFraction = (double)speed / terminalSpeed;
        return Mathf.Abs((float)(gravity * terminalSpeedFraction * terminalSpeedFraction));
    }

    public static Vector2 XZ(this Vector3 value)
    {
        return new Vector2(value.x, value.z);
    }

    public static void SetXZ(this ref Vector3 value, Vector2 xz)
    {
        value.x = xz.x;
        value.z = xz.y;
    }

    public static int3 ToCrds(int index, int3 cellCounts)
    {
        return new int3(
            index % cellCounts.x,
            index % (cellCounts.x * cellCounts.y) / cellCounts.x,
            index / (cellCounts.x * cellCounts.y));
    }

    public static int ToIndex(int3 crds, int3 cellCounts)
    {
        return crds.x + crds.y * cellCounts.x + crds.z * cellCounts.y * cellCounts.x;
    }
}
