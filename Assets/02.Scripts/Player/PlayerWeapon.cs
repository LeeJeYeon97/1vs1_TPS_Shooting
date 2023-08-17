using Photon.Pun;
using UnityEngine;
using static PlayerController;
using UnityEngine.Windows;
using System.Collections.Generic;
using System.Collections;

public class PlayerWeapon : MonoBehaviourPun
{
    public Transform firePos;
    public GameObject bullet;
    public float attackPower;
    public float attackDelay;
    public float reloadDelay;
    public int invenAmmo;
    public int maxAmmo;
    public int curAmmo;

    private Dictionary<string,int> inven;
    private PlayerInput input;
    private Animator anim;
    private PlayerController playerController;
    
    bool isFire = false;
    public GameObject weapon;
    public Transform weaponCase;
    public Transform weaponHand;

    private Vector3 target;
    AudioSource _audio;
    public AudioClip shotClip;
    public AudioClip reloadClip;
    public ParticleSystem muzzle;

    private void Start()
    {
        inven = GetComponent<PlayerInventory>().inven;
        input = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        _audio = GetComponent<AudioSource>();
        attackPower = 10;
        attackDelay = 0.2f;
        reloadDelay = 1.0f;
        maxAmmo = 30;
        curAmmo = 0;
        invenAmmo = 0;
        
    }
    private void Update()
    {
        
        if(photonView.IsMine && input.isAttack)
        {
            if (playerController.State == WeaponState.Unarmed) return;
            if(playerController.State == WeaponState.Rifle)
            {
                playerController.State = WeaponState.Aim;
            }

            target = playerController._camera.GetComponent<CameraController>().fireTarget;
            if (curAmmo <= 0)
            {
                if (invenAmmo > 0)
                {
                    Reload();
                }
                else return;
            }
            else
            {
                photonView.RPC("Fire", RpcTarget.All, target,photonView.Owner.ActorNumber);
                WeaponPanel.UpdateAmmoText(invenAmmo, curAmmo);
            }
            
        }
    }
    [PunRPC]
    public void Fire(Vector3 target, int actorNum)
    {
        if (!isFire)
        {
            GameObject _bullet = Instantiate(bullet,
            firePos.position, Quaternion.identity);
            Destroy(_bullet, 10.0f);
            muzzle.Play(true);

            _audio.PlayOneShot(shotClip);
            _bullet.GetComponent<BulletController>().SetDir(target,actorNum);
            curAmmo--;
            isFire = true;
            StartCoroutine(FireDelay(attackDelay));
        }
    }
    IEnumerator FireDelay(float attackDelay)
    {
        yield return new WaitForSeconds(attackDelay);
        isFire = false;
    }
    public void Reload()
    {
        StartCoroutine(CoReload());
        int temp = maxAmmo - curAmmo;

        if (temp >= inven["Ammo"])
        {
            curAmmo += inven["Ammo"];
            inven["Ammo"] = 0;
        }
        else
        {
            inven["Ammo"] -= temp; // 가방에 있는 탄약 수 계산
            curAmmo = maxAmmo;
        }
        invenAmmo = inven["Ammo"];
        WeaponPanel.UpdateAmmoText(invenAmmo, curAmmo);
    }
    IEnumerator CoReload()
    {
        // 애니메이션 실행
        anim.SetTrigger("Reload");
        _audio.PlayOneShot(reloadClip);
        yield return new WaitForSeconds(reloadDelay);

    }
    // 숫자키 1번 입력 시 무기를 넣고 뺐다 하는 함수
    [PunRPC]
    public void WeaponEquip()
    {
        if (input.weaponEquip == true)
        {
            playerController.State = WeaponState.Unarmed;
            anim.SetTrigger("UnEquip");
        }
        if (input.weaponEquip == false)
        {
            playerController.State = WeaponState.Rifle;
            anim.SetTrigger("Equip");
        }

        input.weaponEquip = !input.weaponEquip;

        if (photonView.IsMine)
        {
            photonView.RPC("WeaponEquip", RpcTarget.OthersBuffered, null);
            photonView.RPC("WeaponTransformChange", RpcTarget.OthersBuffered, null);
        }
    }
    // 무기 위치 변경
    [PunRPC]
    public void WeaponTransformChange()
    {
        if (input.weaponEquip == false)
        {
            weapon.transform.parent = weaponCase;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
        if (input.weaponEquip == true)
        {
            weapon.transform.parent = weaponHand;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

}
