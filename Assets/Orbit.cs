using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform Target;
    public float orbitSpeed;
    Vector3 offSet;













    void Start()
    {
        offSet = transform.position - Target.position;
    }

    
    void Update()
    {
        transform.position = Target.position + offSet;
        transform.RotateAround(Target.position,Vector3.up,orbitSpeed * Time.deltaTime);

        offSet = transform.position - Target.position;
    }



















}
