using System;
using MR;
using Network.Client;
using UnityEngine;

namespace Common
{
    public class HandlerBroadcastManager : MonoBehaviour
    {
        private void Start()
        {
            ClientHandlerCenter.Instance.OnHandlerBroadcastRequestAction += HandlerBroadcastRequest;
        }

        private void HandlerBroadcastRequest(BroadcastInfo info)
        {
            switch ((BroadcastType)info.Type)
            {
                case BroadcastType.UpdateTransform:
                    UpdateTransform(info.Data);
                    break;
                case BroadcastType.CrteateAssetByID:
                    CreateAssetByID(info.Data);
                    break;
                case BroadcastType.RemoveAssetByID:
                    RemoveAssetByID(info.Data);
                    break;
                case BroadcastType.RemoveAllAsset:
                    RemoveAllAsset();
                    break;
                case BroadcastType.PlayAniNextByID:
                    PlayAniNextByID(info.Data);
                    break;
                case BroadcastType.PlyAniLastByID:
                    PlayAniLastByID(info.Data);
                    break;
                case BroadcastType.RequestCallibration:
                    ARManager.Instance.OnRequestCalibration(info);
                    break;
                case BroadcastType.ConfirmCallibration:
                    ARManager.OnConfirmCalibration(info.UserId);
                    break;
                case BroadcastType.CallibrationData:
                    ARManager.Instance.OnCalibrationData(info);
                    break;
                case BroadcastType.StartHoloview:
                    ARManager.OnStartHoloView(info);
                    break;
                case BroadcastType.StopHoloview:
                    ARManager.OnStopHoloView(info);
                    break;
                case BroadcastType.HoloviewData:
                    ARManager.OnHoloViewData(info);
                    break;
                case BroadcastType.DockByID:
                    DockByID(info.Data);
                    break;
                case BroadcastType.UndockByID:
                    UndockByID(info.Data);
                    break;
                default:
                    break;
            }
        }

        private static void DockByID(byte[] data)
        {
            var id = BitConverter.ToInt64(data, 0);
            var updateTransform = UpdateTransformManager.Instance.TransformItmeList.Find((UpdateTransform t) => { return t.updateID == id; });

            //这里是自己加的
            // DockPosition Pos = updateTransform.GetComponent<DockPosition>();
            // updateTransform.GetComponent<Dockable>().Dock(Pos);
        }

        private static void UndockByID(byte[] data)
        {
            // long id = BitConverter.ToInt64(data, 0);
            // UpdateTransform updateTransform = UpdateTransformManager.Instance.TransformItmeList.Find((UpdateTransform t) => { return t.updateID == id; });
            // updateTransform.GetComponent<Dockable>().Undock();
        }

        private static void PlayAniNextByID(byte[] data)
        {
            long id = BitConverter.ToInt64(data, 0);
            UpdateTransform updateTransform = UpdateTransformManager.Instance.TransformItmeList.Find((UpdateTransform t) => { return t.updateID == id; });
            updateTransform.GetComponent<PlayAnimation>().PlayAniNext();
        }

        private static void PlayAniLastByID(byte[] data)
        {
            long id = BitConverter.ToInt64(data, 0);
            UpdateTransform updateTransform = UpdateTransformManager.Instance.TransformItmeList.Find((UpdateTransform t) => { return t.updateID == id; });
            updateTransform.GetComponent<PlayAnimation>().PlayAniLast();
        }

        private static void RemoveAllAsset()
        {
            for (int i = 0; i < UpdateTransformManager.Instance.TransformItmeList.Count; i++)
            {
                Destroy(UpdateTransformManager.Instance.TransformItmeList[i].gameObject);
            }

            UpdateTransformManager.Instance.TransformItmeList.Clear();
            Resources.UnloadUnusedAssets();
        }

        private static void RemoveAssetByID(byte[] data)
        {
            long id = BitConverter.ToInt64(data, 0);
            UpdateTransform updateTransform = UpdateTransformManager.Instance.TransformItmeList.Find((UpdateTransform t) => { return t.updateID == id; });
            if (updateTransform != null) Destroy(updateTransform.gameObject);
            Resources.UnloadUnusedAssets();
        }

        private static void UpdateTransform(byte[] data)
        {
            UpdateTransformManager.Instance.UpdateTransform(data);
        }

        private static void CreateAssetByID(byte[] data)
        {
            ARManager.Instance.CreateAssetByID(data);
            print("ARManager.Instance.CreateAssetByID(data);");
        }
    }
}