using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR|| UNITY_WSA
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Sharing;
#endif
public class HoloLensAnchorManager : MonoBehaviour
{
    public static HoloLensAnchorManager Instance;
    Action<byte[]> onFinishedExprot;
    List<byte> anchorData = new List<byte>();
    GameObject objAttachAnchor;
    private void Awake()
    {
        Instance = this;
    }

    bool isExporting;
#if UNITY_EDITOR|| UNITY_WSA
    /// <summary>
    /// 导出锚点
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="onFinishedExprot"></param>
    public void ExprotAnchorData(GameObject obj,Action<byte[]>onFinishedExprot) 
    {
        if (isExporting) 
        {
            Debug.Log("正在导出中...!");
            return;
        }
        if (Application.isEditor) 
        {
            Debug.Log("非HoloLens无法导出锚点!");
            return;
        }
        if (obj == null)
        {
            Debug.Log("ExprotAnchor obj 不能为空!");
            return;
        }
        isExporting = true;
        this.onFinishedExprot =  onFinishedExprot;
        WorldAnchor anchor= obj.GetComponent<WorldAnchor>();
        if (anchor == null) 
        {
            anchor = obj.AddComponent<WorldAnchor>();
        }
        WorldAnchorTransferBatch batch = new WorldAnchorTransferBatch();
        batch.AddWorldAnchor(obj.name, anchor);
        anchorData.Clear();
        //开始异步导出锚点
        WorldAnchorTransferBatch.ExportAsync(batch,onDataAvailable,onCompleted);
        Debug.Log("开始导出锚点:"+ obj.name);
    }

    private void onDataAvailable(byte[] data)
    {
        anchorData.AddRange(data);
    }
    private void onCompleted(SerializationCompletionReason completionReason)
    {
        if (completionReason == SerializationCompletionReason.Succeeded)
        {
            onFinishedExprot?.Invoke(anchorData.ToArray());
            Debug.Log("导出锚点完成:" + anchorData.Count);
            anchorData.Clear();
            onFinishedExprot = null;
        }
        else 
        {
            Debug.Log("导出锚点失败:" + completionReason);
            ShowMessageManager.Instance.ShowMessage("导出锚点失败:" + completionReason);
        }
        isExporting = false;
    }

    bool isImporting;
    /// <summary>
    /// 导入锚点
    /// </summary>
    /// <param name="data"></param>
    /// <param name="obj"></param>
    public void ImportAnchorData(byte[] data,GameObject obj) 
    {
        if (isImporting)
        {
            Debug.Log("正在导入中...");
            return;
        }
        if (Application.isEditor)
        {
            Debug.Log("非HoloLens无法导入锚点!");
            return;
        }
        if (obj == null)
        {
            Debug.Log("ImportAnchor obj 不能为空!");
            return;
        }
        isImporting = true;
        objAttachAnchor = obj;
        WorldAnchorTransferBatch.ImportAsync(data,onComplete);
        Debug.Log("开始导入锚点:"+data.Length);
    }

    private void onComplete(SerializationCompletionReason completionReason, WorldAnchorTransferBatch deserializedTransferBatch)
    {
        if (completionReason == SerializationCompletionReason.Succeeded)
        {
            string anchorName = deserializedTransferBatch.GetAllIds()[0];
            WorldAnchor anchor= objAttachAnchor.GetComponent<WorldAnchor>();
            if (anchor != null) 
            {
                DestroyImmediate(anchor);
            }
            deserializedTransferBatch.LockObject(anchorName, objAttachAnchor);
            Debug.Log("导入锚点完成!");
        }
        else 
        {
            Debug.Log("导入锚点失败:" + completionReason);
        }
        deserializedTransferBatch.Dispose();
        isImporting = false;
    }
#else

    public void ExprotAnchorData(GameObject obj, Action<byte[]> onFinishedExprot) 
    {
    
    }
    public void ImportAnchorData(byte[] data, GameObject obj) 
    {

    }
#endif
}
