using UnityEngine;

public class Bullet : MonoBehaviour
{

    public int damage;
    public bool isMelee;
    public bool isRock;


    public void Init(int baseDamage, int bonusDamage)
    {
        // 기본 데미지 + 플레이어의 원거리 버프 수치
        damage = baseDamage + bonusDamage;
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if(!isRock && collision.gameObject.tag == "Floor")
        {
            Destroy(gameObject, 3);
        }
        

    }

    void OnTriggerEnter(Collider other)
    {
        if (!isMelee && other.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }

    }















}
