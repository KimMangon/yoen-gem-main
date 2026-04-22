using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Deflect : MonoBehaviour
{
    public Transform target;
    public GameObject player;
    public Weapon weapon;

    public GameObject deflectEffect;

    private void Update()
    {
        transform.position = target.position;
        transform.rotation = player.transform.rotation;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "EnemyBullet" && weapon.blockArea.enabled)
        {
            Rigidbody bulletRigid = other.GetComponent<Rigidbody>();
            if (bulletRigid != null)
            {
                // NavMeshAgent 있으면 먼저 끄기
                NavMeshAgent nav = other.GetComponent<NavMeshAgent>();
                Vector3 originalVelocity = Vector3.zero;

                if (nav != null)
                {
                    originalVelocity = nav.velocity; // NavMesh velocity 저장
                    nav.enabled = false;
                }
                else
                {
                    originalVelocity = bulletRigid.linearVelocity;
                }

                // 원래 날아온 방향 반전
                if (originalVelocity != Vector3.zero)
                {
                    bulletRigid.linearVelocity = -originalVelocity;
                    other.transform.rotation = Quaternion.LookRotation(-originalVelocity);
                }

                other.tag = "Bullet";
                other.gameObject.layer = LayerMask.NameToLayer("PlayerBullet");

                BossMissile bossMissile = other.GetComponent<BossMissile>();
                if (bossMissile != null)
                {
                    bossMissile.enabled = false;
                    Destroy(other.gameObject, 2f);
                }

                if (deflectEffect != null)
                    Instantiate(deflectEffect, other.transform.position,
                        Quaternion.LookRotation(bulletRigid.linearVelocity));
            }
        }
    }
}
