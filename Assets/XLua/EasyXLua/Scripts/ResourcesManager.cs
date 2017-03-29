using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using XLua;
using UnityEngine.UI;


[LuaCallCSharp]
public class ResourcesManager : MonoBehaviour {
    // Use this for initialization
    Dictionary<string, AssetBundle> bundles;
    private AssetBundleManifest manifest;
    private AssetBundle  assetbundle;
    private string[] m_Variants = { };


  

    public void Initialize()
    {
        byte[] stream = null;
        string uri = string.Empty;
        bundles = new Dictionary<string, AssetBundle>();
        uri = LuaConst.DataPath + "StreamingAssets";
        if (!File.Exists(uri)) return;
        
        stream = File.ReadAllBytes(uri);
        
        assetbundle = AssetBundle.LoadFromMemory(stream);

         manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");//todo
                                                                                     //  manifest = assetbundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;


    }
    /// <summary>
    /// 载入素材
    /// </summary>
    public T LoadAsset<T>(string abname, string assetname) where T : UnityEngine.Object
    {
        abname = abname.ToLower();
        AssetBundle bundle = LoadAssetBundle(abname);
        return bundle.LoadAsset<T>(assetname);
    }


    public void LoadPrefab(string abName, string[] assetNames, LuaFunction func)
    {
        abName = abName.ToLower();
        List<UnityEngine.Object> result = new List<UnityEngine.Object>();
        for (int i = 0; i < assetNames.Length; i++)
        {
            UnityEngine.Object go = LoadAsset<UnityEngine.Object>(abName, assetNames[i]);
            if (go != null) result.Add(go);
        }
        if (func != null) func.Call((object)result.ToArray());
    }

    /// <summary>
    /// 载入AssetBundle
    /// </summary>
    public AssetBundle LoadAssetBundle(string abname)
    {
        if (!abname.EndsWith(".medsci"))
        {
            
            abname += ".medsci";
        }
        AssetBundle bundle = null;
        if (!bundles.ContainsKey(abname))
        {
            byte[] stream = null;
            string uri = LuaConst.DataPath + abname;
            LoadDependencies(abname);
            stream = File.ReadAllBytes(uri);
            bundle = AssetBundle.LoadFromMemory(stream); //关联数据的素材绑定
            bundles.Add(abname, bundle);
        }
        else
        {
            bundles.TryGetValue(abname, out bundle);
        }
        return bundle;
    }

    /// <summary>
    /// 载入依赖
    /// </summary>
    void LoadDependencies(string name)
    {
        if (manifest == null)
        {
            Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
            return;
        }
        // Get dependecies from the AssetBundleManifest object..
        string[] dependencies = manifest.GetAllDependencies(name);
        if (dependencies.Length == 0) return;

        for (int i = 0; i < dependencies.Length; i++)
            dependencies[i] = RemapVariantName(dependencies[i]);

        // Record and load all dependencies.
        for (int i = 0; i < dependencies.Length; i++)
        {
            LoadAssetBundle(dependencies[i]);
        }
    }

    // Remaps the asset bundle name to the best fitting asset bundle variant.
    //?????????????????????????????????
    string RemapVariantName(string assetBundleName)
    {
        string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();

        // If the asset bundle doesn't have variant, simply return.
        if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
            return assetBundleName;

        string[] split = assetBundleName.Split('.');

        int bestFit = int.MaxValue;
        int bestFitIndex = -1;
        // Loop all the assetBundles with variant to find the best fit variant assetBundle.
        for (int i = 0; i < bundlesWithVariant.Length; i++)
        {
            string[] curSplit = bundlesWithVariant[i].Split('.');
            if (curSplit[0] != split[0])
                continue;

            int found = System.Array.IndexOf(m_Variants, curSplit[1]);
            if (found != -1 && found < bestFit)
            {
                bestFit = found;
                bestFitIndex = i;
            }
        }
        if (bestFitIndex != -1)
            return bundlesWithVariant[bestFitIndex];
        else
            return assetBundleName;
    }

    /// <summary>
    /// 销毁资源
    /// </summary>
    void OnDestroy()
    {
   //     if (shared != null) shared.Unload(true);
        if (manifest != null) manifest = null;
        Debug.Log("~ResourceManager was destroy!");
    }


  


}


