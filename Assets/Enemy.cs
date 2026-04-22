using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type {A, B, C, D };
    public Type enemyType;

    public int maxHealth;
    public int curHealth;
    public int score;
    public BoxCollider meleeArea;
    public GameObject bullet;
    public GameObject[] coins;
    public GameManager manger;
    public Transform target;
    public bool isChase;
    public bool isAttack;
    public bool isDead;
    public bool isHit;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshs;
    public NavMeshAgent nav;
    public Animator anim;

    private Dictionary<int, float> lastHitTimes = new Dictionary<int, float>();
    public float hitCooldown = 1.0f; // 다시 맞을 수 있게 되는 시간 (1초)

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if(enemyType != Type.D)
            Invoke("ChaseStart", 2);
    }



    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void Update()
    {
        if (nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
            
    }


    void Targerting()
    {
        if(!isDead && enemyType != Type.D)
        {
            float targetRadius = 0;
            float targetRange = 0;

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 9f;
                    break;
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 30f;
                    break;



            }

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }

    }


    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.5f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(0.5f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.3f);
                rigid.AddForce(transform.forward*80, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.linearVelocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);

                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position , transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.linearVelocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;

        }
 

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);

    }



    void FixedUpdate()
    {
        FreezeVelocity();
        Targerting();
        
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.angularVelocity = Vector3.zero;
            rigid.linearVelocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (isDead) return;

        if (other.tag == "Melee" || other.tag == "Bullet")
        {
            if (isHit) return; 

            int damage = 0;
            if (other.tag == "Melee") damage = other.GetComponent<Weapon>().damage;
            else
            {
                Bullet bullet = other.GetComponent<Bullet>();
                damage = bullet.damage;
                Destroy(other.gameObject);
            }

            curHealth -= damage;
            isHit = true;
            GameManager.Instance.ShowDamageText(damage, transform.position);
            StartCoroutine(OnDamage(Vector3.zero, false));
        }

      
        else if (other.tag == "RollMelee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            if (weapon != null)
            {
                curHealth -= weapon.damage; 
                isHit = true;
                GameManager.Instance.ShowDamageText(weapon.damage, transform.position);
                StartCoroutine(OnDamage(Vector3.zero, false));
                weapon.StartCoroutine("HitCooldown");
            }
        }
       
    }


    public IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        

        foreach(MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;

        

        if (curHealth > 0)
        {
            yield return new WaitForSeconds(0.15f);
            isHit = false;
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
            
        }
        else 
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.gray;

            gameObject.layer = 12;
            isHit=false;
            isDead = true;
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");

            if (GetComponent<Boss>() != null)
            {
                BossMissile[] missiles = FindObjectsByType<BossMissile>(FindObjectsSortMode.None);
                foreach (BossMissile missile in missiles)
                    Destroy(missile.gameObject);
            }

            Player player = target.GetComponent<Player>();
            player.score += score;
            int ranCoin = Random.Range(0, 3);
            Instantiate(coins[ranCoin], transform.position, Quaternion.identity);



            switch (enemyType)
            {
                case Type.A:
                    manger.enemyCntA--;
                    break;
                case Type.B:
                    manger.enemyCntB--;
                    break;
                case Type.C:
                    manger.enemyCntC--;
                    break;
                case Type.D:
                    manger.enemyCntD--;
                    break;
            }

            if (isGrenade) 
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 4, ForceMode.Impulse);

            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;

                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }
                
            
                 Destroy(gameObject, 4);
        }



    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;

        StartCoroutine(OnDamage(reactVec, true));

    }























}
