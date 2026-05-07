using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Blacksmith : MonoBehaviour
{
    public RectTransform uiGroup;
    public Text talkText;

    public Text[] recipeTexts;      // 레시피 텍스트 (가격 텍스트 재활용)
    public GameObject[] recipeButtons;

    public GameObject[] recipeItemObjs;
    public Transform[] itemPos;

    Player enterPlayer;

    int[] recipeCost = new int[] { 5, 0, 0, 5, 0 };
    int recipeResult = 4;

    public void Enter(Player player)
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero;
        UpdateRecipeUI();
    }

    public void Exit()
    {
        uiGroup.anchoredPosition = Vector3.down * 1000;
    }

    void UpdateRecipeUI()
    {
        // talkText에 보유량 표시
        talkText.text =
            $"<color=#FF8C00>망치 x{enterPlayer.weaponPickCount[0]}</color>  " +
            $"<color=#00CC00>핸드건 x{enterPlayer.weaponPickCount[1]}</color>  " +
            $"<color=#0099FF>머신건 x{enterPlayer.weaponPickCount[2]}</color>  " +
            $"<color=#8B4513>방패 x{enterPlayer.weaponPickCount[3]}</color>";

        // 레시피 텍스트
        recipeTexts[0].text =
            $"<color=#FF8C00>{enterPlayer.weaponPickCount[0]}/5</color>  " +
            $"<color=#8B4513>{enterPlayer.weaponPickCount[3]}/5</color>";

        recipeTexts[1].text =
        $"<color=#00CC00>{enterPlayer.weaponPickCount[1]}/4</color>";


    }

    public void Craft(int recipeIndex)  // 어떤 레시피인지 인덱스로 받기
    {
        // 카타나 (recipeIndex == 0)
        if (recipeIndex == 0)
        {
            if (enterPlayer.weaponPickCount[0] < 5 || enterPlayer.weaponPickCount[3] < 5)
            {
                StopCoroutine(Talk());
                StartCoroutine(Talk());
                return;
            }
            enterPlayer.weaponPickCount[0] -= 5;
            enterPlayer.weaponPickCount[3] -= 5;

            if (enterPlayer.weaponPickCount[0] <= 0)
                RemoveWeaponFromSlot(0); // 망치
            if (enterPlayer.weaponPickCount[3] <= 0)
                RemoveWeaponFromSlot(3); // 방패
        }

        if (recipeIndex == 1)
        {
            if (enterPlayer.weaponPickCount[1] < 4)
            {
                StopCoroutine(Talk());
                StartCoroutine(Talk());
                return;
            }
            enterPlayer.weaponPickCount[1] -= 4;

            if (enterPlayer.weaponPickCount[1] <= 0)
                RemoveWeaponFromSlot(1); // 권총 제거
        }

        // 아이템 스폰
        Vector3 ranVec = Vector3.right * Random.Range(-3, 3) + Vector3.forward * Random.Range(-3, 3);
        Instantiate(recipeItemObjs[recipeIndex], itemPos[recipeIndex].position + ranVec, itemPos[recipeIndex].rotation);

        UpdateRecipeUI();
    }

    void RemoveWeaponFromSlot(int weaponIndex)
    {
        // 장착 중이면 해제
        if (enterPlayer.equipWeapon != null &&
            enterPlayer.weapons[weaponIndex].GetComponent<Weapon>() == enterPlayer.equipWeapon)
        {
            enterPlayer.equipWeapon.gameObject.SetActive(false);
            enterPlayer.equipWeapon = null;
            enterPlayer.equipWeaponIndex = -1;
        }

        // 슬롯에서 제거
        for (int i = 0; i < enterPlayer.weaponSlots.Length; i++)
        {
            if (enterPlayer.weaponSlots[i] == weaponIndex)
            {
                enterPlayer.weaponSlots[i] = -1;
                break;
            }
        }

        // hasWeapons 초기화
        enterPlayer.hasWeapons[weaponIndex] = false;
        enterPlayer.weapons[weaponIndex].SetActive(false);
    }

    IEnumerator Talk()
    {
        talkText.text = "재료가 부족합니다."; // 잠깐 부족 메시지 표시
        yield return new WaitForSeconds(2f);
        UpdateRecipeUI(); // 다시 보유량으로 복귀
    }
}