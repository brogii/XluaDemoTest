using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;
using System.Security.Cryptography;

public class LuaConst  {
    public const bool DebugMode = false;
    public const bool UpdateMode =false;
    public const bool LuaBundleMode = true; 

    public static string WedUrl = "http://brogii.imwork.net:30813/";

    public static string DataPath
    {
        get
        {
            string game = "xluagame";
            if (Application.isMobilePlatform)
            {
                return Application.persistentDataPath + "/" + game + "/";
            }
            if (LuaConst.DebugMode)
            {
                return Application.dataPath + "/" + "StreamingAssets" + "/";
            }
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                int i = Application.dataPath.LastIndexOf('/');
                return Application.dataPath.Substring(0, i + 1) + game + "/";
            }
            return "c:/" + game + "/";
        }
    }
    public static string AppContentPath
    {
        get {
            string path = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    path = "jar:file://" + Application.dataPath + "!/assets/";
                    return path;
                    break;
                case RuntimePlatform.IPhonePlayer:
                    path = Application.dataPath + "/Raw/";
                    return path;
                    break;
                default:
                    path = Application.dataPath + "/" + "StreamingAssets" + "/";
                    return path;
                    break;
            }
           
        }
       
    }
    public static string md5file(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }

    #region 加/解密
    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="data">原文</param>
    /// <param name="EncryptKey">密钥</param>
    /// <param name="EncryptIV">偏移值</param>
    /// <returns>密文</returns>
    public static string Encrypt(string data, string EncryptKey, string EncryptIV)
    {
        try
        {
            byte[] oriStr = System.Text.ASCIIEncoding.ASCII.GetBytes(EncryptKey);
            byte[] encryptKey = System.Text.ASCIIEncoding.ASCII.GetBytes(EncryptIV);
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(oriStr, encryptKey), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();
            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="data">密文</param>
    /// <param name="DecryptKey">密钥</param>
    /// <param name="DecryptIV">偏移值</param>
    /// <returns>译文</returns>
    public static string Decrypt(string data, string DecryptKey, string DecryptIV)
    {
        try
        {
            byte[] decryptKey = System.Text.ASCIIEncoding.ASCII.GetBytes(DecryptKey);
            byte[] decryptIV = System.Text.ASCIIEncoding.ASCII.GetBytes(DecryptIV);
            byte[] byEnc = Convert.FromBase64String(data);
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(decryptKey, decryptIV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);
            return sr.ReadToEnd();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
    #endregion

}
