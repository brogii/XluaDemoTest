using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CommonUI
{
    private static CommonUI _instance;
    private static Transform canvas;

    public static CommonUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CommonUI();
                canvas = GameObject.Find("Canvas").transform;
            }
            return _instance;
        }
    }

    private static GameObject tips;
    private static GameObject check;
    private static GameObject progressBar;
    private Tween tw;

    public void ShowTips(string s)
    {
        if (tips == null)
        {
            tips = (GameObject)GameObject.Instantiate(Resources.Load("Tips"), canvas, false);
            tips.transform.SetAsLastSibling();
            tips.transform.localPosition = new Vector3(0, 0, 0);
        }
        if (tw != null)
        {
            tw.Complete();
        }
        tips.GetComponent<CanvasGroup>().alpha = 1;
        tips.transform.FindChild("Text").GetComponent<Text>().text = s;
        tw = tips.GetComponent<CanvasGroup>().DOFade(0, 1.0f).SetDelay(3.0f);
    }

    public void ShowCheck(string s, Action ok_event = null, Action cancel_event = null)
    {
        if (check == null)
        {
            check = (GameObject)GameObject.Instantiate(Resources.Load("Check"), canvas, false);
            check.transform.localPosition = new Vector3(0, 0, 0);
        }
        check.GetComponent<CanvasGroup>().alpha = 1;
        check.GetComponent<CanvasGroup>().interactable = true;
        check.transform.FindChild("Text").GetComponent<Text>().text = s;
        if (ok_event != null)
            check.transform.FindChild("OK").GetComponent<Button>().onClick.AddListener(() => ok_event());
        check.transform.FindChild("OK").GetComponent<Button>().onClick.AddListener(() => checkDisappear());
        if (cancel_event != null)
            check.transform.FindChild("Cancel").GetComponent<Button>().onClick.AddListener(() => cancel_event());
        check.transform.FindChild("Cancel").GetComponent<Button>().onClick.AddListener(() => checkDisappear());
    }

    public void UpdateProgressBar(float percent)
    {
        if (progressBar == null)
        {
            progressBar = (GameObject)GameObject.Instantiate(Resources.Load("ProgressBar"), canvas, false);
            progressBar.transform.localPosition = new Vector3(0, 0, 0);
        }
        if (progressBar.GetComponent<Slider>().value == 1)
        {
            progressBar.GetComponent<Slider>().value = 0;
        }
        progressBar.GetComponent<CanvasGroup>().alpha = 1;
        progressBar.GetComponent<Slider>().value = percent;
        progressBar.transform.FindChild("Text").GetComponent<Text>().text = Math.Round(percent * 100, 2).ToString("F2") + "%";
        if (percent == 1)
        {
            progressBar.GetComponent<CanvasGroup>().DOFade(0, 1.0f).SetDelay(3.0f);
        }
    }

    private void checkDisappear()
    {
        check.GetComponent<CanvasGroup>().alpha = 0;
        check.GetComponent<CanvasGroup>().interactable = false;
        check.transform.FindChild("OK").GetComponent<Button>().onClick.RemoveAllListeners();
        check.transform.FindChild("Cancel").GetComponent<Button>().onClick.RemoveAllListeners();
    }
}