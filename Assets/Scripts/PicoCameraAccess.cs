using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR;

public class PicoCameraAccess : MonoBehaviour
{
    public Renderer displayRenderer;
    void Start()
    {
        StartCamera();
        RenderTexture cameraTexture = new RenderTexture(1024, 1024, 24);
        displayRenderer.material.mainTexture = cameraTexture;

        PXR_Plugin.Pxr_CameraCreateTexturesMainThread();
        // 将摄像头数据绑定到RenderTextur
    }

    void StartCamera()
    {
        PXR_Plugin.Pxr_CameraStart();
    }
    
    void OnDisable()
    {
        
        PXR_Plugin.Pxr_CameraStop();
    }
}