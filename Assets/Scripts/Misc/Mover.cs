using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] Vector3 speed;

    void Update()
    {
        transform.position += speed * Time.deltaTime;
    }

}
