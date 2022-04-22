using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 不需要继承MonoBehaviour的Manager基类
/// </summary>
public class BaseMananger<T> where T : new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = new T();
            return instance;
        }
    }
}
