﻿using System;
using System.Collections;
using Common;
using UnityEngine;
// using TMPro;
// using Vuforia;
// using Microsoft.MixedReality.Toolkit.UI;
// using Microsoft.MixedReality.Toolkit.Utilities;
// using Dock;
using MrPlatform.Scripts.Network.Client;

public class ARManager : MonoBehaviour
{
    public static ARManager Instance;
    public Transform AnchorRoot;
    // public VuforiaBehaviour Vuforia;
    // public ImageTargetBehaviour ImageTarget_Callibration;
    public int ScanOutTime = 15;
    private void Awake()
    {
        Instance = this;
    }
    public void OnRequestCallibration(BroadcastInfo info) 
    {
        //ShowMessageManager.Instance.ShowSelectBox("开始扫描校准?",()=> { StartCallibration(info); });  
        PlaySoundManager.Instance.PlayEffect("StartScan");
        StartCallibration(info);
    }

    int serverClientId;
    void StartCallibration(BroadcastInfo info)
    {
        print("StartARCallibration");
        float height = BitConverter.ToSingle(info.Data, 0);
        serverClientId = info.UserId;

        if (height < 0.1f)
        {
            ShowMessageManager.Instance.ShowMessage("屏幕高度不正确!");
            return;
        }
        try
        {
            // ImageTarget_Callibration.SetHeight(0.01f*height);
            // Vuforia.enabled = true;
            // Debug.Log("ImageTarget Size W:"+ ImageTarget_Callibration.GetSize().x+",H:" 
            //     + ImageTarget_Callibration.GetSize().y);
            // StartCoroutine(CheckScan());
        }
        catch (Exception e)
        {
            Debug.LogError("StartCallibration error:" + e.Message);
        }
    }
    IEnumerator CheckScan() 
    {
        isScanning = false;
        yield return new WaitForSeconds(2);
        isScanning = true;
        yield return new WaitForSeconds(ScanOutTime);
        if (isScanning) 
        {
            Debug.Log("扫描超时!");
            ShowMessageManager.Instance.ShowMessage("扫描超时!");
            StopScan();
        }
    }
    bool isScanning;
    public void OnFinishedScan(string targetName)
    {
        if (!isScanning) return;
        if (targetName == "Calibration") 
        {
            PlaySoundManager.Instance.PlayEffect("FinishScan");
            Debug.Log("OnFinishedScan");
            // trackedPos = ImageTarget_Callibration.transform.position;
            // trackedRot = ImageTarget_Callibration.transform.rotation;
            StopScan();
            ConfirmCallibration();
        }
    }
    void StopScan() 
    {
        isScanning = false;
        // Vuforia.enabled = false;
    }

    Vector3 trackedPos;
    Quaternion trackedRot;
    public  GameObject CallibrationCube;
    public void ConfirmCallibration()
    {
        GameObject go = GameObject.Instantiate(CallibrationCube);
        go.SetActive(true);
        // go.transform.localScale = new Vector3(ImageTarget_Callibration.GetSize().x,1, ImageTarget_Callibration.GetSize().y);
        go.transform.rotation = trackedRot;
        go.transform.position = trackedPos;
        Destroy(go, 10);

        //发送消息给你服务端获取服务端设备数据
        BroadcastInfo info = new BroadcastInfo();
        info.Type = (int)BroadcastType.ConfirmCallibration;
        info.UserId = ClientNetworkManager.Instance.UserId;
        info.PeerId = serverClientId;
        SendDataManager.Instance.SendBroadcastById(info);
    }

    public GameObject AniButtons;
    UpdateTransform selectedUpdateTransform;
    float lastTime;
    internal void ShowAniButtons(UpdateTransform updateTransform, Vector3 position,Quaternion rot)
    {
        lastTime = Time.time;
        AniButtons.SetActive(true);
        AniButtons.transform.position = position;
        AniButtons.transform.rotation = rot;
        selectedUpdateTransform = updateTransform;
    }

