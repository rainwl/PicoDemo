using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Network.Client
{
    public class ClientHandlerCenter : MonoBehaviour
    {
        public static ClientHandlerCenter Instance;
        public Action<List<ClientInfo>> OnGetClientListAction;
        public Action<BroadcastInfo> OnHandlerBroadcastRequestAction;
        public Action<byte[]> OnOnDownloadAnchorData;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (ClientNetworkManager.Instance.ReceiveDataQueue.Count > 0)
            {
                var data = ClientNetworkManager.Instance.ReceiveDataQueue.Dequeue();
                HandlerData(data);
            }
        }

        private void HandlerData(byte[] data)
        {
            var model = DataCodec.Decode(data);

            switch (model.Type)
            {
                case DataType.TYPE_NONE:
                    break;
                case DataType.TPYE_MR:
                    HandlerMrRequest(model);
                    break;
                case DataType.TYPE_BROADCAST:
                    HandlerBroadcastRequest(model);
                    break;
            }
        }

        private void HandlerMrRequest(DataModel model)
        {
            switch (model.Request)
            {
                case DataRequest.MR_UPDATE_USERINFO:
                    OnUpdateUserInfo(model);
                    break;
                case DataRequest.MR_GET_CLIENTLIST:
                    OnGetClientList(model);
                    break;
                case DataRequest.MR_UPLOAD_ANCHOR:
                    OnUploadAnchor(model);
                    break;
                case DataRequest.MR_DOWNLOAD_ANCHOR:
                    OnDownloadAnchor(model);
                    break;
                default:
                    break;
            }
        }

        private static void OnUpdateUserInfo(DataModel model)
        {
            var id = BitConverter.ToInt32(model.Message, 0);
            ClientNetworkManager.Instance.UserId = id;
        }

        private void OnGetClientList(DataModel model)
        {
            var list = MessageCodec.BytesToObject<List<ClientInfo>>(model.Message);
            print("list count:" + list.Count);
            foreach (var t in list)
            {
                print("client id:" + t.UserId + ",type:" + (RuntimePlatform)t.UserType + ",ip" + t.UserIP);
            }

            OnGetClientListAction?.Invoke(list);
        }

        private static void OnUploadAnchor(DataModel model)
        {
            var result = (Result)BitConverter.ToInt32(model.Message, 0);
            print("OnUploadAnchor result:" + result);
            // ShowMessageManager.Instance.ShowMessage("上传锚点:" + result);
        }

        private void OnDownloadAnchor(DataModel model)
        {
            if (model.Message.Length == 4)
            {
                var result = (Result)BitConverter.ToInt32(model.Message, 0);
                print("OnDownloadAnchor result:" + result);
                // ShowMessageManager.Instance.ShowMessage("锚点下载:" + result);
            }
            else if (model.Message.Length > 4)
            {
                // ShowMessageManager.Instance.ShowMessage("下载成功!");
                print("OnDownloadAnchor length:" + model.Message.Length);
                OnOnDownloadAnchorData?.Invoke(model.Message);
            }
        }

        private void HandlerBroadcastRequest(DataModel model)
        {
            //print("HandlerBroadcastRequest:" + model.Message.Length);
            var info = MessageCodec.BytesToObject<BroadcastInfo>(model.Message);
            OnHandlerBroadcastRequestAction?.Invoke(info);
        }
    }
}