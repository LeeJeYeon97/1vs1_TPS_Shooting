using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class HudUI : UIBase
{
    public GameObject weaponInfo;
    public Slider hpBar;
    public Slider armorBar;
    public PlayerController player;

    public Button runButton;
    public Button getItemButton;
    public Button fireButton;
    public Button reloadButton;
    public Button jumpButton;
    float curHp;
    float maxHp;
    float hpValue;
    enum PanelName
    {
        WeaponInfo
    }
    enum ProgressBar
    {
        HpBar,
    }
    private void Awake()
    {
        Bind<Slider>(typeof(ProgressBar));
        Bind<Transform>(typeof(PanelName));
        weaponInfo = Get<Transform>((int)PanelName.WeaponInfo).gameObject;
        hpBar = Get<Slider>((int)(ProgressBar.HpBar));
      
    }
    public void SetProgressBar(GameObject target)
    {
        if(player == null)
        {
            player = target.GetComponent<PlayerController>();
        }
        // Hp
        curHp = player.curHp;
        maxHp = player.maxHp;
        hpValue = curHp/maxHp;
        hpBar.GetComponent<Slider>().value = hpValue;
    
    }
    private void Update()
    {
        curHp = player.curHp;
        maxHp = player.maxHp;
        hpValue = curHp / maxHp;
        hpBar.value = hpValue;
    }
}