    public void PlayAniNext()
    {
        BroadcastInfo info = new BroadcastInfo();
        info.Type = (int)BroadcastType.PlayAniNextByID;
        info.Data = BitConverter.GetBytes(selectedUpdateTransform.updateID);
        SendDataManager.Instance.SendBroadcastAll(info);
    }
    public void PlayAniLast()
    {
        BroadcastInfo info = new BroadcastInfo();
        info.Type = (int)BroadcastType.PlyAniLastByID;
        info.Data = BitConverter.GetBytes(selectedUpdateTransform.updateID);
        SendDataManager.Instance.SendBroadcastAll(info);
    }
    public void Dock()
    {
        BroadcastInfo info = new BroadcastInfo();
        info.Type = (int)BroadcastType.DockByID;
        info.Data = BitConverter.GetBytes(selectedUpdateTransform.updateID);
        SendDataManager.Instance.SendBroadcastAll(info);
    }

    public void Undock()
    {
        BroadcastInfo info = new BroadcastInfo();
        info.Type = (int)BroadcastType.UndockByID;
        info.Data = BitConverter.GetBytes(selectedUpdateTransform.updateID);
        SendDataManager.Instance.SendBroadcastAll(info);
    }
    private void Update()
    {
        if(Time.time - lastTime > 5)
        {
            AniButtons.SetActive(false);
        }
    }

    //收到服务端发来的校准数据
    internal void OnCallibrationData(BroadcastInfo info)
    {
        float posX = BitConverter.ToSingle(info.Data, 0);
        float posY = BitConverter.ToSingle(info.Data, 4);
        float posZ = BitConverter.ToSingle(info.Data, 8);
        float rotX = BitConverter.ToSingle(info.Data, 12);
        float rotY = BitConverter.ToSingle(info.Data, 16);
        float rotZ = BitConverter.ToSingle(info.Data, 20);

        //设置屏幕相对于AnchorRoot的位置
        GameObject screen = new GameObject();
        screen.transform.SetParent(AnchorRoot);
        screen.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);
        screen.transform.localPosition = new Vector3(posX, posY, posZ);

        //设置screen的位置
        screen.transform.SetParent(null);
        AnchorRoot.SetParent(screen.transform);
        screen.transform.rotation = trackedRot;
        screen.transform.position = trackedPos;

        AnchorRoot.SetParent(null);

