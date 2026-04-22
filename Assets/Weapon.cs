using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    
    public enum Type {Melee, Range, Block, RollMelee, Katana};
    public Type type;
    public int damage;
    public int MaxAmmo;
    public int curAmmo;

    public float rate;
    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    public Transform bulletPos;
    public GameObject bullet;

    public Transform bulletCasePos;
    public GameObject bulletCase;

    public BoxCollider blockArea;
    public GameObject blockEffect;

    public BoxCollider rollArea;


    public Sprite icon;
    public Player player;

    private List<Enemy> hitEnemies = new List<Enemy>();
    

    public void Use(float duration = 3f)
    {
        if (type == Type.Melee)
        {

            StopCoroutine("Swing");
            StartCoroutine("Swing");

        }
        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
        else if (type == Type.Block) 
        {
            StopCoroutine("Block");
            StartCoroutine("Block");
            
        }
        else if(type == Type.RollMelee)
        {
            StopCoroutine("Roll");
            StartCoroutine(Roll(duration));
        }
        else if (type == Type.Katana)
        {
            // 좌클릭: 휘두르기
            StopCoroutine("KatanaSwing");
            StartCoroutine("KatanaSwing");
        }
    }


    public void StartBlockEffect()
    {
        blockEffect.SetActive(true);
    }

    public void EndBlockEffect()
    {
        blockEffect.SetActive(false);
    }

    IEnumerator Swing()
    {
        
        yield return new WaitForSeconds(0.4f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.15f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;

    }

    IEnumerator Shot() 
    {

        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.linearVelocity = bulletPos.forward * 50;

        // 2. 생성된 총알 오브젝트에서 Bullet 스크립트를 가져와 데미지 설정
        Bullet bulletScript = intantBullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            // 내 기본 데미지 + 플레이어가 가진 원거리 보너스 데미지 전달
            bulletScript.Init(this.damage, player.bonusRangeDamage);
        }

        yield return null;

        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -1) + Vector3.up * Random.Range(3, 1);
        caseRigid.AddForce (caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10 , ForceMode.Impulse);
    }

    IEnumerator Block()
    {
        yield return new WaitForSeconds(0.1f);
        blockArea.enabled = true;
        
        yield return new WaitForSeconds(0.3f);
        blockArea.enabled = false;
        
        yield return new WaitForSeconds(0.2f);

    }
    IEnumerator Roll(float duration)
    {
        yield return new WaitForSeconds(0.1f);
        if (rollArea != null)
        {
            rollArea.gameObject.SetActive(true);
            rollArea.enabled = true;
        }

        yield return new WaitForSeconds(duration);

        StopRoll();
    }

    public IEnumerator HitCooldown()
    {
        rollArea.enabled = false;
        yield return new WaitForSeconds(1.0f);
        rollArea.enabled = true;
    }

    public void StopRoll()
    {
        StopCoroutine("Roll");
        StopCoroutine("HitCooldown");

        if (rollArea != null)
        {
            rollArea.enabled = false;
            rollArea.gameObject.SetActive(false);
        }
    }


    IEnumerator KatanaSwing()
    {
        yield return new WaitForSeconds(0.4f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.15f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }

    public void Deflect()
    {
        
        StopCoroutine("KatanaDeflect");
        StartCoroutine("KatanaDeflect");
    }

    IEnumerator KatanaDeflect()
    {
        yield return new WaitForSeconds(0.1f);
        blockArea.enabled = true;

        yield return new WaitForSeconds(0.3f);
        blockArea.enabled = false;

        yield return new WaitForSeconds(0.2f);
    }

    






}
