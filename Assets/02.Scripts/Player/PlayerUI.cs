using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class PlayerUI : MonoBehaviour
{
    public Text playerNameText;
    public Slider playerHealthSlider;

    private PlayerController playerController;
    
    // 주기적으로 player의 체력을 봐주어야함
    public void SetTarget(PlayerController _target)
    {
        if(_target == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> PlayController target for PlayerUI.SetTarget.", this);
        }
        playerController = _target;

        CapsuleCollider coll = playerController.GetComponent<CapsuleCollider>();

        if (playerNameText != null)
        {
            playerNameText.text = playerController.photonView.Owner.NickName;
        }
    }

    private void Update()
    {
        if (playerController == null)
        {
            Destroy(this.gameObject);
            return;
        }
        if(playerHealthSlider != null)
        {
            {
                playerHealthSlider.value = playerController.curHp / playerController.maxHp;
            }
        }
        //transform.LookAt(Camera.main.transform);
        transform.rotation = Camera.main.transform.rotation;
    }
    
}
