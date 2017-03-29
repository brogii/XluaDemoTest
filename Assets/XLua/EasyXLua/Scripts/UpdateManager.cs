using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine.UI;

public class UpdateManager : MonoBehaviour
{
    private Thread thread;//下载线程
    static readonly object m_lockObject = new object();//锁定物体
    private static Queue<object[]> events = new Queue<object[]>();//下载事件队列

    private string currDownFile = string.Empty;//当前下载文件
    private List<string> downloadFiles = new List<string>();//已完成
    private List<string> willDownLoadUrl = new List<string>();//from  
    private  List<string> willDownLoadDestination = new List<string>();//to 
    private float totalSize;//总大小
    private float finishedSize;//已下载完成大小
    private float percent;
    private float oldValue;
    void Update() {
        if (percent != oldValue) {
            CommonUI.Instance.UpdateProgressBar(percent);
            oldValue = percent;
        }

    }

    void Awake()
    {
        Init();
        thread = new Thread(OnUpdate);
    }
    void Start() {
        thread.Start();
        oldValue = 0;
        percent = 0;
    }
    void Init()
    {
        DontDestroyOnLoad(gameObject);  //防止销毁自己
        CheckExtractResource(); //释放资源
        Screen.sleepTimeout = SleepTimeout.NeverSleep;    
    }


