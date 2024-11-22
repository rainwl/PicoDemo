using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System;
using MrPlatform.Scripts.Network.Client;
using Network.Client;

public class ServerNetPanelManager : MonoBehaviour
{
    public TextMeshProUGUI IpText;

    void Start()
    {
        GetIp();
        SwitchIp();

        if (!IpList.Contains(ClientNetworkManager.Instance.ip)) 
        {
            ClientNetworkManager.Instance.ip = IpList[0];
        }
    }


    public void ServerToggle(Toggle tog) 
    {
        if (tog.isOn)
        {
            ServerNetworkManager.Instance.StartServer();
        }
        else 
        {
            ServerNetworkManager.Instance.StopServer();
        }
    }

    void Update()
    {

    }
    public void SwitchIp() 
    {
        if (IpList.Count > 0)
        {
            if (ipIndex > IpList.Count - 1) ipIndex = 0;
            IpText.text = IpList[ipIndex];
            ipIndex++;
        }
        else
        {
            IpText.text = "NO IP";
        }
    }

    int ipIndex;
    List<string> IpList=new List<string> ();
    void GetIp() 
    {
        string hostName = Dns.GetHostName();
        IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
        IpList.Clear();
        for (int i = 0; i < ipEntry.AddressList.Length; i++)
        {
            if (ipEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork) 
            {
                IpList.Add(ipEntry.AddressList[i].ToString());
            }
        }
    }

}
