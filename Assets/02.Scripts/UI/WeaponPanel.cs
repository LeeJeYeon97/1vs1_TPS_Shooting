using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponPanel : MonoBehaviour
{
    private Text ammoText = null;

    public static Action<int,int> UpdateAmmoText;

    private void Awake()
    {
        Transform[] transforms = GetComponentsInChildren<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == "AmmoText")
            {
                ammoText = transforms[i].GetComponent<Text>();
                continue;
            }
        }

        ammoText.text = $"{0} / {0}";
        UpdateAmmoText += (int invenAmmo, int curAmmo) => SetPanel(invenAmmo, curAmmo);
    }
    public void SetPanel(int invenAmmo, int curAmmo = 0)
    {
        ammoText.text = $"{curAmmo} / {invenAmmo}";   
    }
}
