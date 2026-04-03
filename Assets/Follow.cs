using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform tatget;
    public Vector3 offset;

    void Update()
    {
        transform.position = tatget.position + offset;
    }























}
