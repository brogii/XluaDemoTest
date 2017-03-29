using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;

public class LuaPackager  {
    static List<string> files = new List<string>();
    static List<string> paths = new List<string>();
    static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();

    [MenuItem("XLua/Build Resources(Andriod)", false, 101)]
    static void BuildAndriodResources() {
       BuildResources(BuildTarget.Android);
    }
    [MenuItem("XLua/Build Resources(IOS)", false, 101)]
    static void BuildIOSResources()
    {
        BuildTarget target;
#if UNITY_5
        target = BuildTarget.iOS;
#else
        target = BuildTarget.iPhone;
#endif
        BuildResources(target);
    }
    [MenuItem("XLua/Build Resources(Windows64)", false, 101)]
    static void BuildWindows64Resources()
    {
        BuildResources(BuildTarget.StandaloneWindows64);
    }



    static void BuildResources(BuildTarget buildtarget) {
        if (Directory.Exists(LuaConst.DataPath))
        {
            Directory.Delete(LuaConst.DataPath, true);
        }
        string streamPath = Application.streamingAssetsPath;
        if (Directory.Exists(streamPath))
        {
            Directory.Delete(streamPath, true);
        }
        Directory.CreateDirectory(streamPath);
        AssetDatabase.Refresh();
        maps.Clear();

        if (LuaConst.LuaBundleMode) {


            AddBuildMap("Lua" + ".medsci", "*.txt", "Assets/XLua/EasyXLua/ToBuild/Lua/");
        }
        else {
            HandleLuaFile();
        }
        
        HandleBundle();

        string resPath = "Assets/" + "StreamingAssets";
        BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle |
                                          BuildAssetBundleOptions.UncompressedAssetBundle;

        BuildPipeline.BuildAssetBundles(resPath, maps.ToArray(), options, buildtarget);//TODO还有ios
     
        BuildFileIndex();

        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 处理Lua文件
    /// </summary>
    static void HandleLuaFile() {

        string luaPath = Application.dataPath + "/StreamingAssets/lua/";
        if (!Directory.Exists(luaPath))
        {
            Directory.CreateDirectory(luaPath);
        }
        string[] luaPaths = { Application.dataPath + "/XLua/EasyXLua/ToBuild/Lua/"
                              };

        for (int i = 0; i < luaPaths.Length; i++)
        {
            paths.Clear(); files.Clear();
            string luaDataPath = luaPaths[i].ToLower();
            Recursive(luaDataPath);
            int n = 0;
            foreach (string f in files)
            {
                if (f.EndsWith(".meta")) continue;
                string newfile = f.Replace(luaDataPath, "");
                string newpath = luaPath + newfile;
                string path = Path.GetDirectoryName(newpath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                if (File.Exists(newpath))
                {
                    File.Delete(newpath);
                }
                
                    File.Copy(f, newpath, true);
                
                UpdateProgress(n++, files.Count, newpath);
            }
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 处理资源
    /// </summary>
    static void HandleBundle()
    {
        string resPath =Application.dataPath + "/" +"StreamingAssets" + "/";
        if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);

      //  AddBuildMap("png" + ".unity3d", "*.png", "Assets/XLua/EasyXLua/ToBuild/png/");
        AddBuildMap("other" + ".MedSci", "*.prefab", "Assets/XLua/EasyXLua/ToBuild/Other/");
        AddBuildMap("test" + ".MedSci", "*.prefab", "Assets/XLua/EasyXLua/ToBuild/Test/");
    }
    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }
    /// <summary>
    /// 更新进度条
    /// </summary>
    static void UpdateProgress(int progress, int progressMax, string desc)
    {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }
/// <summary>
/// 生成md5文本文件
/// </summary>
    static void BuildFileIndex()
    {
        string resPath = Application.dataPath + "/StreamingAssets/";
        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "/files.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear(); files.Clear();
        Recursive(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store")) continue;

            string md5 = LuaConst.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            sw.WriteLine(value + "|" + md5);
        }
        sw.Close(); fs.Close();
    }
   /// <summary>
   /// 生成资源map
   /// </summary>
    static void AddBuildMap(string bundleName, string pattern, string path)
    {
        string[] files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
        if (files.Length == 0) return;
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = files[i].Replace('\\', '/');
            if (bundleName == "Lua.medsci")
            {
                string luafile = File.ReadAllText(files[i]);
                
                files[i] = files[i].Replace("/XLua/EasyXLua/ToBuild/Lua", "/XLua/EasyXLua/ToBuild/LuaEncrypt");
                try
                {
                    if (File.Exists(files[i]))
                    {                      
                        File.Delete(files[i]);
                    }
                    if (!Directory.Exists(Path.GetDirectoryName(files[i]))) Directory.CreateDirectory(Path.GetDirectoryName(files[i]));   
                    
                    FileStream fs = File.Create(files[i]);
                    byte[] temp = System.Text.Encoding.UTF8.GetBytes(LuaConst.Encrypt(luafile, "12345678", "56781234"));
                    fs.Write(temp, 0, temp.Length);                  
                    fs.Close();                    
                }
                catch (Exception e) {
                    Debug.LogError(e);
                }
            }

        }
 

        AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = bundleName;
            build.assetNames = files;
            maps.Add(build);
        
    }


}
