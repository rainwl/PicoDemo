using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace MrPlatform.Scripts.Network.Client
{
    public class ClientNetworkManager : MonoBehaviour
    {
        public static ClientNetworkManager Instance;
        SocketClient client;
        public string IP;
        public int Port;
        public string IpKey = "IpKey";
        bool isOnConnectResult; 
        bool isOnDisconnect;
        public Action<bool> OnConnectResultAction;
        public Action OnDisconnectAction;
        public int UserId { get; internal set; }

        public bool isAutoConnect;
        public int AutoConnectTime = 20;

        public Queue<byte[]> ReceiveDataQueue = new Queue<byte[]>();
        private void Awake()
        {
            Instance = this;
            Init();
            if (PlayerPrefs.HasKey(IpKey)) 
            {
                IP = PlayerPrefs.GetString(IpKey);
                print("PlayerPrefs get ip:"+ IP);
            }
        }

        void Start()
        {   
            if (isAutoConnect) 
            {
                StartCoroutine(AutoConnect());
            }

        }

        /// <summary>
        /// 实例化客户端
        /// </summary>
        public void Init() 
        {
            client = new SocketClient();
            client.OnConnect += OnConnect;
            client.OnDisconnect += OnDisconnect;
            client.OnReceiveData += OnReceiveData;
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        public void ConnectServer() 
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(IP, out ipAddress)|| Port<0 || Port > 65535) 
            {
                Debug.LogError("ip or port is wrong!");
                return;
            }
            client.ConnectServer(IP,Port);
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisconnectServer() 
        {
            client.Close();
        }

        /// <summary>
        /// 检测断开连接后自动连接
        /// </summary>
        /// <returns></returns>
        IEnumerator AutoConnect() 
        {
            while (true)
            {
                yield return new WaitForSeconds(AutoConnectTime);
                if (!client.Connected) ConnectServer();
            }
        }

        /// <summary>
        /// 向服务器发送数据
        /// </summary>
        /// <param name="model"></param>
        internal void Send(DataModel model)
        {
            if (client.Connected)
            {
                client.Send(DataCodec.Encode(model));
            }
            else 
            {
                print("offline!");
            }
       
        }

        /// <summary>
        /// 收到消息回调
        /// </summary>
        /// <param name="data"></param>
        private void OnReceiveData(byte[] data)
        {
            lock (ReceiveDataQueue)
            {
                ReceiveDataQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// 断开连接回调
        /// </summary>
        private void OnDisconnect()
        {
            print("OnDisconnect");
            isOnDisconnect = true;
        }

        private void OnDestroy()
        {
            DisconnectServer();
        }

        bool connectResult;


        /// <summary>
        /// 连接结果回调
        /// </summary>
        /// <param name="result"></param>
        private void OnConnect(bool result)
        {
            print("OnConnect:"+result);
            connectResult = result;
            isOnConnectResult = true;
            if (result) 
            {
                PlayerPrefs.SetString(IpKey,IP);
                PlayerPrefs.Save();
                print("PlayerPrefs save ip:"+IP);
                StartCoroutine(SendHeart());
            }
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        /// <returns></returns>
        IEnumerator SendHeart() 
        {
            print("start heart...");
            while (client.Connected)
            {
                Send(new DataModel());
                yield return new WaitForSeconds(2);
            }
            print("stop heart!");
        }

        void Update()
        {
            if (isOnDisconnect) 
            {
                isOnDisconnect = false;
                OnDisconnectAction?.Invoke();
            }
            if (isOnConnectResult) 
            {
                isOnConnectResult = false;
                OnConnectResultAction?.Invoke(connectResult);
            }
        }
    }
}
