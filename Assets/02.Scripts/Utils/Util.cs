using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Util 
{
    // recursive = ��������� ã��������(�ڽ��� �ڽĵ� Ž��)
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) 
        where T : UnityEngine.Object
    {
        if (go == null)
            return null;
        
        if(recursive == false)
        {
            // �ڽĸ� ã��
            for(int i = 0; i < go.transform.childCount; i++)
            {
                Transform trasnform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || trasnform.name == name)
                {
                    T component = trasnform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            // �ڽ��� �ڽı��� ã��
            foreach(T component in go.GetComponentsInChildren<T>(true))
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }
}
