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

    // 근접 강화(0번) 코루틴 및 수치 저장
    Coroutine buffRoutine;
    int buffAmount = 0;

    // 근접 약화(1번) 코루틴 및 수치 저장
    Coroutine debuffRoutine;
    int debuffAmount = 0;

    // 원거리 강화(2번) 코루틴 및 수치 저장
    Coroutine rangeBuffRoutine;
    int rangeBuffAmount = 0;

    // 원거리 약화(3번) 코루틴 및 수치 저장
    Coroutine rangeDebuffRoutine;
    int rangeDebuffAmount = 0;

    // 이동속도 강화(6번) 코루틴 및 수치 저장
    Coroutine speedBuffRoutine;
    float speedBuffAmount = 0;

    // 이동속도 약화(7번) 코루틴 및 수치 저장
    Coroutine speedDebuffRoutine;
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

    private void LateUpdate()
    {
        augmentName.text = data.augmentName;
        augmentLevel.text = "Lv." + (level);
    }

    public void OnClick()
    {
        maxLevel = data.counts.Length - 1;
        if (level > maxLevel) level = maxLevel;

        if (level < data.counts.Length)
        {
            switch (data.augmentType)
            {
                case AugmentData.AugmentType.Melee:
                    float mDuration = data.counts[level];
                    int mAmount = (int)data.damages[level];

                    if (data.augmentId == 0)
                    {
                        
                        if (buffRoutine != null) player.StopCoroutine(buffRoutine);
                        buffRoutine = player.StartCoroutine(ApplyBuff(mAmount, mDuration, true));
                    }
                    else if (data.augmentId == 1)
                    {
                        
                        if (debuffRoutine != null) player.StopCoroutine(debuffRoutine);
                        debuffRoutine = player.StartCoroutine(ApplyBuff(-mAmount, mDuration, false));
                    }
                    break;

                case AugmentData.AugmentType.Bullet:
                    float rDuration = data.counts[level];
                    int rAmount = (int)data.damages[level];

                    if (data.augmentId == 2)
                    {
                        
                        if (rangeBuffRoutine != null) player.StopCoroutine(rangeBuffRoutine);
                        rangeBuffRoutine = player.StartCoroutine(ApplyRangeEffect(rAmount, rDuration, true));
                    }
                    else if (data.augmentId == 3)
                    {
                        
                        if (rangeDebuffRoutine != null) player.StopCoroutine(rangeDebuffRoutine);
                        rangeDebuffRoutine = player.StartCoroutine(ApplyRangeEffect(-rAmount, rDuration, false));
                    }
                    break;

                case AugmentData.AugmentType.Health:
                    int healthVal = (int)data.damages[level];

                    if (data.augmentId == 4)
                    {
                        player.Heal(healthVal);
                        Debug.Log($"[회복 증강] {healthVal}만큼 즉시 회복되었습니다.");
                    }
                    else if (data.augmentId == 5)
                    {
                        player.health -= healthVal;
                        if (player.health <= 0)
                            player.OnDie();
                        Debug.Log($"[체력 감소] {healthVal}만큼 감소 (남은 체력: {player.health})");
                    }
                    break;

                case AugmentData.AugmentType.Shoe:
                    float sDuration = data.counts[level];
                    float sAmount = data.damages[level];

                    if (data.augmentId == 6)
                    {
                        
                        if (speedBuffRoutine != null) player.StopCoroutine(speedBuffRoutine);
                        speedBuffRoutine = player.StartCoroutine(ApplySpeedEffect(sAmount, sDuration, true));
                    }
                    else if (data.augmentId == 7)
                    {
                        
                        if (speedDebuffRoutine != null) player.StopCoroutine(speedDebuffRoutine);
                        speedDebuffRoutine = player.StartCoroutine(ApplySpeedEffect(-sAmount, sDuration, false));
                    }
                    break;

                case AugmentData.AugmentType.editWeapon:
                    if (data.augmentId == 8)
                    {
                        int targetCount = (int)data.counts[level];
                        float rollDuration = data.durations[level];
                        

                        for (int i = 0; i < player.rollWeapons.Length; i++)
                        {
                            bool shouldBeActive = i < targetCount;
                            player.rollWeapons[i].SetActive(shouldBeActive);

                            if (shouldBeActive)
                            {
                                Weapon w = player.rollWeapons[i].GetComponent<Weapon>();
                                if (w != null)
                                {
                                    w.damage = (int)data.damages[level] + player.bonusMeleeDamage;
                                    w.Use(rollDuration);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        if (level < maxLevel)
            level++;
    }

    void UpdateAllMeleeDamage(int amount)
    {
        if (player.weapons[0] != null)
        {
            Weapon mainWep = player.weapons[0].GetComponent<Weapon>();
            if (mainWep != null)
                mainWep.damage += amount;
        }

        for (int i = 0; i < player.rollWeapons.Length; i++)
        {
            if (player.rollWeapons[i] == null) continue;
            Weapon rollWep = player.rollWeapons[i].GetComponentInChildren<Weapon>(true);
            if (rollWep != null)
                rollWep.damage += amount;
        }
    }

    IEnumerator ApplyBuff(int amount, float time, bool isBuff)
    {
        if (isBuff)
        {
            UpdateAllMeleeDamage(-buffAmount);
            player.bonusMeleeDamage -= buffAmount;
            buffAmount = amount;
        }
        else
        {
            UpdateAllMeleeDamage(-debuffAmount);
            player.bonusMeleeDamage -= debuffAmount;
            debuffAmount = amount;
        }

        UpdateAllMeleeDamage(amount);
        player.bonusMeleeDamage += amount;
        Debug.Log($"{(isBuff ? "강화" : "약화")} 적용: {amount}, 시간: {time}초");

        yield return new WaitForSeconds(time);

        if (isBuff)
        {
            UpdateAllMeleeDamage(-buffAmount);
            player.bonusMeleeDamage -= buffAmount;
            buffAmount = 0;
            buffRoutine = null;
        }
        else
        {
            UpdateAllMeleeDamage(-debuffAmount);
            player.bonusMeleeDamage -= debuffAmount;
            debuffAmount = 0;
            debuffRoutine = null;
        }
        Debug.Log($"{(isBuff ? "강화" : "약화")} 종료");
    }

    IEnumerator ApplyRangeEffect(int amount, float time, bool isBuff)
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
        Debug.Log($"원거리 {(isBuff ? "강화" : "약화")} 적용: {amount}");

        yield return new WaitForSeconds(time);

        if (isBuff)
        {
            player.bonusRangeDamage -= rangeBuffAmount;
            rangeBuffAmount = 0;
            rangeBuffRoutine = null;
        }
        else
        {
            player.bonusRangeDamage -= rangeDebuffAmount;
            rangeDebuffAmount = 0;
            rangeDebuffRoutine = null;
        }
        Debug.Log($"원거리 {(isBuff ? "강화" : "약화")} 종료");
    }

    IEnumerator ApplySpeedEffect(float amount, float time, bool isBuff)
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
        Debug.Log($"이동속도 {(isBuff ? "강화" : "약화")} 적용: {amount}");

        yield return new WaitForSeconds(time);

        if (isBuff)
        {
            player.bonusSpeed -= speedBuffAmount;
            speedBuffAmount = 0;
            speedBuffRoutine = null;
        }
        else
        {
            player.bonusSpeed -= speedDebuffAmount;
            speedDebuffAmount = 0;
            speedDebuffRoutine = null;
        }
        Debug.Log($"이동속도 {(isBuff ? "강화" : "약화")} 종료");
    }


















}