using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject Missile;
    public Transform missilePortA;
    public Transform missilePortB;
    public bool isLook;
    public GameObject bossRockPrefab; // 발사할 총알 프리팹

    Vector3 lookVec;
    Vector3 tauntVec;

    bool hasUsedPhase2 = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;

        foreach (Orbit orbit in GetComponentsInChildren<Orbit>())
            orbit.Target = transform;

        StartCoroutine(Think());
    }

    void Update()
    {
        if (isDead)
        {
            StopAllCoroutines();
            return;
        }

        if (!hasUsedPhase2 && curHealth <= maxHealth * 0.5f)
        {
            hasUsedPhase2 = true;
            StartCoroutine(Phase2Pattern());
        }

        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookVec);
        }
        else
            nav.SetDestination(tauntVec);
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);

        int ranAction = Random.Range(0, 5);

        switch (ranAction)
        {
            case 0:
            case 1:
                StartCoroutine(MissileShot());
                break;
            case 2:
            case 3:
                StartCoroutine(RockShot());
                break;
            case 4:
                StartCoroutine(Taunt());
                break;
        }
    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(Missile, missilePortA.position, missilePortA.rotation);
        BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        bossMissileA.target = target;

        yield return new WaitForSeconds(0.3f);
        GameObject instantMissileB = Instantiate(Missile, missilePortB.position, missilePortB.rotation);
        BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        bossMissileB.target = target;

        yield return new WaitForSeconds(2f);

        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("doBigShot");

        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f);

        isLook = true;
        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec;

        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("doTaunt");
        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;

        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(1f);
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;
        StartCoroutine(Think());
    }

    IEnumerator Phase2Pattern()
    {
        anim.SetTrigger("doBigShot");
        yield return new WaitForSeconds(2f);

        Orbit[] orbiters = GetComponentsInChildren<Orbit>();

        int shotCount = 0;
        while (shotCount < 14)
        {
            foreach (Orbit orbiter in orbiters)
            {
                Vector3 dir = (orbiter.transform.position - transform.position).normalized;
                GameObject rock = Instantiate(bossRockPrefab, orbiter.transform.position, Quaternion.LookRotation(dir));

                Rigidbody rockRigid = rock.GetComponent<Rigidbody>();
                if (rockRigid != null)
                    rockRigid.linearVelocity = dir * 20f;
            }

            shotCount++;
            yield return new WaitForSeconds(0.5f);
        }
    }
}