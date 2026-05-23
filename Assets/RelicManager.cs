using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour
{
    public Player player;
    public List<RelicData> ownedRelics = new List<RelicData>();

    private int killCount = 0;
    private int attackCount = 0;
    private bool hasRevived = false;

    public bool isBonusDamageActive = false;
    public static RelicManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public bool HasRelic(int relicId)
    {
        return ownedRelics.Exists(r => r.relicId == relicId);
    }

    public void AddRelic(RelicData relic)
    {
        ownedRelics.Add(relic);
        ApplyPassiveRelic(relic);
    }

    void ApplyPassiveRelic(RelicData relic)
    {
        switch (relic.effectType)
        {
            case RelicData.RelicEffectType.DodgeBoost:
                // 도약 거리 증가 - Player.cs에서 처리
                player.dodgeSpeedMultiplier = relic.values.Length > 0 ? relic.values[0] : 1.5f;
                break;

            case RelicData.RelicEffectType.DoubleEdgedSword:
                player.damageMultiplier = 2f;
                player.receiveDamageMultiplier = 2f;
                break;

            case RelicData.RelicEffectType.HealToAttack:
                player.healToAttack = true;
                Debug.Log("healToAttack: " + player.healToAttack);
                break;
        }
    }

    // 킬 발생 시 호출
    public void OnKill()
    {
        killCount++;

        foreach (var relic in ownedRelics)
        {
            switch (relic.effectType)
            {
                case RelicData.RelicEffectType.HealOnKill:
                    float interval = relic.values.Length > 1 ? relic.values[1] : 10f;
                    if (killCount % (int)interval == 0)
                    {
                        float healAmount = relic.values.Length > 0 ? relic.values[0] : 5f;
                        player.Heal((int)healAmount);
                    }
                    break;

                
            }
        }
    }

    // 공격 발생 시 호출
    public void OnAttack()
    {
        attackCount++;

        foreach (var relic in ownedRelics)
        {
            switch (relic.effectType)
            {
                case RelicData.RelicEffectType.BonusDamageEveryN:
                    float n = relic.values.Length > 1 ? relic.values[1] : 5f;
                    if (attackCount % (int)n == 0)
                        isBonusDamageActive = true;
                    break;
            }
        }
    }

    // 부활 체크 - Player.cs OnDie()에서 호출
    public bool CheckRevive()
    {
        foreach (var relic in ownedRelics)
        {
            if (relic.effectType == RelicData.RelicEffectType.Revive && !hasRevived)
            {
                hasRevived = true;
                player.health = 50;
                return true; // 부활 성공
            }
        }
        return false; // 부활 불가
    }

    
}