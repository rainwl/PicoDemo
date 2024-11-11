using System;
using System.Collections;
using System.Collections.Generic;

public class ServerHandlerCenter : AbsHandlerCenter
{
    ServerNetworkManager serverNetworkManager;
    public byte[] AnchorData {get;private set; }
    public ServerHandlerCenter(ServerNetworkManager manager) 
    {
        serverNetworkManager = manager;
    }
    public override void ClientClose(UserToken token, string error)
    {
        serverNetworkManager.OnClientClose(token,error);
    }

    public override void ClientConnect(UserToken token)
    {
        serverNetworkManager.OnClientConnect(token);
    }

    public override void MessageReceive(UserToken token, byte[] data)
    {
        DataModel model= DataCodec.Decode(data);
        switch (model.Type) 
        {
            case DataType.TYPE_NONE:
                break;
            case DataType.TPYE_MR:
                HandlerMrRequest(token, model);
                break;
            case DataType.TYPE_BROADCAST:
                HandlerBroadcastRequest(token, model);
                break;

        } 
    }

    void HandlerMrRequest(UserToken token,DataModel model) 
    {

        switch (model.Request)
        {
            case DataRequest.MR_UPDATE_USERINFO:
                UpdateUserInfo(token, model); 
                break;
            case DataRequest.MR_GET_CLIENTLIST:
                GetClientList(token, model);
                break;
            case DataRequest.MR_UPLOAD_ANCHOR:
                UploadAnchor(token, model);
                break;
            case DataRequest.MR_DOWNLOAD_ANCHOR:
                DownloadAnchor(token, model);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    /// <param name="token"></param>
    /// <param name="model"></param>
    private void UpdateUserInfo(UserToken token, DataModel model)
    {
        ClientInfo client = MessageCodec.BytesToObject<ClientInfo>(model.Message);
        token.UserName = client.UserName;
        token.UserType = client.UserType;

        model.Message = BitConverter.GetBytes(token.UserId);
        Send(token, model);

        serverNetworkManager.IsClientListUpdate = true;
    }

    /// <summary>
    /// 获取客户端列表
    /// </summary>
    /// <param name="token"></param>
    /// <param name="model"></param>
    private void GetClientList(UserToken token, DataModel model)
    {
        List<ClientInfo> list = new List<ClientInfo>();
        List<UserToken> userList = serverNetworkManager.GetClientList();
        for (int i = 0; i < userList.Count; i++)
        {
            ClientInfo info = new ClientInfo();
            info.UserId = userList[i].UserId;
            info.UserType = userList[i].UserType;
            info.UserName = userList[i].UserName;
            info.UserIP = userList[i].Client.RemoteEndPoint.ToString();
            list.Add(info);
        }
        model.Message = MessageCodec.ObjectToBytes(list);
        Send(token, model);
    }
    /// <summary>
    /// 下载锚点
    /// </summary>
    /// <param name="token"></param>
    /// <param name="model"></param>
    private void UploadAnchor(UserToken token, DataModel model)
    {
        AnchorData = model.Message;
        model.Message = BitConverter.GetBytes((int)Result.Success);
        Send(token, model);
    }

    /// <summary>
    /// 上传锚点
    /// </summary>
    /// <param name="token"></param>
    /// <param name="model"></param>
    private void DownloadAnchor(UserToken token, DataModel model)
    {
        if (AnchorData != null)
        {
            model.Message = AnchorData;
            Send(token, model);
        }
        else 
        {
            model.Message = BitConverter.GetBytes((int)Result.None);
            Send(token, model);
        }
    }
    void HandlerBroadcastRequest(UserToken token, DataModel model)
    {
        switch (model.Request)
        {
            case DataRequest.BROADCAST_ALL:
                BroadcastAll(token,model);
                break;
            case DataRequest.BROADCAST_OTHER:
                BroadcastOther(token, model);
                break;
            case DataRequest.BROADCAST_BY_ID:
                BroadcastById(token, model);
                break;
            default:
                break;
        }

    }
    /// <summary>
    /// 向所有客户端发送
    /// </summary>
    /// <param name="token"></param>
    /// <param name="model"></param>
    private void BroadcastAll(UserToken token, DataModel model)
    {
        List<UserToken> list = serverNetworkManager.GetClientList();
        byte[] data = DataCodec.Encode(model);
        for (int i = 0; i < list.Count; i++)
        {
            list[i].Send(data);
        }
    }
    /// <summary>
    /// 向其他客户端发送
    /// </summary>
    /// <param name="token"></param>
    /// <param name="model"></param>
    private void BroadcastOther(UserToken token, DataModel model)
    {
        List<UserToken> list = serverNetworkManager.GetClientList();
        byte[] data = DataCodec.Encode(model);
        for (int i = 0; i < list.Count; i++)
        {
            if(token!= list[i])
            list[i].Send(data);
        }
    }
    /// <summary>
    /// 发送给Id客户端
    /// </summary>
    /// <param name="token"></param>
    /// <param name="model"></param>
    private void BroadcastById(UserToken token, DataModel model)
    {
        List<UserToken> list = serverNetworkManager.GetClientList();
        BroadcastInfo info = MessageCodec.BytesToObject<BroadcastInfo>(model.Message);

        byte[] data = DataCodec.Encode(model);
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].UserId== info.PeerId)
                list[i].Send(data);
        }
    }
    void Send(UserToken token, DataModel model) 
    {
        byte[] data = DataCodec.Encode(model);
        token.Send(data);
    }
}
