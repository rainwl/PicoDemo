using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ShowMessageManager : MonoBehaviour
{
    public static ShowMessageManager Instance;

    public GameObject MessagePrfab;
    public Transform MessageLayout;

    public GameObject SelectBox;
    public TextMeshProUGUI WarnText;

    Action yesAction;
    Action noAction;
    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        //if (Input.GetKeyUp(KeyCode.M))
        //{
        //    ShowMessage("0123456789abc".Substring(UnityEngine.Random.Range(0,10)));
        //}
        //if (Input.GetKeyUp(KeyCode.B))
        //{
        //    ShowSelectBox("是否删除?",null,null);
        //}
    }

    public void ShowMessage(string mes,float time=2)
    {
        GameObject go = Instantiate(MessagePrfab,MessageLayout);
        go.SetActive(true);
        TextMeshProUGUI text=go.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        text.text = mes;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(text.preferredWidth + 10, rect.sizeDelta.y);
        Destroy(go,time);
    }
    public void ShowSelectBox(string warn,Action yes,Action no=null) 
    {
        SelectBox.SetActive(true);
        WarnText.text = warn;
        yesAction = yes;
        noAction = no;
    }

    public void Yes() 
    {
        yesAction?.Invoke();
        WarnText.text = "";
        SelectBox.SetActive(false);
    }

    public void No() 
    {
        noAction?.Invoke();
        WarnText.text = "";
        SelectBox.SetActive(false);
    }
}
