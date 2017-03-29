using UnityEngine;
using System.Collections;
using XLua;
using System.IO;

public class SignatureLoaderTest : MonoBehaviour {
    public static string PUBLIC_KEY = "<RSAKeyValue><Modulus>v3/dBuxvZWYqMzmFDCD1iwHOQzEKaNh4eTtK3JAMcsma8V2XA2H2a960vjMrIaqgIV/q02pG/JfB84TDxENevNa1shX61HzYr0zvEYjRnIS44aKRgeGbZYSHCh80mRTfLf9bgz3VompbVCUBuFSeOzpdGmdz8aVyPVDii5js4y0=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

    // Use this for initialization
    void Start () {
        LuaEnv luaenv = new LuaEnv();
#if UNITY_EDITOR
        luaenv.AddLoader(new SignatureLoader(PUBLIC_KEY, (ref string filepath) =>
        {
            filepath = Application.dataPath + "/XLua/EasyXLua/XLua/Examples/10_SignatureLoader/" + filepath.Replace('.', '/') + ".lua";
            if (File.Exists(filepath))
            {
                return File.ReadAllBytes(filepath);
            }
            else
            {
                return null;
            }
        }));
#else //为了让手机也能测试
        luaenv.AddLoader(new SignatureLoader(PUBLIC_KEY, (ref string filepath) =>
        {
            filepath = filepath.Replace('.', '/') + ".lua";
            TextAsset file = (TextAsset)Resources.Load(filepath);
            if (file != null)
            {
                return file.bytes;
            }
            else
            {
                return null;
            }
        }));
#endif
        luaenv.DoString(@"
            require 'signatured1'
            require 'signatured2'
        ");
        luaenv.Dispose();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
