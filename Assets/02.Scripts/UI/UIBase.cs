using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class UIBase : MonoBehaviour
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();
    // C# 리플렉션을 이용해 enum값을 string으로 가져온다
    // 제네릭을 이용해 어떤 컴포넌트를 연결해줄지 힌트를 제공
    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        // 넘어온 enum값을 string의 배열로 반환
        string[] names = Enum.GetNames(type);

        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];

        _objects.Add(typeof(T), objects);

        for(int i = 0; i< names.Length; i++)
        {
            objects[i] = Util.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Debug.Log($"Failed to bind ({names[i]})");
        }
    }

    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if(_objects.TryGetValue(typeof(T), out objects) == false)
        {
            return null;  
        }
        return objects[idx] as T;
    }

    protected Text GetText(int idx){ return Get<Text>(idx); }
    protected Button GetButton(int idx){ return Get<Button>(idx); }
    protected Image GetImage(int idx){ return Get<Image>(idx); }

    public void AddPanel<T>(GameObject obj, GameObject parent = null, string key = null)
    {
        GameObject prefabs = null;
        GameObject panel = null;

        if(typeof(T) == typeof(WeaponPanel))
        {
            prefabs = Resources.Load("Prefabs/WeaponPanel") as GameObject;
            panel = Instantiate(prefabs);
            //panel.GetComponent<WeaponPanel>().SetPanel();
        }
        if(prefabs == null)
        {
            Debug.Log("Panel Prefabs null!");
        }
        if(parent != null) 
        {
            panel.transform.SetParent(parent.transform);
            panel.transform.localPosition = Vector3.zero;
            panel.transform.localPosition = new Vector3(0, 0, 0);
            panel.transform.localScale = Vector3.one;
        }
    }
}
