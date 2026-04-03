using UnityEngine;
using System.Collections;
public class Block : MonoBehaviour
{
    public Transform target;
    public GameObject Player;

    public Weapon weapon;

    private void Update()
    {
        transform.position = target.position;
        transform.rotation = Player.transform.rotation;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "EnemyBullet")
        {
            Destroy(other.gameObject);
            weapon.StartBlockEffect();
            StartCoroutine(EndEffectDelay());
        }

    }


    IEnumerator EndEffectDelay()
    {
        yield return new WaitForSeconds(0.3f);
        weapon.EndBlockEffect();
    }













}
