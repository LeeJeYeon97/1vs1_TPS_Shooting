using Cinemachine;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static WeaponPanel;

public class PlayerController : MonoBehaviourPun,IPunObservable
{
    public enum WeaponState
    {
        Unarmed,
        Rifle,
        Aim,
    }
    [SerializeField]
    private WeaponState _state;
    public WeaponState State
    {
        get { return _state; }
        set
        {
            switch(value)
            {
                case WeaponState.Unarmed:
                    anim.SetBool(hashRifle, false);
                    anim.SetBool(hashUnarmed, true);
                    anim.SetBool(hashAim, false);
                    _state = value;
                    break;
                case WeaponState.Rifle:
                    anim.SetBool(hashRifle, true);
                    anim.SetBool(hashUnarmed, false);
                    anim.SetBool(hashAim, false);
                    _camera.GetComponent<CameraController>().SetDistance(2.0f);
                    _state = value;
                    break;
                case WeaponState.Aim:
                    anim.SetBool(hashRifle, false);
                    anim.SetBool(hashUnarmed, false);
                    anim.SetBool(hashAim, true);
                    transform.rotation = cameraRoot.transform.rotation;
                    _camera.GetComponent<CameraController>().SetDistance(1.0f);
                    _state = value;
                    break;
            }
        }
    }
    
    private Rigidbody rb;
    private Animator anim;
    private PlayerInput input;

    // Animation Hash Value
    private readonly int hashJump = Animator.StringToHash("IsJump");
    public readonly int hashRun = Animator.StringToHash("IsRun");
    public readonly int hashSpeed = Animator.StringToHash("Speed");

    public readonly int hashUnarmed = Animator.StringToHash("Unarmed");
    public readonly int hashRifle = Animator.StringToHash("Rifle");
    public readonly int hashAim = Animator.StringToHash("Aim");
    // Camera Value
    private float xRotateMove;
    private float xRotate;
    private float yRotateMove;
    private float yRotate;

    [Header("Camera Transform")]
    public Transform cameraRoot;

    [Header("Move And Jump Setting")]
    [Range(1.0f, 20.0f)]
    public float moveSpeed = 2.0f;
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    [Range(1.0f, 100.0f)]
    public float rotateSpeed;
    [Range(100.0f, 500.0f)]
    public float jumpPower = 300.0f;


    [Header("Grounded")]
    public bool isGrounded;
    [Range(0.0f, 10.0f)]
    public float groundedRadius;
    [Range(0.0f, 20.0f)]
    public float groundedOffset;
    public LayerMask groundLayers;

    [Header("Hp")]
    public float curHp;
    public float maxHp;

    public CinemachineVirtualCamera _camera;

    public RigBuilder rigBuild;

    private Vector3 receivePos;
    private Quaternion receiveRot;
    private bool receiveGrounded;

    public float damping = 10.0f;

    private Collider myCollider;
    bool isDie = false;

    public GameObject playerUiPrefab;
    private AudioSource _audio;
    public AudioClip footClip;
    public GameObject arrowMap;

