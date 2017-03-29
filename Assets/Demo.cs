using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using XLua;

[LuaCallCSharp]
public  class Demo 
{
    public static void DemoTest()
    {
        Debug.Log("????");
       
    }
 
    IEnumerator  wwwhelper(WWW www, LuaFunction LF) {
        yield return www;
        LF.Call();

    }

   

}