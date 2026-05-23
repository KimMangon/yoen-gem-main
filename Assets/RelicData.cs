using UnityEngine;

[CreateAssetMenu(fileName = "Relic", menuName = "Scriptable/Relic")]
public class RelicData : ScriptableObject
{



    public enum RelicEffectType
    {
        HealOnKill,          // 킬 10번마다 체력 회복
        BonusDamageEveryN,   // N번째 공격마다 추가 데미지
        DodgeBoost,          // 도약 거리 증가
        Revive,              // 부활
        DoubleEdgedSword,    // 양날의 검
        HealToAttack,        // 회복 불가 힐 → 공격력

    }


    [Header("기본 정보")]
    public string relicName;
    public int relicId;
    [TextArea] public string relicDesc;
    public Sprite relicIcon;
    public RelicEffectType effectType;


    [Header("수치")]
    public float[] values;



}