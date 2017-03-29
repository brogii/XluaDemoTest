using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System.IO;
using UnityEngine.UI;



[LuaCallCSharp]
public class LuaManager : MonoBehaviour {

    public static LuaEnv Lua = null;
    private static Dictionary<string,byte[]> LuaDict = new Dictionary<string, byte[]>();
    void Start () {
	}
    public  void Initialize() {
        Lua = new LuaEnv();
        Lua.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);



        if (LuaConst.DebugMode ==true)
        {
            string path = Application.dataPath + "/XLua/EasyXLua/ToBuild/lua/";
            GetAllFiles(new DirectoryInfo(path));
        }
        else {
            if (LuaConst.LuaBundleMode )
            {
                string uri = LuaConst.DataPath + "Lua.medsci";
                byte[]   stream = File.ReadAllBytes(uri);
                AssetBundle bundle = AssetBundle.LoadFromMemory(stream);
                TextAsset[] TA = bundle.LoadAllAssets<TextAsset>();
                foreach (TextAsset t in TA) {
                    string luaDecrypt =  LuaConst.Decrypt(t.text, "12345678", "56781234");
                    LuaDict.Add(t.name, System.Text.Encoding.UTF8.GetBytes(luaDecrypt));
                }
            }
            else {
                string path = LuaConst.DataPath + "/lua/";
                GetAllFiles(new DirectoryInfo(path));
            }      
        }  
        XLua.LuaEnv.CustomLoader CL = new XLua.LuaEnv.CustomLoader(GetLuaFiles);
        Lua.AddLoader(CL);   
    }

    byte[] GetLuaFiles(ref string filename)
    {
        foreach (KeyValuePair<string, byte[]> luafile in LuaDict)
        {
            if (filename == luafile.Key.Split('.')[0] || filename == (luafile.Key.Split('.')[0] + "." + luafile.Key.Split('.')[1]) || filename == luafile.Key)
            {

                //     string st = File.ReadAllText(luafile.Value);
                //      return System.Text.Encoding.UTF8.GetBytes(st);
                return luafile.Value;
            }
        }
        return null;
    }


    void GetAllFiles(DirectoryInfo dir)//搜索文件夹中所有文件以及子文件夹中的文件
    {
       
        FileInfo[] allFile = dir.GetFiles();
        foreach (FileInfo fi in allFile)
        {
            LuaDict.Add(fi.Name,File.ReadAllBytes(fi.FullName));
        }
        DirectoryInfo[] allDir = dir.GetDirectories();
        foreach (DirectoryInfo d in allDir)
        {
            GetAllFiles(d);
        }
       
    }
    /// <summary>
    /// 获取textasset文件
    /// </summary>
    public static string GetTextAsset(string filename)
    {
        foreach (KeyValuePair<string, byte[]> luafile in LuaDict)
        {
            if (filename == luafile.Key.Split('.')[0] || filename == (luafile.Key.Split('.')[0] + "." + luafile.Key.Split('.')[1]) || filename == luafile.Key)
            {
                string st = System.Text.Encoding.Default.GetString(luafile.Value);
                return st;
            }
        }
        return null;
    }




    public void StartMain() {        
       Lua.DoString("require 'Main'");
       Lua.DoString("Main()");
    }
    public void StartLoginPanel() {
        Lua.DoString("require 'Login'");
        Lua.DoString("Login()");
    }
	

	void Update () {
        if (Lua != null)
        {
            Lua.Tick();
        }
    }

    void OnDestroy() {
   //     Lua.Dispose();
    }
}
