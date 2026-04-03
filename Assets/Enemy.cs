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
                rigid.AddForce(transform.forward*70, ForceMode.Impulse);
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
        // 죽었거나 이미 맞고 있는 중(무적 시간)이면 리턴
        if (isDead) return;

        // 1. 일반 근접 및 총알 처리 (기존 로직 유지)
        if (other.tag == "Melee" || other.tag == "Bullet")
        {
            if (isHit) return; // 무적 시간 체크

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

        // 2.회전 망치(RollMelee) 전용 로직
        else if (other.tag == "RollMelee")
        {
            // 망치는 isHit(무적시간)과 상관없이 때릴 수 있어야 하므로 별도로 체크하거나,
            // 망치 자체가 꺼졌다 켜지는 쿨타임을 이용합니다.
            Weapon weapon = other.GetComponent<Weapon>();
            if (weapon != null)
            {
                curHealth -= weapon.damage;

                // 시각적 피격 효과를 위해 잠시 isHit를 켰다 끕니다.
                isHit = true;
                GameManager.Instance.ShowDamageText(weapon.damage, transform.position);

                // 넉백 없이 빨간색 연출만 실행
                StartCoroutine(OnDamage(Vector3.zero, false));

                // [중요] 부딪힌 망치의 판정을 1초간 끕니다. (Weapon 스크립트의 코루틴 호출)
                weapon.StartCoroutine("HitCooldown");
            }
        }
        /*
        if (!isHit && !isDead)
        {


            if (other.tag == "Melee")
            {
                Weapon weapon = other.GetComponent<Weapon>();
                int damage = weapon.damage; // 데미지 값을 미리 변수에 저장
                curHealth -= weapon.damage;
                Vector3 reactVec = transform.position - other.transform.position;
                isHit = true;
                GameManager.Instance.ShowDamageText(damage, transform.position);

                StartCoroutine(OnDamage(reactVec, false));

            }


            else if (other.tag == "Bullet")
            {
                Bullet bullet = other.GetComponent<Bullet>();
                int damage = bullet.damage; // 데미지 값을 미리 변수에 저장
                curHealth -= bullet.damage;
                Vector3 reactVec = transform.position - other.transform.position;
                Destroy(other.gameObject);
                isHit = true;
                GameManager.Instance.ShowDamageText(damage, transform.position);

                StartCoroutine(OnDamage(reactVec, false));

            }

            else if (other.tag == "RollMelee")
            {
                Weapon weapon = other.GetComponent<Weapon>();
                if (weapon != null)
                {
                    curHealth -= weapon.damage;
                    isHit = true;
                    GameManager.Instance.ShowDamageText(weapon.damage, transform.position);

                    // 넉백 없이 연출 실행
                    StartCoroutine(OnDamage(Vector3.zero, false));

                    // [핵심] 부딪힌 망치의 판정을 여기서 끕니다.
                    // Weapon 스크립트에 이 기능을 수행할 함수를 하나 만들 겁니다.
                    weapon.StartCoroutine("HitCooldown");
                }
            }

        }
        */
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
