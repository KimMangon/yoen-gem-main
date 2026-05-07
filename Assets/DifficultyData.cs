using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Scriptable/Difficulty")]
public class DifficultyData : ScriptableObject
{
    public string difficultyName;
    public float enemyHealthMult = 1f;   // 적 체력 배율
    public float enemyDamageMult = 1f;   // 적 데미지 배율
    public int extraEnemyPerStage = 0;   // 스테이지당 추가 적 수
    public float coinMult = 1f;          // 돈 획득 배율
}