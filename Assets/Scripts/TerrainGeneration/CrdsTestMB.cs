using UnityEngine;
using Unity.Mathematics;

public class CrdsTestMB : MonoBehaviour
{
    [SerializeField] int3 cellCounts;
    [SerializeField] int inputIndex;
    [SerializeField] int3 outPutCrds;
    [SerializeField] int outputIndex;

    void OnValidate()
    {
        outPutCrds = ToCrds(inputIndex, cellCounts);
        outputIndex = ToIndex(outPutCrds, cellCounts);
    }

    static int3 ToCrds(int index, int3 cellCounts)
    {
        return new int3(
            index % cellCounts.x,
            index % (cellCounts.x * cellCounts.y) / cellCounts.y,
            index / (cellCounts.x * cellCounts.y));
    }

    static int ToIndex(int3 crds, int3 cellCounts)
    {
        return crds.x + crds.y * cellCounts.x + crds.z * cellCounts.y * cellCounts.x;
    }
}