        Destroy(screen, 1);
        Debug.Log("校准完成!");
        ShowMessageManager.Instance.ShowMessage("校准完成!");
    }



    internal void DownLoadAnchor()
    {
        SendDataManager.Instance.SendDownloadAnchor();
        ClientHandlerCenter.Instance.OnOnDownloadAnchorData = OnOnDownloadAnchorData;
    }
    void OnOnDownloadAnchorData(byte[] data) 
    {
        HoloLensAnchorManager.Instance.ImportAnchorData(data, AnchorRoot.gameObject);
        ClientHandlerCenter.Instance.OnOnDownloadAnchorData = null;
    }

    internal void UploadAnchor()
    {
        //SendDataManager.Instance.SendUploadAnchor(new byte[1024]);
        HoloLensAnchorManager.Instance.ExprotAnchorData(AnchorRoot.gameObject, SendDataManager.Instance.SendUploadAnchor);
    }

 


    //发送校准请求给客户端
    internal void SendCallibrationRequest(int id)
    {

    }
    internal void OnConfirmCallibration(int id)
    {

    }
    private void OnDisable()
    {

    }
    //========================
    // public AxisFlags RotateAxisConstraint;
    // public ObjectManipulator.RotateInOneHandType RotateInOneHandType;
    public float ScaleMaximum = 10;
    public float ScaleMinimum = 0.2f;

    public bool UseLocalAsset;
    public bool CanChildMove;

    public void CrteateAssetByID(byte[] data)
    {
        int assetID = BitConverter.ToInt32(data, 0);
        long updateID = BitConverter.ToInt64(data, 4);

        float pox = BitConverter.ToSingle(data, 12);
        float poy = BitConverter.ToSingle(data, 16);
        float poz = BitConverter.ToSingle(data, 20);
        float rotx = BitConverter.ToSingle(data, 24);
        float roty = BitConverter.ToSingle(data, 28);
        float rotz = BitConverter.ToSingle(data, 32);

        GameObject asset = null;
        if (UseLocalAsset)
        {


        }
        else
        {
            asset = AssetPanelManager.Instance.GetAsset(assetID);
        }

        if (asset != null)
        {
            GameObject go = Instantiate(asset, AssetPanelManager.Instance.AnchorRoot);
            go.transform.localPosition = new Vector3(pox, poy, poz);
            go.transform.localEulerAngles = new Vector3(rotx, roty, rotz);
            go.AddComponent<UpdateTransform>().updateID = updateID;
            go.AddComponent<PlayAnimation>();
            // go.AddComponent<Dockable>();//考虑下这里是不是可以用其他代码实现
            //
            // //添加MRTK手势操作
            // ObjectManipulator om = go.AddComponent<ObjectManipulator>();
            // Debug.Log("添加了MRTK手势操作");
            // om.OneHandRotationModeFar = RotateInOneHandType;
            // om.OneHandRotationModeNear = RotateInOneHandType;
            // //旋转轴向约束
            // RotationAxisConstraint rc = go.AddComponent<RotationAxisConstraint>();
            // rc.ConstraintOnRotation = RotateAxisConstraint;
            // //缩放约束
            // MinMaxScaleConstraint scale = go.AddComponent<MinMaxScaleConstraint>();
            // scale.ScaleMaximum = ScaleMaximum;
            // scale.ScaleMinimum = ScaleMinimum;
            //
            // om.MoveLerpTime = UpdateTransformManager.Instance.UpdateTime;
            // om.RotateLerpTime = UpdateTransformManager.Instance.UpdateTime;
            // om.ScaleLerpTime = UpdateTransformManager.Instance.UpdateTime;


            if (CanChildMove)
            {
                Transform[] children = go.transform.GetComponentsInChildren<Transform>(true);//true的意思是是否包含了隐藏的物体，如果子物体隐藏状态，也是添加这个组件的
                for(int i = 0; i < children.Length; i++)
                {
                    if(children[i].GetComponent<Collider>() != null)
                    {
                        // children[i].gameObject.AddComponent<UpdateTransform>().updateID = updateID + i+1;
                        // children[i].gameObject.AddComponent<Dockable>();
                        //
                        // //添加MRTK手势操作
                        // ObjectManipulator om2 = children[i].gameObject.AddComponent<ObjectManipulator>();
                        // om2.OneHandRotationModeFar = RotateInOneHandType;
                        // om2.OneHandRotationModeNear = RotateInOneHandType;
                        // //旋转轴向约束
                        // RotationAxisConstraint rc2 = children[i].gameObject.AddComponent<RotationAxisConstraint>();
                        // rc2.ConstraintOnRotation = RotateAxisConstraint;
                        // //缩放约束
                        // MinMaxScaleConstraint scale2 = children[i].gameObject.AddComponent<MinMaxScaleConstraint>();
                        // scale2.ScaleMaximum = ScaleMaximum;
                        // scale2.ScaleMinimum = ScaleMinimum;
                        //
                        // om2.MoveLerpTime = UpdateTransformManager.Instance.UpdateTime;
                        // om2.RotateLerpTime = UpdateTransformManager.Instance.UpdateTime;
                        // om2.ScaleLerpTime = UpdateTransformManager.Instance.UpdateTime;

                    }
                }
            }
            print("CrteateAssetByID:" + assetID);
            
        }
        else
        {
            print("资产不存在:" + assetID);
        }
    }

    public void SendHoloviewData(byte[] data)
    {
        BroadcastInfo info = new BroadcastInfo();
        info.Type = (int)BroadcastType.HoloviewData;
        info.UserId = ClientNetworkManager.Instance.UserId;
        info.PeerId = serverClientId;
        info.Data = data;
        SendDataManager.Instance.SendBroadcastById(info);
    }
    internal void OnStartHoloview(BroadcastInfo info)
    {
        HoloCaptureManager.Instance.StartCapture();
    }
    internal void OnHoloviewData(BroadcastInfo info)
    {

    }
    internal void OnStopHoloview(BroadcastInfo info)
    {
        HoloCaptureManager.Instance.StopCapture();
    }
}