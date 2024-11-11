using System.Collections;
using System.Collections.Generic;
using MrPlatform.Scripts.Network.Client;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClientItem : MonoBehaviour
{
    public GameObject SubPanel;
    public int Type;
    public int Id;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI IpText;
    public Image TypeImage;
    public Sprite[] TypeSprits;
    RuntimePlatform platform;
    public void SetValue(int id, int type, string name, string ip)
    {
        Id = id;
        Type = type;
        int spriteIndex = 0;
        platform = (RuntimePlatform)type;
        if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.WindowsEditor
            || platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.OSXPlayer) 
        {
            spriteIndex = 1;
        }
        else if (platform == RuntimePlatform.IPhonePlayer)
        {
            spriteIndex = 2;
        }
        else if (platform == RuntimePlatform.Android)
        {
            spriteIndex = 3;
        }
        else if (platform == RuntimePlatform.WSAPlayerX86)
        {
            spriteIndex = 4;
        }
        else if (platform == RuntimePlatform.WSAPlayerARM)
        {
            spriteIndex = 5;
        }
        NameText.text = name;
        IpText.text = ip;
        TypeImage.sprite = TypeSprits[spriteIndex];
    }

    private void OnEnable()
    { 
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetComponent<RectTransform>());
    }

    public void OnClick() 
    {
        SubPanel.SetActive(!SubPanel.activeInHierarchy);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
    }

    public void UploadAnchor() 
    {
        if (platform == RuntimePlatform.WSAPlayerX86 || platform == RuntimePlatform.WSAPlayerARM)
        {
            ARManager.Instance.UploadAnchor();
        }
        else
        {
            ShowMessageManager.Instance.ShowMessage("请选择HoloLens设备!");
        }
    }

    public void DownLoadAnchor()
    {
        if (platform == RuntimePlatform.WSAPlayerX86 || platform == RuntimePlatform.WSAPlayerARM)
        {
            ARManager.Instance.DownLoadAnchor();
        }
        else
        {
            ShowMessageManager.Instance.ShowMessage("请选择HoloLens设备!");
        }
    }
    public void Callibration() 
    {
        if (Id == ClientNetworkManager.Instance.UserId || ServerNetworkManager.Instance==null || !ServerNetworkManager.Instance.IsStartServer) 
        {
            ShowMessageManager.Instance.ShowMessage("无权限!");
            return;
        }

        ShowMessageManager.Instance.ShowSelectBox("校准客户端?",()=> 
        {
            ARManager.Instance.SendCallibrationRequest(Id);   
        });
    
    }
}
