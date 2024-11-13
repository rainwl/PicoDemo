using Dock;
using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using MrPlatform.Scripts.Network.Client;
using UnityEngine;

public class HandlerBroadcastManager : MonoBehaviour
{
   
    void Start()
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
                CrteateAssetByID(info.Data);
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
                ARManager.Instance.OnRequestCallibration(info);
                break;
            case BroadcastType.ConfirmCallibration:
                ARManager.Instance.OnConfirmCallibration(info.UserId);
                break;
            case BroadcastType.CallibrationData:
                ARManager.Instance.OnCallibrationData(info);
                break;
            case BroadcastType.StartHoloview:
                ARManager.Instance.OnStartHoloview(info);
                break;
            case BroadcastType.StopHoloview:
                ARManager.Instance.OnStopHoloview(info);
                break;
            case BroadcastType.HoloviewData:
                ARManager.Instance.OnHoloviewData(info);
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
    private void DockByID(byte[] data)
    {
        long id = BitConverter.ToInt64(data, 0);
        UpdateTransform updateTransform = UpdateTransformManager.Instance.TransformItmeList.Find((UpdateTransform t) => { return t.updateID == id; });

        //这里是自己加的
        // DockPosition Pos = updateTransform.GetComponent<DockPosition>();
        // updateTransform.GetComponent<Dockable>().Dock(Pos);
    }

    private void UndockByID(byte[] data)
    {
        // long id = BitConverter.ToInt64(data, 0);
        // UpdateTransform updateTransform = UpdateTransformManager.Instance.TransformItmeList.Find((UpdateTransform t) => { return t.updateID == id; });
        // updateTransform.GetComponent<Dockable>().Undock();
    }
    private void PlayAniNextByID(byte[] data)
    {
        long id = BitConverter.ToInt64(data, 0);
        UpdateTransform updateTransform = UpdateTransformManager.Instance.TransformItmeList.Find((UpdateTransform t) => { return t.updateID == id; });
        updateTransform.GetComponent<PlayAnimation>().PlayAniNext();
    }
    private void PlayAniLastByID(byte[] data)
    {
        long id = BitConverter.ToInt64(data, 0);
        UpdateTransform updateTransform = UpdateTransformManager.Instance.TransformItmeList.Find((UpdateTransform t) => { return t.updateID == id; });
        updateTransform.GetComponent<PlayAnimation>().PlayAniLast();
    }

    private void RemoveAllAsset()
    {
        for (int i = 0; i < UpdateTransformManager.Instance.TransformItmeList.Count; i++)
        {
            Destroy(UpdateTransformManager.Instance.TransformItmeList[i].gameObject);
        }
        UpdateTransformManager.Instance.TransformItmeList.Clear();
        Resources.UnloadUnusedAssets();
    }
    private void RemoveAssetByID(byte[] data)
    {
        long id = BitConverter.ToInt64(data,0);
        UpdateTransform updateTransform=UpdateTransformManager.Instance.TransformItmeList.Find((UpdateTransform t)=> { return t.updateID == id; });
        if (updateTransform != null) Destroy(updateTransform.gameObject);
        Resources.UnloadUnusedAssets();
    }

    private void UpdateTransform(byte[] data)
    {
        UpdateTransformManager.Instance.UpdateTransform(data);
    }

    private void CrteateAssetByID(byte[] data)
    {
        ARManager.Instance.CrteateAssetByID(data);
    }

    void Update()
    {
        
    }
}
