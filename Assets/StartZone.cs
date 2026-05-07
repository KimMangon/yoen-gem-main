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
        if (other.gameObject.tag == "Player")
        {
            if (!manger.isBattle && !manger.isMapOpen && !isTriggered)
            {
                if (manger.currentNode != null && manger.currentNode.isCleared)
                {
                    manger.OpenMap();
                    return;
                }

                isTriggered = true;
                manger.isGameStarted = true;
                manger.NodeCleared(); // [변경] 모든 노드에서 NodeCleared 호출
            }
        }
    }



























}
