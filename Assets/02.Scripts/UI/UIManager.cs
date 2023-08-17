using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject obj = new GameObject("UIManager");
                obj.AddComponent<UIManager>();
                instance = obj.GetComponent<UIManager>();
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }

    public Dictionary<string,GameObject> uiList = new Dictionary<string,GameObject>();
    public List<GameObject> list = new List<GameObject>();

    private void Awake()
    {
        instance = this;
        foreach (var item in list)
        {
            uiList.Add(item.name, item);
        }
    }
    public void ShowUI(string key, bool condition)
    {
        if(uiList.ContainsKey(key))
        {
            uiList[key].SetActive(condition);
        }
    }
    public void CloseUI(string key, bool condition)
    {
        if (uiList.ContainsKey(key))
        {
            uiList[key].SetActive(condition);
        }
    }
    public bool GetShowUI(string key)
    {
        if (uiList.ContainsKey(key))
        {
            return uiList[key].activeSelf;
        }
        Debug.Log("GetShowUI : UI no Key!");
        return false;
    }
    public GameObject GetUI(string key)
    {
        if(uiList.ContainsKey(key))
        {
            return uiList[key];
        }
        Debug.Log("GetUI : UI no key");
        return null;
    }
    public void OnShowUI(string key)
    {
        bool condition = GetShowUI(key);
        ShowUI(key, !condition);
        //OnCloseSidePanel();
    }

}
