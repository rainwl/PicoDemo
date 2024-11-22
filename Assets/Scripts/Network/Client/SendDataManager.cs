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

        public static void SendDownloadAnchor()
        {
            var model = new DataModel(DataType.TPYE_MR, DataRequest.MR_DOWNLOAD_ANCHOR);
            ClientNetworkManager.Instance.Send(model);
        }

        public static void SendUploadAnchor(byte[] data)
        {
            var model = new DataModel(DataType.TPYE_MR, DataRequest.MR_UPLOAD_ANCHOR, data);
            ClientNetworkManager.Instance.Send(model);
        }

        public static void SendBroadcastAll(BroadcastInfo info)
        {
            var model = new DataModel(DataType.TYPE_BROADCAST, DataRequest.BROADCAST_ALL, MessageCodec.ObjectToBytes(info));
            ClientNetworkManager.Instance.Send(model);
        }

        public static void SendBroadcastById(BroadcastInfo info)
        {
            DataModel model = new DataModel(DataType.TYPE_BROADCAST, DataRequest.BROADCAST_BY_ID, MessageCodec.ObjectToBytes(info));
            ClientNetworkManager.Instance.Send(model);
        }

        public static void SendUpdateUserInfo()
        {
            var info = new ClientInfo
            {
                UserType = (int)Application.platform,
                UserName = SystemInfo.deviceName
            };
            var data = MessageCodec.ObjectToBytes(info);
            var model = new DataModel(DataType.TPYE_MR, DataRequest.MR_UPDATE_USERINFO, data);
            ClientNetworkManager.Instance.Send(model);
        }

        public void SendGetUserList()
        {
            var model = new DataModel(DataType.TPYE_MR, DataRequest.MR_GET_CLIENTLIST);
            ClientNetworkManager.Instance.Send(model);
        }
    }
}