﻿using System.Collections;
using System.Collections.Generic;
using Common;
using MixedReality.Toolkit.UX;
using Network.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ClientNetPanelManager : MonoBehaviour
    {
        public GameObject clientItemPrefab;
        public Transform clientList;
        public GameObject netConnectPanel;
        public TMP_InputField ipInputField;
        public PressableButton clientToggle;
        public GameObject keyboard;
        
        private IEnumerator Start()
        {
            ClientNetworkManager.Instance.OnConnectResultAction += OnConnectResult;
            ClientNetworkManager.Instance.OnDisconnectAction += OnDisconnect;
            ClientHandlerCenter.Instance.OnGetClientListAction += RefreshClientList;

            yield return new WaitForSeconds(1);
            ClientNetworkManager.Instance.ConnectServer();
        }

        private void OnDisconnect()
        {
            ipInputField.text = ClientNetworkManager.Instance.ip;
            netConnectPanel.SetActive(true);
            keyboard.SetActive(true);
            RefreshClientList(new List<ClientInfo>());
            ShowMessageManager.Instance.ShowMessage("The server is disconnected");
            clientToggle.ForceSetToggled(false);
        }

        private void OnConnectResult(bool result)
        {
            ipInputField.text = ClientNetworkManager.Instance.ip;
            netConnectPanel.SetActive(!result);
            keyboard.SetActive(!result);
            //连接成功后更新用户信息
            if (result)
            {
                SendDataManager.SendUpdateUserInfo();
                clientToggle.ForceSetToggled(true);
            }

            ShowMessageManager.Instance.ShowMessage("Server connection:" + result);
        }

        public void Connect()
        {
            if (string.IsNullOrEmpty(ipInputField.text))
            {
                print("请输入IP!");
            }
            else
            {
                ClientNetworkManager.Instance.ip = ipInputField.text;
                ClientNetworkManager.Instance.ConnectServer();
            }
        }

        public void ClientToggle(PressableButton tog)
        {
            if (tog.IsToggled)
            {
                ClientNetworkManager.Instance.ConnectServer();
            }
            else
            {
                ClientNetworkManager.Instance.DisconnectServer();
            }
        }

        public void RefreshClientList(List<ClientInfo> list)
        {
            for (int i = 0; i < clientList.childCount; i++)
            {
                Destroy(clientList.GetChild(i).gameObject);
            }

            for (int i = 0; i < list.Count; i++)
            {
                GameObject go = GameObject.Instantiate(clientItemPrefab, clientList);
                go.GetComponent<ClientItem>().SetValue(list[i].UserId, list[i].UserType, list[i].UserName, list[i].UserIP);
            }

            print("RefreshClientList:" + list.Count);
            LayoutRebuilder.ForceRebuildLayoutImmediate(clientList.GetComponent<RectTransform>());
            ShowMessageManager.Instance.ShowMessage("The client list has been updated");
        }
    }
}