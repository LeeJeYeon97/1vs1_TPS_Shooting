using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static UnityEngine.InputSystem.DefaultInputActions;

public class PlayerInput : MonoBehaviourPun
{
    [Header("Character Input Values")]
    public Vector3 moveDir;
    public Vector2 rotate;

    [Header("State")]
    public bool run;
    public bool jump;
    public bool weaponEquip;
    public bool isAttack;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;

    private PlayerController controller;
    private PlayerInventory playerInventory;
    private PlayerWeapon weaponCtrl;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        playerInventory = GetComponent<PlayerInventory>();
        weaponCtrl = GetComponent<PlayerWeapon>();
        jump = false;
    }
    public void OnMove(InputValue value)
    {
        if (!photonView.IsMine)
            return;

        Vector2 input = value.Get<Vector2>();
        if (input != null)
        {
            moveDir = new Vector3(input.x, 0, input.y);
        }
    }
    public void OnRun()
    {
        if (!photonView.IsMine)
            return;
        if (controller.State == PlayerController.WeaponState.Aim) return;

        if (!run)
        {
            run = true;
        }
        else
        {
            run = false;
        }
    }
    public void OnRotate(InputValue value)
    {
        if (!photonView.IsMine)
            return;
        Vector2 input = value.Get<Vector2>();
        if (input != null)
        {
            rotate = new Vector2(input.x, input.y);
        }
    }
    public void OnJump()
    {
        if (!photonView.IsMine)
            return;

        if (!jump)
        {
            jump = true;
        }
        else
            return;
    }
    private void OnEquip()
    {
        if (!photonView.IsMine)
            return;

        weaponCtrl.WeaponEquip(); 
        
    }

    public void OnAttack(InputValue input)
    {
        if (!photonView.IsMine)
            return;

        if (input.isPressed)
        {
            isAttack = true;
        }
        else
        {
            isAttack = false;
        }
    }
    public void OnGetItem()
    {
        if (!photonView.IsMine)
            return;

        playerInventory.OnGetItemButton();
    }

    public void OnShowMap()
    {
        if (!photonView.IsMine)
            return;
        UIManager.Instance.OnShowUI("Map");
    }
    public void OnReload()
    {
        if (!photonView.IsMine)
            return;
        weaponCtrl.Reload();
    }
    public void OnAim()
    {
        controller.Aim();
    }
}
