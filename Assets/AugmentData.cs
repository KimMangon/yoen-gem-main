using UnityEngine;

[CreateAssetMenu(fileName = "Augment", menuName = "Scriptable/Augment")]
public class AugmentData : ScriptableObject
{
    public enum AugmentType { Melee, Bullet, Health, Shoe, editWeapon, Reload }

    [Header("Main Info")]
    public AugmentType augmentType; // 어떤 공격을 강화할지
    public string augmentName;
    public int augmentId;
    [TextArea] public string augmentDesc;
    public Sprite augmentIcon;

    [Header("Level Data")]
    public float baseDamage; // 소환형 무기 베이스 데미지
    public int basecount;  // 소환형 무기 베이스 개수
    public float[] damages;  // 레벨별 위력
    public int[] counts;     // 레벨별 지속 시간 소환형 무기일땐 갯수
    public float[] durations; // 소환형 무기 지속시간

    [Header("Object Settings")]
    public GameObject[] weaponPrefab;  // 만약 소환형이면 등록

}