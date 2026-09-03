using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Augment : MonoBehaviour
{
    public AugmentData data;
    public int level;
    public Player player;
    public Bullet bullet;
    public Weapon weapon;

    Image icon;
    Text augmentName;
    Text augmentLevel;

    // 근접 강화(0번) 수치 저장
    int buffAmount = 0;
    // 근접 약화(1번) 수치 저장
    int debuffAmount = 0;
    // 원거리 강화(2번) 수치 저장
    int rangeBuffAmount = 0;
    // 원거리 약화(3번) 수치 저장
    int rangeDebuffAmount = 0;
    // 최대체력 증가(4번) 수치 저장
    int maxHealthBuffAmount = 0;
    // 최대체력 감소(5번) 수치 저장
    int maxHealthDebuffAmount = 0; 
    // 이동속도 강화(6번) 수치 저장
    float speedBuffAmount = 0;
    // 이동속도 약화(7번) 수치 저장
    float speedDebuffAmount = 0;

    int maxLevel;

    void Awake()
    {
        icon = GetComponentsInChildren<Image>()[1];
        icon.sprite = data.augmentIcon;

        Text[] texts = GetComponentsInChildren<Text>();
        augmentName = texts[0];
        augmentLevel = texts[1];
    }

    void LateUpdate()
{
    augmentName.text = data.augmentName;

    int nextLevel = Mathf.Min(level, data.damages.Length - 1);

    switch (data.augmentType)
    {
            case AugmentData.AugmentType.Melee:
                int mVal = (int)data.damages[nextLevel];
                if (data.augmentId == 0)
                    augmentLevel.text = "Lv." + level + "\n<color=#6AB4FF>" + mVal + " 강화</color>";
                else
                    augmentLevel.text = "Lv." + level + "\n<color=#FF6464>" + mVal + " 약화</color>";
                break;

            case AugmentData.AugmentType.Bullet:
                int rVal = (int)data.damages[nextLevel];
                if (data.augmentId == 2)
                    augmentLevel.text = "Lv." + level + "\n<color=#6AB4FF>" + rVal + " 강화</color>";
                else
                    augmentLevel.text = "Lv." + level + "\n<color=#FF6464>" + rVal + " 약화</color>";
                break;

            case AugmentData.AugmentType.Health:
                int hVal = (int)data.damages[nextLevel];
                if (data.augmentId == 4)
                    augmentLevel.text = "Lv." + level + "\n<color=#6AB4FF>" + hVal + " 최대체력 증가</color>";
                else
                    augmentLevel.text = "Lv." + level + "\n<color=#FF6464>" + hVal + " 최대체력 감소</color>";
                break;

            case AugmentData.AugmentType.Shoe:
                float sVal = data.damages[nextLevel];
                if (data.augmentId == 6)
                    augmentLevel.text = "Lv." + level + "\n<color=#6AB4FF>" + sVal + " 강화</color>";
                else
                    augmentLevel.text = "Lv." + level + "\n<color=#FF6464>" + sVal + " 약화</color>";
                break;

            case AugmentData.AugmentType.Reload:
                float reloadDisplayVal = data.damages[nextLevel];
                augmentLevel.text = "Lv." + level + "\n<color=#6AB4FF>" + reloadDisplayVal + "% 감소</color>";
                break;

            default:
                augmentLevel.text = "Lv." + level;
                break;

        }
    }

    public void OnClick()
    {
        AudioManager.Instance.Play(AudioManager.SFX.Augment);
        maxLevel = data.counts.Length - 1;
        if (level > maxLevel) level = maxLevel;

        if (level < data.counts.Length)
        {
            switch (data.augmentType)
            {
                case AugmentData.AugmentType.Melee:
                    int mAmount = (int)data.damages[level];
                    if (data.augmentId == 0)
                        ApplyBuff(mAmount, true);
                    else if (data.augmentId == 1)
                        ApplyBuff(-mAmount, false);
                    break;

                case AugmentData.AugmentType.Bullet:
                    int rAmount = (int)data.damages[level];
                    if (data.augmentId == 2)
                        ApplyRangeEffect(rAmount, true);
                    else if (data.augmentId == 3)
                        ApplyRangeEffect(-rAmount, false);
                    break;

                case AugmentData.AugmentType.Health:
                    int healthVal = (int)data.damages[level];
                    if (data.augmentId == 4)
                    {
                        player.maxHealth -= maxHealthBuffAmount; // 이전 값 제거
                        maxHealthBuffAmount = healthVal;
                        player.maxHealth += maxHealthBuffAmount; // 새 값 적용
                        player.health += healthVal; // 회복분은 그대로 즉시 지급 (레벨업 시 체력도 같이 차오르게)
                    }
                    else if (data.augmentId == 5)
                    {
                        player.maxHealth += maxHealthDebuffAmount; // 이전 값 되돌림
                        maxHealthDebuffAmount = healthVal;
                        player.maxHealth -= maxHealthDebuffAmount; // 새 값 적용
                        if (player.maxHealth < 1)
                            player.maxHealth = 1;

                        if (player.health > player.maxHealth)
                            player.health = player.maxHealth;

                        if (player.health <= 0)
                            player.OnDie();
                    }
                    break;

                case AugmentData.AugmentType.Shoe:
                    float sAmount = data.damages[level];
                    if (data.augmentId == 6)
                        ApplySpeedEffect(sAmount, true);
                    else if (data.augmentId == 7)
                        ApplySpeedEffect(-sAmount, false);
                    break;

                case AugmentData.AugmentType.editWeapon:
                    if (data.augmentId == 8)
                    {
                        GameManager.Instance.skillEGroup.SetActive(true);
                        // 나머지 플레이어에서 실행
                    }
                    break;

                case AugmentData.AugmentType.Reload:
                    float reloadVal = data.damages[level] / 100f; // % → 소수
                    player.reloadSpeedBonus += reloadVal;
                    if (player.reloadSpeedBonus >= 0.9f) // 최대 90% 감소
                        player.reloadSpeedBonus = 0.9f;
                    break;
            }
        }

        if (level < maxLevel)
            level++;
    }


    void ApplyBuff(int amount, bool isBuff)
    {
        if (isBuff)
        {
            player.UpdateAllMeleeDamage(-buffAmount);
            player.bonusMeleeDamage -= buffAmount;
            buffAmount = amount;
        }
        else
        {
            player.UpdateAllMeleeDamage(-debuffAmount);
            player.bonusMeleeDamage -= debuffAmount;
            debuffAmount = amount;
        }

        player.UpdateAllMeleeDamage(amount);
        player.bonusMeleeDamage += amount;
    }

    void ApplyRangeEffect(int amount, bool isBuff)
    {
        if (isBuff)
        {
            player.bonusRangeDamage -= rangeBuffAmount;
            rangeBuffAmount = amount;
        }
        else
        {
            player.bonusRangeDamage -= rangeDebuffAmount;
            rangeDebuffAmount = amount;
        }

        player.bonusRangeDamage += amount;
    }

    void ApplySpeedEffect(float amount, bool isBuff)
    {
        if (isBuff)
        {
            player.bonusSpeed -= speedBuffAmount;
            speedBuffAmount = amount;
        }
        else
        {
            player.bonusSpeed -= speedDebuffAmount;
            speedDebuffAmount = amount;
        }

        player.bonusSpeed += amount;
    }


















}