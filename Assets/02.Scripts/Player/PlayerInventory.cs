using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using JetBrains.Annotations;

public class PlayerInventory : MonoBehaviourPun
{
    public Dictionary<string,int> inven = new Dictionary<string, int>();
    public Collider[] scanItems;

    [Header("Items")]
    [Range(0.0f, 5.0f)]
    public float itemScanRange;

    private Animator playerAnim;

    public LayerMask itemLayer;

    PlayerWeapon weapon;

    private void Start()
    {
        playerAnim = GetComponent<Animator>();
        weapon = GetComponent<PlayerWeapon>();
    }
    private void Update()
    {
        ItemScan();
    }
    private void ItemScan()
    {
        if (photonView.IsMine)
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y,
               transform.position.z);
            scanItems = Physics.OverlapSphere(spherePosition, itemScanRange, itemLayer);

            if (scanItems.Length > 0)
            {
                UIManager.Instance.ShowUI("ToolTip", true);
            }
            else
            {
                UIManager.Instance.ShowUI("ToolTip", false);
            }
        }
        
    }
    public void OnGetItemButton()
    {
        if(photonView.IsMine)
        {
            GetItem();
            photonView.RPC("GetItem", RpcTarget.OthersBuffered, null);
            WeaponPanel.UpdateAmmoText(weapon.invenAmmo, weapon.curAmmo);
        }
    }
    [PunRPC]
    public void GetItem()
    {
        if (scanItems.Length <= 0 ) return;

        if(scanItems[0].gameObject.CompareTag("Ammo"))
        {
            playerAnim.SetTrigger("GetItem");

            if (inven.ContainsKey(scanItems[0].tag))
            {
                inven[scanItems[0].tag] += 30;
                weapon.invenAmmo = inven[scanItems[0].tag];
                
            }
            else
            {
                inven.Add(scanItems[0].tag, 30);
                weapon.invenAmmo = inven[scanItems[0].tag];
            }
            scanItems[0].gameObject.SetActive(false);   
        }
    }
    // 스캔 범위 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.0f, 0.0f, 1.0f, 0.35f);
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y, transform.position.z),
            itemScanRange);
    }
}
