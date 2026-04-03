using UnityEngine;

public class StartZone : MonoBehaviour
{
    public GameManager manger;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            manger.StageStart();
        }
    }



























}