    private void Start()
    {
        anim = GetComponent<Animator>();
        input = GetComponent<PlayerInput>();
        myCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        rigBuild = GetComponent<RigBuilder>();
        _audio = GetComponent<AudioSource>();
        State = WeaponState.Unarmed;

        // ground Set
        groundedRadius = 0.25f;
        groundedOffset = 0.1f;
        groundLayers = LayerMask.GetMask("GROUND");
        isGrounded = true;

        cameraRoot = GameObject.Find("CameraRoot").transform;
        _camera = GameObject.FindGameObjectWithTag("FollowCam").GetComponent<CinemachineVirtualCamera>();
        
        maxHp = 100.0f;
        curHp = maxHp;

        if(photonView.IsMine)
        {
            UIManager.Instance.GetUI("Hud").GetComponent<HudUI>().SetProgressBar(this.gameObject);
            arrowMap.gameObject.SetActive(true);
        }
        if(playerUiPrefab != null && !photonView.IsMine)
        {
            GameObject _uiGo = Instantiate(playerUiPrefab);
            _uiGo.transform.SetParent(transform);
            _uiGo.transform.localPosition = new Vector3(0, 2.5f, 0);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
    }
    
    private void Update()
    {
        if (curHp <= 0 && !isDie)
        {
            StartCoroutine(PlayerDie());
        }
        if (photonView.IsMine)
        {
            Move();
            Jump();
            GroundCheck();
            CameraRotate();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position,
                receivePos, Time.deltaTime * damping);

            transform.rotation = Quaternion.Slerp(transform.rotation,
                receiveRot, Time.deltaTime * damping);
            isGrounded = receiveGrounded;
            if(isGrounded)
            {
                GroundAnim();
            }
        }
    }
    private void LateUpdate()
    {
        if (!photonView.IsMine)
            return;
        cameraRoot.transform.position = new Vector3(transform.position.x, transform.position.y + 1.5f,
            transform.position.z);
    }
    private void CameraRotate()
    {
        if (!photonView.IsMine)
            return;
        if (input.rotate != Vector2.zero)
        {

            if(State == WeaponState.Aim)
            {
                yRotateMove = input.rotate.x * Time.deltaTime * rotateSpeed;
                yRotate = yRotate + yRotateMove;
                cameraRoot.eulerAngles = new Vector3(transform.eulerAngles.x, yRotate);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotate);

                // 상화 회전은 CameraRoot를 이용하여 수행
                xRotateMove = input.rotate.y * Time.deltaTime * rotateSpeed;
                xRotate = xRotate + xRotateMove;
                xRotate = Mathf.Clamp(xRotate, -90, 90); // 위, 아래 고정
                cameraRoot.eulerAngles = new Vector3(xRotate, cameraRoot.eulerAngles.y);
            }
            else
            { 
                // 좌우회전
                yRotateMove = input.rotate.x * Time.deltaTime * rotateSpeed;
                yRotate = yRotate + yRotateMove;
                cameraRoot.eulerAngles = new Vector3(transform.eulerAngles.x, yRotate);

                // 상화 회전은 CameraRoot를 이용하여 수행
                xRotateMove = input.rotate.y * Time.deltaTime * rotateSpeed;
                xRotate = xRotate + xRotateMove;
                xRotate = Mathf.Clamp(xRotate, -90, 90); // 위, 아래 고정
                cameraRoot.eulerAngles = new Vector3(xRotate, cameraRoot.eulerAngles.y);
            }
        }
    }

    private void Move()
    {
        if (isDie)
            return;

        moveSpeed = input.run ? runSpeed : walkSpeed;
        if (State == WeaponState.Aim) moveSpeed = walkSpeed;

        Vector3 inputDir = input.moveDir.normalized;

        if(input.moveDir == Vector3.zero)
        {
            moveSpeed = 0.0f;
        }
        anim.SetFloat(hashSpeed, moveSpeed);

        if (input.moveDir != Vector3.zero)
        {
            // TODO : 캐릭터 회전 넣기
            // 카메라의 y값에 움직여야할 각도를 더해 캐릭터의 각도를 움직여준다.
            if(State != WeaponState.Aim)
            {
                float rotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg
                                + cameraRoot.transform.eulerAngles.y;
                // 축기준으로 각도만큼 돌려줌
                transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
                transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
            }
            else if(State == WeaponState.Aim)
            {
                transform.Translate(input.moveDir * Time.deltaTime * moveSpeed);
            }
        }
    }

    private void Jump()
    {
        if (!photonView.IsMine) return;
        if (!isGrounded) return;

        if (input.jump)
        {
            ActiveJump();
            photonView.RPC("ActiveJump", RpcTarget.Others, null);
        }
    }
    [PunRPC]
    private void ActiveJump()
    {
        rb.AddForce(Vector3.up * jumpPower);
        anim.SetTrigger(hashJump);
    }

    private void GroundCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + groundedOffset,
                transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
            QueryTriggerInteraction.Ignore);

        if(isGrounded)
        {
            GroundAnim();
        }
    }
    private void GroundAnim()
    {
        input.jump = false;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("FallingLoop"))
            anim.Play("Jump_Down");
    }

    public void Aim()
    {
        if (State == WeaponState.Unarmed)
        {
            rigBuild.enabled = false;
            return;
        }
        else if (State == WeaponState.Rifle)
        {
            State = WeaponState.Aim;
            rigBuild.enabled = true;
            _camera.GetComponent<CameraController>().SetDistance(1.0f);       
        }
        else if(State == WeaponState.Aim)
        {
            State = WeaponState.Rifle;
            rigBuild.enabled = false;
            _camera.GetComponent<CameraController>().SetDistance(2.0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (isGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z),
            groundedRadius);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isGrounded);
        }
        else
        {
            receivePos = (Vector3)stream.ReceiveNext();
            receiveRot = (Quaternion)stream.ReceiveNext();
            receiveGrounded = (bool)stream.ReceiveNext();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (curHp > 0 && other.transform.CompareTag("Bullet"))
        {
            curHp -= 10;
            Debug.Log(other.GetComponent<BulletController>()._actorNum);
        }
    }
    IEnumerator PlayerDie()
    {
        myCollider.enabled = false;
        rb.useGravity = false;
        isDie = true;
        if(photonView.IsMine)
        {
            GameManager.DieInfo();
        }
        
        List<Transform> posList = GameObject.Find("GameManager").GetComponent<GameManager>().spawnPosList;
        int idx = UnityEngine.Random.Range(0, posList.Count);
        DieAnim();
        photonView.RPC("DieAnim", RpcTarget.OthersBuffered, null);

        yield return new WaitForSeconds(5.0f);

        transform.position = posList[idx].position;

        myCollider.enabled = true;
        rb.useGravity = true;

        
        // 다시 생성
        curHp = 100;
        isDie = false;
        anim.Play("Idle_Walk_Run");
        UIManager.Instance.GetUI("Hud").GetComponent<HudUI>().SetProgressBar(this.gameObject);
        if (photonView.IsMine)
        {
            GameManager.StartInfo();
        }
    }
    [PunRPC]
    private void DieAnim()
    {
        if(State == WeaponState.Aim)
        {
            State = WeaponState.Rifle;
        }
        anim.SetTrigger("Die");
    } 
    public void PlayFootSound()
    {
        if(photonView.IsMine)
            _audio.PlayOneShot(footClip);
    }
}
