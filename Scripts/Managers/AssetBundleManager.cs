using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    // 存储加载过的AB包的容器
    private Dictionary<string, AssetBundle> ABDictionary;

    // 主包
    private AssetBundle ab_main;

    // 依赖关系配置文件
    private AssetBundleManifest manifest;

    // AB包路径
    private string PathUrl
    {
        get
        {
            return Application.streamingAssetsPath + "/";
        }
    }
    // 主包名 
    // 命名规则: IOS -> IOS    安卓 -> Android   PC -> PC
    private string MainAssetBundleName
    { 
        get
        {
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "PC";
#endif
        }
    }

    protected override void Awake()
    {
        base.Awake();
        ABDictionary = new Dictionary<string, AssetBundle>();
        GetMainAssetBundle();
    }


    /// <summary>
    /// 获取主包 
    /// 加载依赖关系配置文件
    /// </summary>
    private void GetMainAssetBundle()
    {
        if (ab_main == null)
        {
            ab_main = AssetBundle.LoadFromFile(PathUrl + MainAssetBundleName);
            manifest = ab_main.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }


    /// <summary>
    /// 加载AB包
    /// </summary>
    /// <param name="abName">AB包名</param>
    private void LoadAB(string abName)
    {
        AssetBundle ab = null;
        // 加载依赖包
        string[] dependencies = manifest.GetAllDependencies(abName);
        for (int i = 0; i < dependencies.Length; i++)
        {
            if (!ABDictionary.ContainsKey(dependencies[i]))
            {
                ab = AssetBundle.LoadFromFile(PathUrl + dependencies[i]);
                ABDictionary.Add(dependencies[i], ab);
            }
        }

        // 加载资源AB包
        if (!ABDictionary.ContainsKey(abName))
        {
            ab = AssetBundle.LoadFromFile(PathUrl + abName);
            ABDictionary.Add(abName, ab);
        }
    }


    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <param name="abName">AB包名</param>
    /// <param name="assetName">资源名</param>
    public Object LoadRes(string abName, string assetName)
    {
        LoadAB(abName);

        return ABDictionary[abName].LoadAsset(assetName);
    }


    /// <summary>
    /// 同步加载资源 Lua不支持泛型 所以这里要在参数中指定类型
    /// </summary>
    public Object LoadRes(string abName, string assetName, System.Type type)
    {
        LoadAB(abName);

        return ABDictionary[abName].LoadAsset(assetName, type);
    }


    /// <summary>
    /// 同步加载资源 泛型
    /// </summary>
    public T LoadRes<T>(string abName, string assetName) where T : Object
    {
        LoadAB(abName);

        return ABDictionary[abName].LoadAsset<T>(assetName);
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    public void LoadResAsync(string abName, string resName, UnityAction<Object> callBack)
    {
        StartCoroutine(RealLoadResAsync(abName, resName, callBack));
    }
    private IEnumerator RealLoadResAsync(string abName, string resName, UnityAction<Object> callBack)
    {
        LoadAB(abName);
        AssetBundleRequest abRequest = ABDictionary[abName].LoadAssetAsync(resName);
        yield return abRequest;
        callBack(abRequest.asset);
    }


    /// <summary>
    /// 异步加载资源 Type
    /// </summary>
    public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callBack)
    {
        StartCoroutine(RealLoadResAsync(abName, resName, type, callBack));
    }
    private IEnumerator RealLoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callBack)
    {
        LoadAB(abName);
        AssetBundleRequest abRequest = ABDictionary[abName].LoadAssetAsync(resName, type);
        yield return abRequest;
        callBack(abRequest.asset);
    }


    /// <summary>
    /// 异步加载资源 泛型
    /// </summary>
    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callBack) where T : Object
    {
        StartCoroutine(RealLoadResAsync<T>(abName, resName, callBack));
    }
    private IEnumerator RealLoadResAsync<T>(string abName, string resName, UnityAction<T> callBack) where T : Object
    {
        LoadAB(abName);
        AssetBundleRequest abRequest = ABDictionary[abName].LoadAssetAsync<T>(resName);
        yield return abRequest;
        callBack(abRequest.asset as T);
    }


    /// <summary>
    /// 卸载单个AB包
    /// </summary>
    /// <param name="abName">AB包名</param>
    public void UnloadAB(string abName)
    {
        if (ABDictionary.ContainsKey(abName))
        {
            ABDictionary[abName].Unload(false);

            ABDictionary.Remove(abName);
        }
    }


    /// <summary>
    /// 卸载所有AB包
    /// </summary>
    public void ClearAB()
    {
        AssetBundle.UnloadAllAssetBundles(false);

        ABDictionary.Clear();

        ab_main = null;
        manifest = null;
    }
}
