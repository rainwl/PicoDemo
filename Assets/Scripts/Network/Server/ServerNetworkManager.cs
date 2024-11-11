using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerNetworkManager : MonoBehaviour
{
    public static ServerNetworkManager Instance;
    SocketServer socketServer;
    ServerHandlerCenter handlerCenter;
    HeartCheck heartCheck;

    public int Port = 6650;
    public int MaxClient=20;

    public bool IsStartServer {private set; get; }

    public bool IsClientListUpdate;

    public Action<List<ClientInfo>> OnClientListUpdate;
    /// <summary>
    /// 当客户端断开连接
    /// </summary>
    /// <param name="token"></param>
    /// <param name="error"></param>
    internal void OnClientClose(UserToken token, string error)
    {
        print("OnClientClose:"+ token.UserName+","+error);
        IsClientListUpdate = true;
    }
    /// <summary>
    /// 当客户端连接
    /// </summary>
    /// <param name="token"></param>
    internal void OnClientConnect(UserToken token)
    {
        print("OnClientConnect:"+ token.UserName);
        IsClientListUpdate = true;
    }

    private void Awake()
    {
        Instance = this; 
    }
    void Start()
    {
        StartServer();
    }
    /// <summary>
    /// 开启服务器
    /// </summary>
    public void StartServer() 
    {
        if (IsStartServer) return;

        IsStartServer = true;
        handlerCenter = new ServerHandlerCenter(this);
        socketServer = new SocketServer(handlerCenter);
        heartCheck = new HeartCheck(socketServer,15);

        socketServer.Start(MaxClient, Port);
        print("server start port:"+Port);
    }

    public void OnDestroy()
    {
        StopServer();
    }
    /// <summary>
    /// 关闭服务器
    /// </summary>
    public void StopServer() 
    {
        if (!IsStartServer) return;
        IsStartServer = false;
        socketServer.Stop();
        heartCheck.Close();
        print("server stop!");
    }
    /// <summary>
    /// 获取客户端列表
    /// </summary>
    /// <returns></returns>
    internal List<UserToken> GetClientList()
    {
        return socketServer.GetUserTokenList();
    }

    void Update()
    {
        if (IsClientListUpdate) 
        {
            IsClientListUpdate = false;
            List<UserToken> userTokenList = GetClientList();
            List<ClientInfo> list = new List<ClientInfo>();
            for (int i = 0; i < userTokenList.Count; i++)
            {
                ClientInfo info = new ClientInfo();
                info.UserId = userTokenList[i].UserId;
                info.UserName = userTokenList[i].UserName;
                info.UserType = userTokenList[i].UserType;
                info.UserIP = userTokenList[i].Client.RemoteEndPoint.ToString();
                list.Add(info);
            }
            OnClientListUpdate?.Invoke(list);

            //通知客户端更新列表信息
            DataModel model = new DataModel(DataType.TPYE_MR,DataRequest.MR_GET_CLIENTLIST,MessageCodec.ObjectToBytes(list));
            byte[] data = DataCodec.Encode(model);
            for (int i = 0; i < userTokenList.Count; i++)
            {
                userTokenList[i].Send(data);
            }
        }
    }
}
