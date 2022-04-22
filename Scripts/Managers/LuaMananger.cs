using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

/// <summary>
/// Lua管理器 提供Lua解析器 保证解析器的唯一性
/// </summary>
public class LuaMananger : BaseMananger<LuaMananger>
{
    private LuaEnv luaEnv;

    private enum errorType
    {
        UninitializedLuaEnv,
        RedirectFail,
    }


    /// <summary>
    /// _G
    /// </summary>
    public LuaTable Global
    {
        get
        {
            return luaEnv.Global;
        }
    }


    /// <summary>
    /// 初始化解析器
    /// </summary>
    public void InitializeLuaEnv()
    {
        if (luaEnv != null)
            return;

        luaEnv = new LuaEnv();

        // 加载Lua脚本路径重定向
        luaEnv.AddLoader(MyCustomLoader);
        luaEnv.AddLoader(MyCunstomABLoader);
    }

    /// <summary>
    /// 执行Lua语言
    /// </summary>
    public void DoString(string chunk)
    {
        isLuaEnvNull();
        luaEnv.DoString(chunk);
    }


    /// <summary>
    /// 可以直接通过lua脚本名执行脚本
    /// </summary>
    public void DoLuaFile(string fileName)
    {
        isLuaEnvNull();
        DoString("require('" + fileName + "')");
    }


    /// <summary>
    /// 垃圾回收
    /// </summary>
    public void Tick()
    {
        isLuaEnvNull();
        luaEnv.Tick();
    }


    /// <summary>
    /// 销毁Lua解析器
    /// </summary>
    public void Dispose()
    {
        isLuaEnvNull();
        luaEnv.Dispose();
        luaEnv = null;
    }


    /// <summary>
    /// 加载Lua脚本路径重定向
    /// </summary>
    private byte[] MyCustomLoader(ref string filepath)
    {
        // filepath是文件名 通过这个文件名拼出路径
        string path = Application.dataPath + "/Lua/" + filepath + ".lua";

        if (File.Exists(path))
            return File.ReadAllBytes(path);
        else
            ShowErrorLog(errorType.RedirectFail, filepath);

        return null;
    }


    /// <summary>
    /// 重定向 加载AB包中的Lua脚本
    /// </summary>
    private byte[] MyCunstomABLoader(ref string filepath)
    {
        /*
        string path = Application.streamingAssetsPath + "/lua";
        AssetBundle ab = AssetBundle.LoadFromFile(path);
        TextAsset textAsset = ab.LoadAsset<TextAsset>(filepath + ".lua");
        return textAsset.bytes;
        */

        TextAsset luaScript = AssetBundleManager.Instance.LoadRes<TextAsset>("lua", filepath + ".lua");

        if (luaScript != null)
            return luaScript.bytes;
        else
            ShowErrorLog(errorType.RedirectFail, filepath);

        return null;
    }


    /// <summary>
    /// 控制台输出错误日志
    /// </summary>
    private void ShowErrorLog(errorType errorType, string filepath = " ")
    {
        switch (errorType)
        {
            // luaEnv为空
            case errorType.UninitializedLuaEnv:
                Debug.LogError("未初始化LuaEnv");
                break;
            case errorType.RedirectFail:
                Debug.LogError("重定向失败 文件名: " + filepath);
                break;
            default:
                break;
        }
    }

    
    /// <summary>
    /// 检查Lua解析器是否为空 并输出错误日志
    /// </summary>
    private void isLuaEnvNull()
    {
        if (luaEnv == null)
            ShowErrorLog(errorType.UninitializedLuaEnv);
    }
}
