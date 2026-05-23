using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponSlotDrop : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    public GameManager gameManager;

    public void OnDrop(PointerEventData eventData)
    {
        WeaponSlotUI draggedSlot = eventData.pointerDrag.GetComponent<WeaponSlotUI>();
        if (draggedSlot == null) return;

        int fromSlot = draggedSlot.slotIndex;
        int toSlot = slotIndex;

        if (fromSlot == toSlot) return;

        // 두 슬롯의 무기 인덱스 교환
        int temp = gameManager.player.weaponSlots[fromSlot];
        gameManager.player.weaponSlots[fromSlot] = gameManager.player.weaponSlots[toSlot];
        gameManager.player.weaponSlots[toSlot] = temp;

        // 현재 장착 중인 무기 슬롯 인덱스 업데이트
        if (gameManager.player.equipWeaponIndex == fromSlot)
            gameManager.player.equipWeaponIndex = toSlot;
        else if (gameManager.player.equipWeaponIndex == toSlot)
            gameManager.player.equipWeaponIndex = fromSlot;

        GameManager.Instance.UpdateWeaponSlots();

        Debug.Log($"슬롯 {fromSlot} ↔ 슬롯 {toSlot} 교환 완료");
    }




}