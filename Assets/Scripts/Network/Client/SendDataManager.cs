using MrPlatform.Scripts.Network.Client;
using UnityEngine;

namespace Network.Client
{
    public class SendDataManager : MonoBehaviour
    {
        public static SendDataManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void SendDownloadAnchor()
        {
            DataModel model = new DataModel(DataType.TPYE_MR, DataRequest.MR_DOWNLOAD_ANCHOR);
            ClientNetworkManager.Instance.Send(model);
        }

        public void SendUploadAnchor(byte[] data)
        {
            DataModel model = new DataModel(DataType.TPYE_MR, DataRequest.MR_UPLOAD_ANCHOR, data);
            ClientNetworkManager.Instance.Send(model);
        }

        public void SendBroadcastAll(BroadcastInfo info)
        {
            DataModel model = new DataModel(DataType.TYPE_BROADCAST, DataRequest.BROADCAST_ALL, MessageCodec.ObjectToBytes(info));
            ClientNetworkManager.Instance.Send(model);
        }

        public void SendBroadcastById(BroadcastInfo info)
        {
            DataModel model = new DataModel(DataType.TYPE_BROADCAST, DataRequest.BROADCAST_BY_ID, MessageCodec.ObjectToBytes(info));
            ClientNetworkManager.Instance.Send(model);
        }

        public void SendUpdateUserInfo()
        {
            ClientInfo info = new ClientInfo();
            info.UserType = (int)Application.platform;
            info.UserName = SystemInfo.deviceName;
            byte[] data = MessageCodec.ObjectToBytes(info);
            DataModel model = new DataModel(DataType.TPYE_MR, DataRequest.MR_UPDATE_USERINFO, data);
            ClientNetworkManager.Instance.Send(model);
        }

        public void SendGetUserList()
        {
            DataModel model = new DataModel(DataType.TPYE_MR, DataRequest.MR_GET_CLIENTLIST);
            ClientNetworkManager.Instance.Send(model);
        }
    }
}