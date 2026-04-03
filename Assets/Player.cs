using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public GameObject grenadeObj;
    public GameObject[] rollWeapons;
    public Camera followCamera;
    public GameManager manger;


    public int ammo;
    public int coin;
    public int health;
    public int hasGrenades;
    public int score;
    public int bonusMeleeDamage = 0; // 강화/약화 수치를 저장할 변수
    public int bonusRangeDamage; // 원거리(총알) 추가 데미지
    public float bonusSpeed = 0; // 추가된 이동속도 보너스

    public int maxammo;
    public int maxcoin;
    public int maxHealth;
    public int maxHasGrenades;
    

    int equipWeaponIndex = -1;

    float hAxis;
    float vAxis;
    float fireDelay;

    bool wDown;
    bool jDown;
    bool iDown;
    bool fDown;
    bool gDown;
    bool rDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool sDown4;
    bool vDown;

    bool isDodge;
    bool isJump;
    bool isBorder;
    bool isSwap;
    bool isFireReady = true;
    bool isReload;
    bool isDamage;
    bool isShop;
    bool isDead;
    

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;

    Animator anim;

    MeshRenderer[] meshs;

    GameObject nearObject;
    public Weapon equipWeapon;



    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>();

       
    }

    void Update()
    {
        if (manger.isSetting) return;

        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Grenade();
        Reload();
        Dodge();
        Swap();
        Interation();

    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }
    
    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interation");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Grenade");
        rDown = Input.GetButtonDown("Reload");
        vDown = Input.GetButtonDown("vDown");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        sDown4 = Input.GetButtonDown("Swap4");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;
        if (isSwap || !isFireReady || isReload || isDead)
            moveVec = Vector3.zero;
        if(!isBorder)

        transform.position += moveVec * (speed + bonusSpeed) * (wDown ? 0.5f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);

        if (fDown && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);

            }

        }

    }

    void Jump()
    {
        if (vDown && !isJump && !isDodge && !isSwap && !isShop && !isDead) 
        {
           rigid.AddForce(Vector3.up * 40 , ForceMode.Impulse);
           anim.SetBool("isJump", true);
           anim.SetTrigger("doJump");
           isJump = true;
        }
    }

    void Attack()
    {

        if (equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady &&!isDodge && !isSwap && !isShop && !isDead)
        {
            equipWeapon.Use();
            if(equipWeapon.type == Weapon.Type.Melee)
            {
                anim.SetTrigger("doSwing");
            }
            else if (equipWeapon.type == Weapon.Type.Range)
            {
                anim.SetTrigger("doShot");
            }
            else if (equipWeapon.type == Weapon.Type.Block)
            {
                anim.SetTrigger("doBlock");
            }

            fireDelay = 0;
        }
        


    }

    void Grenade()
    {

        if (hasGrenades == 0)
            return;

        if(gDown && !isReload && !isSwap && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 15;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 1 , ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);

            }
        }

    }



    void Reload()
    {
        if(equipWeapon == null) 
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;

        if (ammo == 0)
            return;

        if (isReload)
            return;
        
        if (rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop && !isDead)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 2.5f);
        }




    }


    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.MaxAmmo ? ammo : equipWeapon.MaxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }






    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap && !isShop && !isDead)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;
        if (sDown4 && (!hasWeapons[3] || equipWeaponIndex == 3))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;
        if (sDown4) weaponIndex = 3;

        if ((sDown1 || sDown2 || sDown3 || sDown4) && !isJump && !isDodge && !isShop && !isDead)
        {
            if(equipWeapon != null)
              equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");
            isSwap = true;
            Invoke("SwapOut", 0.3f);
        }

    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Interation()
    {
        if (iDown && nearObject != null && !isJump && !isDodge && !isDead)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);

            }

            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor") 
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxammo)
                        ammo = maxammo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxcoin)
                        coin = maxcoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;


                    break;

            }
            Destroy(other.gameObject);
        }

        else if(other.tag == "EnemyBullet" || other.tag == "EnemyMeleeBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;
                if(other.GetComponent<Rigidbody>() != null)
                    Destroy(other.gameObject);

                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAtk));
            }

            if(other.GetComponent<Rigidbody>() != null)
            {
                Destroy(other.gameObject);
            }
        }

    }


    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;

        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }

        if (isBossAtk) 
        {
            rigid.AddForce(transform.forward * -40, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(0.1f);

        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

        if (health <= 0 && !isDead)
        {
            OnDie();
        }

        yield return new WaitForSeconds(0.4f);

        isDamage = false;
        
        rigid.linearVelocity = Vector3.zero;

        

    }

    public void OnDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        manger.GameOver();
    }


    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Shop")
            nearObject = other.gameObject;
        

    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
        else if (other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
            
    }

    public void Heal(int amount)
    {
        health += amount;

        // 최대 체력(maxHealth)을 넘지 않도록 제한
        if (health > maxHealth)
        {
            health = maxHealth;
        }

        Debug.Log($"체력 회복! 현재 체력: {health}");
    }



}