    /// <summary>
    /// 检查是否需要释放资源
    /// </summary>
    public void CheckExtractResource()
    {
        bool isExists = Directory.Exists(LuaConst.DataPath) &&(LuaConst.LuaBundleMode?true : Directory.Exists(LuaConst.DataPath + "lua/"))
          && File.Exists(LuaConst.DataPath + "files.txt");
        if (isExists ||LuaConst.DebugMode)
        {
            CommonUI.Instance.ShowTips("已是最新版");
            StartCoroutine(OnCheckUpdateResource());
            return;   //文件已经解压过了，自己可添加检查文件列表逻辑
        }
        CommonUI.Instance.ShowTips("开始解压");
        StartCoroutine(OnExtractResource());    //启动释放协成 
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    IEnumerator OnExtractResource()
    {
        string dataPath = LuaConst.DataPath;  //数据目录
        string resPath =LuaConst.AppContentPath; //游戏包资源目录

        if (Directory.Exists(dataPath)) Directory.Delete(dataPath, true);
        Directory.CreateDirectory(dataPath);

        string infile = resPath + "files.txt";
        string outfile = dataPath + "files.txt";
        if (File.Exists(outfile)) File.Delete(outfile);

        string message = "正在解包文件:>files.txt";
        Debug.Log(infile);
        Debug.Log(outfile);
        if (Application.platform == RuntimePlatform.Android)
        {
            WWW www = new WWW(infile);
            yield return www;
            if (www.isDone)
            {
                File.WriteAllBytes(outfile, www.bytes);
            }
            yield return 0;
        }
        else {
            File.Copy(infile, outfile, true);
        }
        yield return new WaitForEndOfFrame();

        //释放所有文件到数据目录
        string[] files = File.ReadAllLines(outfile);
        //解压进度条的显示
        int ExtractFinished = 0;

        foreach (var file in files)
        {
            string[] fs = file.Split('|');
            infile = resPath + fs[0];  //
            outfile = dataPath + fs[0];
            message = "正在解包文件:>" + fs[0];
            Debug.Log("正在解包文件:>" + infile);
            string dir = Path.GetDirectoryName(outfile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (Application.platform == RuntimePlatform.Android)
            {
                WWW www = new WWW(infile);
                yield return www;

                if (www.isDone)
                {
                    File.WriteAllBytes(outfile, www.bytes);
                }
                ExtractFinished++;
                CommonUI.Instance.UpdateProgressBar((float)ExtractFinished / files.Length);
                yield return 0;
            }
            else
            {
                if (File.Exists(outfile))
                {
                    File.Delete(outfile);
                }
                File.Copy(infile, outfile, true);

                ExtractFinished++;
                CommonUI.Instance.UpdateProgressBar((float)ExtractFinished / files.Length);

            }
            yield return new WaitForEndOfFrame();
        }
        message = "解包完成!!!";
        yield return new WaitForSeconds(0.1f);
        message = string.Empty;
        //释放完成，开始启动更新资源
        StartCoroutine(OnCheckUpdateResource());
    }
    /// <summary>
    /// 是否要更新?
    /// </summary>
    IEnumerator OnCheckUpdateResource()
    {
        if (!LuaConst.UpdateMode)
        {
            OnResourceInited();
            yield break;
        }

        string dataPath = LuaConst.DataPath;  //数据目录
        string url = LuaConst.WedUrl;
        string message = string.Empty;
        string random = DateTime.Now.ToString("yyyymmddhhmmss");
        string listUrl = url + "files.txt?v=" + random;
        Debug.LogWarning("LoadUpdate---->>>" + listUrl);

        WWW www = new WWW(listUrl); yield return www;
        if (www.error != null)
        {
            OnResourceInited();
            Debug.Log("连接更新服务器失败");
            yield return new WaitForSeconds(1.5f);
            yield break;
        }
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
        File.WriteAllBytes(dataPath + "files.txt", www.bytes);
        string filesText = www.text;
        string[] files = filesText.Split('\n');

        for (int i = 0; i < files.Length; i++)
        {
            if (string.IsNullOrEmpty(files[i])) continue;
            string[] keyValue = files[i].Split('|');
            string f = keyValue[0];
            string localfile = (dataPath + f).Trim();
            string path = Path.GetDirectoryName(localfile);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fileUrl = url + f + "?v=" + random;
            bool canUpdate = !File.Exists(localfile);
            if (!canUpdate)
            {
                string remoteMd5 = keyValue[1].Trim();
                string localMd5 = LuaConst.md5file(localfile);
                canUpdate = !remoteMd5.Equals(localMd5);
                if (canUpdate) File.Delete(localfile);
            }
            if (canUpdate)
            {   
                willDownLoadUrl.Add(fileUrl);//下载地址  
                willDownLoadDestination.Add(localfile);//目标文件路径  
                totalSize += GetHttpLength(fileUrl);
            }
        }

        if (willDownLoadUrl.Count > 0)
        {
            double tokbsize = Math.Round((float)totalSize / 1024, 2);
            Debug.Log("是否要下载文件大小为"+tokbsize);
            CommonUI.Instance.ShowCheck("是否更新", () => StartCoroutine(UpdateResources()), () => OnResourceInited());
            //TODO            
          
        }
        else
        {
            CommonUI.Instance.ShowTips("已是最新版本");
            Debug.Log("已经是最新版本");
            OnResourceInited();
        }

    }
    /// <summary>
    /// 更新
    /// </summary>
    IEnumerator UpdateResources()
    {
        for (int i = 0; i < willDownLoadUrl.Count; i++)
        {
            Debug.Log("要下载的文件：" + willDownLoadUrl[i]);
            //这里都是资源文件，用线程下载        
            BeginDownload(willDownLoadUrl[i], willDownLoadDestination[i]);
            while (!(downloadFiles.Contains(willDownLoadDestination[i]))) { yield return new WaitForEndOfFrame(); }
        }
        yield return new WaitForEndOfFrame();
        
        Debug.Log("更新完成");
        CommonUI.Instance.ShowTips("更新完成");
        finishedSize = 0;

        willDownLoadUrl = new List<string>();
        willDownLoadDestination = new List<string>();

        OnResourceInited();
    }

    public void OnResourceInited() {
      //  ResManager.Initialize();
      
        LuaManager luamanager = gameObject.GetComponent<LuaManager>();
        luamanager.Initialize();

       ResourcesManager resourcesManager = gameObject.GetComponent<ResourcesManager>();
       resourcesManager.Initialize();
        
        luamanager.StartMain();
    }


    void OnUpdate()
    {
        
        while (true)
        {
           
            lock (m_lockObject)
            {
                if (events.Count > 0)
                {
                    object[] e = events.Dequeue();
            
                    try
                    {
                        OnDownloadFile(e);      
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError(ex.Message);
                    }
                }
            }
            Thread.Sleep(1);
        }
    }

    void BeginDownload(string url, string file)
    {     
        object[] param = new object[2] { url, file };
     
        events.Enqueue(param);
       
    }

    void OnDownloadFile(object[] evParams)
    {
        string url = evParams[0].ToString();
        currDownFile = evParams[1].ToString();
        using (WebClient client = new WebClient())
        {
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            client.DownloadFileAsync(new System.Uri(url), currDownFile);
        }
    }

    private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {     
        float value = e.BytesReceived;
        percent = (finishedSize + value) / totalSize;
            
        float total = e.TotalBytesToReceive;
        if (e.ProgressPercentage == 100 && e.BytesReceived == e.TotalBytesToReceive)
        {
            finishedSize = e.TotalBytesToReceive + finishedSize;
            downloadFiles.Add(currDownFile);
        }
    }
    void  OnDestory()
    {
        thread.Abort();
    }
    /// <summary>
    /// 得到文件大小
    /// </summary>
    long GetHttpLength(string url)
    {
        var length = 0l;
        try
        {
            var req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            req.Method = "HEAD";
            req.Timeout = 5000;
            var res = (HttpWebResponse)req.GetResponse();
            if (res.StatusCode == HttpStatusCode.OK)
            {
                length = res.ContentLength;
            }

            res.Close();
            return length;
        }
        catch (WebException wex)
        {
            return 0;
        }
    }

}
