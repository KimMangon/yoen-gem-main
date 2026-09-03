using UnityEngine;

public class RockSpin : MonoBehaviour
{
    public float spinSpeed = 480f; // 초당 회전 각도

    void Update()
    {
        transform.Rotate(Vector3.right * spinSpeed * Time.deltaTime);
    }
}
