using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System;
using MrPlatform.Scripts.Network.Client;

public class ClientNetPanelManager : MonoBehaviour
{
    public GameObject ClientItenPrefab;
    public Transform ClientContent;

    public GameObject NetConnectPanel;
    public TMP_InputField IpInputField;

    public Toggle ClientTog;

    IEnumerator Start()
    {
        ClientNetworkManager.Instance.OnConnectResultAction += OnConnectResult;
        ClientNetworkManager.Instance.OnDisconnectAction += OnDisconnect;
        ClientHandlerCenter.Instance.OnGetClientListAction += RefreshClientList;

        yield return new WaitForSeconds(1);
        ClientNetworkManager.Instance.ConnectServer();
    }

    private void OnDisconnect()
    {
        IpInputField.text = ClientNetworkManager.Instance.IP;
        NetConnectPanel.SetActive(true);
        RefreshClientList(new List<ClientInfo>());
        ShowMessageManager.Instance.ShowMessage("服务器连接已断开!");
        ClientTog.isOn = false;
    }

    private void OnConnectResult(bool result)
    {
        IpInputField.text = ClientNetworkManager.Instance.IP;
        NetConnectPanel.SetActive(!result);
        //连接成功后更新用户信息
        if (result) 
        {
            SendDataManager.Instance.SendUpdateUserInfo();
            ClientTog.isOn = true;
        }
        ShowMessageManager.Instance.ShowMessage("服务器连接:"+result);
    }

    public void Connect() 
    {
        if (string.IsNullOrEmpty(IpInputField.text))
        {
            print("请输入IP!");
        }
        else 
        {
            ClientNetworkManager.Instance.IP = IpInputField.text;
            ClientNetworkManager.Instance.ConnectServer();
        }
    }

    public void ClientToggle(Toggle tog)
    {
        if (tog.isOn)
        {
            ClientNetworkManager.Instance.ConnectServer();
        }
        else
        {
            ClientNetworkManager.Instance.DisconnectServer();
        }
    }
    void Update()
    {

    }
   
    public void RefreshClientList(List<ClientInfo> list) 
    {
        for (int i = 0; i < ClientContent.childCount; i++)
        {
            Destroy(ClientContent.GetChild(i).gameObject);
        }
        for (int i = 0; i < list.Count; i++)
        {
            GameObject go = GameObject.Instantiate(ClientItenPrefab, ClientContent);
            go.GetComponent<ClientItem>().SetValue(list[i].UserId, list[i].UserType,list[i].UserName, list[i].UserIP);
        }
        print("RefreshClientList:"+list.Count);
        LayoutRebuilder.ForceRebuildLayoutImmediate(ClientContent.GetComponent<RectTransform>());
        ShowMessageManager.Instance.ShowMessage("客户端列表已更新!");
    }

}
