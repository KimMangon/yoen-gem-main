using System.Collections;
using Unity.VisualScripting;
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

    public int[] weaponPickCount = new int[4];
    public int[] weaponSlots = new int[3] { -1, -1, -1};
    public int equipWeaponIndex = -1;

    float hAxis;
    float vAxis;
    float fireDelay;

    bool wDown;
    bool jDown;
    bool iDown;
    bool fDown;
    bool f1Down;
    bool gDown;
    bool rDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool vDown;

    bool isDodge;
    bool isJump;
    bool isBorder;
    bool isSwap;
    bool isDeflect;
    bool isFireReady = true;
    bool isReload;
    bool isDamage;
    bool isShop;
    bool isDead;

    //e스킬 변수들
    public float rollSkillCooldown = 20f; // 쿨타임 고정
    public float rollSkillTimer = 0f;    // 쿨타임 타이머
    public bool isRollSkillReady = true;  // 스킬 준비 여부
    public int rollSkillMaxCount = 3; // 라운드당 최대 사용 횟수
    public int rollSkillCount = 0;   // 현재 사용 횟수

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
        if (manger.isMapOpen) return;

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
        RollSkill();

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
        f1Down = Input.GetButtonDown("Fire2");
        gDown = Input.GetButtonDown("Grenade");
        rDown = Input.GetButtonDown("Reload");
        vDown = Input.GetButtonDown("vDown");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;
        if (isSwap || !isFireReady || isReload || isDeflect || isDead)
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

            if (Physics.Raycast(ray, out rayHit, 100, LayerMask.GetMask("Floor")))
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
            else if (equipWeapon.type == Weapon.Type.Katana)
            {
                anim.SetTrigger("doSwing"); // 망치 애니메이션 재활용
            }

            fireDelay = 0;
        }

        if (f1Down && equipWeapon != null && equipWeapon.type == Weapon.Type.Katana
    && isFireReady && !isDodge && !isSwap && !isShop && !isDead)
        {
            equipWeapon.Deflect();
            anim.SetTrigger("doBlock");
            isDeflect = true; 
            Invoke("DeflectOut", 0.6f);
            fireDelay = 0;
        }


    }

    void DeflectOut()
    {
        isDeflect = false;
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
        if (sDown1 && (weaponSlots[0] == -1 || equipWeaponIndex == 0)) return;
        if (sDown2 && (weaponSlots[1] == -1 || equipWeaponIndex == 1)) return;
        if (sDown3 && (weaponSlots[2] == -1 || equipWeaponIndex == 2)) return;

        int slotIndex = -1;
        if (sDown1) slotIndex = 0;
        if (sDown2) slotIndex = 1;
        if (sDown3) slotIndex = 2;

        if (slotIndex != -1 && !isJump && !isDodge && !isShop && !isDead)
        {
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = slotIndex;
            equipWeapon = weapons[weaponSlots[slotIndex]].GetComponent<Weapon>();
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

                // 이미 갖고 있는 무기면 횟수만 증가
                if (hasWeapons[weaponIndex])
                {
                    weaponPickCount[weaponIndex]++;
                    Debug.Log($"무기 {weaponIndex} 획득 횟수: {weaponPickCount[weaponIndex]}");
                    Destroy(nearObject);
                    return;
                }

                // 빈 슬롯 찾기
                int emptySlot = -1;
                for (int i = 0; i < weaponSlots.Length; i++)
                {
                    if (weaponSlots[i] == -1)
                    {
                        emptySlot = i;
                        break;
                    }
                }

                if (emptySlot != -1)
                {
                    weaponSlots[emptySlot] = weaponIndex;
                    hasWeapons[weaponIndex] = true;
                    weaponPickCount[weaponIndex]++;

                    if (equipWeapon == null)
                    {
                        equipWeaponIndex = emptySlot;
                        equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
                        equipWeapon.gameObject.SetActive(true);
                    }

                    GameObject weaponObj = nearObject;
                    nearObject = null; // 먼저 null로
                    Destroy(weaponObj);
                }
                else
                {
                    // 슬롯 꽉 참 → 교환/버리기 UI 호출 (2번에서 구현)
                    manger.ShowWeaponSwapUI(weaponIndex, nearObject);
                }
            }

            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }

            else if (nearObject.tag == "Blacksmith")
            {
                Blacksmith blacksmith = nearObject.GetComponent<Blacksmith>();
                blacksmith.Enter(this);
                isShop = true;
            }
        }
    }

    void RollSkill()
    {
        // 쿨타임 카운트
        if (!isRollSkillReady)
        {
            rollSkillTimer -= Time.deltaTime;
            if (rollSkillTimer <= 0)
            {
                isRollSkillReady = true;
                rollSkillTimer = 0;
            }
        }

        if (Input.GetButtonDown("eDown") && isRollSkillReady && !isDead && GameManager.Instance.isBattle)
        {
            if (rollSkillCount >= rollSkillMaxCount) return;
            // 증강에서 레벨 가져오기
            Augment rollAugment = GetRollAugment();
            if (rollAugment == null || rollAugment.level == 0) return;

            float duration = rollAugment.data.durations[rollAugment.level - 1];
            int damage = (int)rollAugment.data.damages[rollAugment.level - 1];
            int count = rollAugment.data.counts[rollAugment.level - 1];

            for (int i = 0; i < rollWeapons.Length; i++)
            {
                bool shouldBeActive = i < count;
                rollWeapons[i].SetActive(shouldBeActive);

                if (shouldBeActive)
                {
                    Weapon w = rollWeapons[i].GetComponent<Weapon>();
                    if (w != null)
                    {
                        w.damage = damage + bonusMeleeDamage;
                        w.Use(duration);
                    }
                }
            }

            isRollSkillReady = false;
            rollSkillTimer = rollSkillCooldown;

            rollSkillCount++;
            isRollSkillReady = false;
            rollSkillTimer = rollSkillCooldown;
        }
    }

    Augment GetRollAugment()
    {
        foreach (GameObject card in GameManager.Instance.allCards)
        {
            Augment aug = card.GetComponent<Augment>();
            if (aug != null && aug.data.augmentId == 8)
                return aug;
        }
        return null;
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
        if (other.tag == "Weapon" || other.tag == "Shop" || other.tag == "Blacksmith")
            nearObject = other.gameObject;
        

    }

    void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        if (other.tag == "Weapon")
        {
            if (nearObject == other.gameObject)
                nearObject = null;
        }
        else if (other.tag == "Shop" && nearObject != null)
        {
            Shop shop = nearObject.GetComponent<Shop>();
            if (shop != null) // [추가] null 체크
            {
                shop.Exit();
                isShop = false;
                nearObject = null;
            }
        }
        else if (other.tag == "Blacksmith" && nearObject != null)
        {
            Blacksmith blacksmith = nearObject.GetComponent<Blacksmith>();
            if (blacksmith != null) // [추가] null 체크
            {
                blacksmith.Exit();
                isShop = false;
                nearObject = null;
            }
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
