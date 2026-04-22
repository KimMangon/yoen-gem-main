using UnityEngine;

public class StartZone : MonoBehaviour
{
    public GameManager manger;
    private bool isTriggered = false;

    void OnEnable()
    {
        isTriggered = false; // [추가] 활성화될 때마다 초기화
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if (!manger.isBattle && !manger.isMapOpen && !isTriggered)
            {
                isTriggered = true;
                manger.isGameStarted = true;
                manger.NodeCleared();
            }
                
        }
    }



























}
