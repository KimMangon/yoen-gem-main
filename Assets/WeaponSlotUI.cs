using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int slotIndex;           // 이 슬롯이 몇 번 슬롯인지
    public Image slotImage;         // 슬롯 이미지 컴포넌트
    public GameManager gameManager;

    private Transform originalParent;
    private Vector3 originalPos;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {

        originalParent = transform.parent;
        originalPos = transform.localPosition;

        // Canvas 최상단으로 이동 (다른 UI 위에 보이게)
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false; // 드래그 중 레이캐스트 무시
    }

    // 드래그 중
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    // 드래그 끝
    public void OnEndDrag(PointerEventData eventData)
    {

        // 원래 자리로 복귀
        transform.SetParent(originalParent);
        transform.localPosition = originalPos;
        canvasGroup.blocksRaycasts = true;
    }
}