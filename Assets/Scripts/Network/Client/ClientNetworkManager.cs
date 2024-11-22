using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Network.Client
{
    public class ClientNetworkManager : MonoBehaviour
    {
        public static ClientNetworkManager Instance;
        private SocketClient _client;
        public string ip;
        public int port;
        public string ipKey = "IpKey";
        private bool _isOnConnectResult;
        private bool _isOnDisconnect;
        public Action<bool> OnConnectResultAction;
        public Action OnDisconnectAction;
        public int UserId { get; internal set; }

        public bool isAutoConnect;
        public int autoConnectTime = 20;
        private bool _connectResult;

        public readonly Queue<byte[]> ReceiveDataQueue = new();

        private void Awake()
        {
            Instance = this;
            Init();
            if (PlayerPrefs.HasKey(ipKey))
            {
                ip = PlayerPrefs.GetString(ipKey);
                print("PlayerPrefs get ip:" + ip);
            }
        }

        private void Start()
        {
            if (isAutoConnect)
            {
                StartCoroutine(AutoConnect());
            }
        }

        private void Init()
        {
            _client = new SocketClient();
            _client.OnConnect += OnConnect;
            _client.OnDisconnect += OnDisconnect;
            _client.OnReceiveData += OnReceiveData;
        }


        public void ConnectServer()
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(ip, out ipAddress) || port < 0 || port > 65535)
            {
                Debug.LogError("ip or port is wrong!");
                return;
            }

            _client.ConnectServer(ip, port);
        }

        public void DisconnectServer()
        {
            _client.Close();
        }

        private IEnumerator AutoConnect()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoConnectTime);
                print("auto connect");
                if (!_client.Connected) ConnectServer();
            }
        }


        internal void Send(DataModel model)
        {
            if (_client.Connected)
            {
                _client.Send(DataCodec.Encode(model));
            }
            else
            {
                print("offline!");
            }
        }


        private void OnReceiveData(byte[] data)
        {
            lock (ReceiveDataQueue)
            {
                ReceiveDataQueue.Enqueue(data);
            }
        }


        private void OnDisconnect()
        {
            print("OnDisconnect");
            _isOnDisconnect = true;
        }

        private void OnDestroy()
        {
            DisconnectServer();
        }

        private void OnConnect(bool result)
        {
            print("OnConnect:" + result);
            _connectResult = result;
            _isOnConnectResult = true;
            if (result)
            {
                PlayerPrefs.SetString(ipKey, ip);
                PlayerPrefs.Save();
                print("PlayerPrefs save ip:" + ip);
                StartCoroutine(SendHeart());
            }
        }

        private IEnumerator SendHeart()
        {
            print("start heart...");
            while (_client.Connected)
            {
                Send(new DataModel());
                yield return new WaitForSeconds(2);
            }

            print("stop heart!");
        }

        private void Update()
        {
            if (_isOnDisconnect)
            {
                _isOnDisconnect = false;
                OnDisconnectAction?.Invoke();
            }

            if (_isOnConnectResult)
            {
                _isOnConnectResult = false;
                OnConnectResultAction?.Invoke(_connectResult);
            }
        }
    }
}