#if UNITY_EDITOR ||UNITY_WSA
using HoloCapture;
#endif

using System.Collections.Generic;
using MR;
using UnityEngine;
public class HoloCaptureManager : MonoBehaviour
{

    public static HoloCaptureManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    //https://docs.microsoft.com/zh-cn/windows/mixed-reality/locatable-camera

    public enum HoloCamFrame
    {
        Holo15,
        Holo30,
    }
    public enum HoloResolution
    {
        Holo_896x504,
        Holo_1280x720,
    }
    //public HoloType holoType;
    public HoloResolution holoResolution;
    public HoloCamFrame holoFrame;
    HoloCapture.Resolution resolution;
    public bool EnableHolograms = true;//看到的画面是混合融合画面

    [Range(0, 1)]
    public float Opacity = 0.9f;//透明度，摄像头和unity的叠加，指的Unitycamera透明度


    public Texture2D VideoTexture { get; set; }//获取渲染的画面
    bool onVideoData = false;

    void Start()
    {
        switch (holoResolution)
        {
            case HoloResolution.Holo_896x504:
                resolution = new HoloCapture.Resolution(896, 504);
                break;
            case HoloResolution.Holo_1280x720:
                resolution = new HoloCapture.Resolution(1280, 720);
                break;
        }
        int frame;
        switch (holoFrame)
        {
            case HoloCamFrame.Holo15:
                frame = 15;
                break;
            case HoloCamFrame.Holo30:
                frame = 30;
                break;
            default:
                frame = 15;
                break;
        }
        VideoTexture = new Texture2D(resolution.width, resolution.height, TextureFormat.BGRA32, false);

// #if UNITY_EDITOR || UNITY_WSA
//         HoloCaptureHelper.Instance.Init(resolution, frame, true, EnableHolograms, Opacity, false,
// UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr(), OnFrameSampleCallback);//最后有个回调
// #endif
    }

#if UNITY_EDITOR || UNITY_WSA

    private void OnDestroy()
    {
        HoloCaptureHelper.Instance.Destroy();
    }

    bool isStartCaputure;
    public void StartCapture()
    {
        if (isStartCaputure) return;
        isStartCaputure = true;
        HoloCaptureHelper.Instance.StartCapture();
    }
    public void StopCapture()
    {
        if (!isStartCaputure) return;
        isStartCaputure = false;
        HoloCaptureHelper.Instance.StopCapture();
    }
    void OnFrameSampleCallback(VideoCaptureSample sample)
    {
        //首先获取视频画面数据，然后开启了一个WSA线程，然后处理HoloLens1和2的画面，垂直画面是镜像的，需要处理
        byte[] imageBytes = new byte[sample.dataLength];

        sample.CopyRawImageDataIntoBuffer(imageBytes);

        sample.Dispose();

        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            if (Application.platform == RuntimePlatform.WSAPlayerX86)
            {
                ImageHorizontalMirror(imageBytes);
            }
            else if (Application.platform == RuntimePlatform.WSAPlayerARM)
            {
                ImageVerticalMirror(imageBytes);
            }
            VideoTexture.LoadRawTextureData(imageBytes);
            VideoTexture.Apply();
            onVideoData = true;
        }, false);
    }
    void ImageHorizontalMirror(byte[] imageBytes)
    {
        int PixelSize = 4;
        int width = resolution.width;
        int height = resolution.height;
        int Line = width * PixelSize;

        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j + 4 < Line / 2; j += 4)
            {
                Swap<byte>(ref imageBytes[Line * i + j], ref imageBytes[Line * i + Line - j - 4]);
                Swap<byte>(ref imageBytes[Line * i + j + 1], ref imageBytes[Line * i + Line - j - 3]);
                Swap<byte>(ref imageBytes[Line * i + j + 2], ref imageBytes[Line * i + Line - j - 2]);
                Swap<byte>(ref imageBytes[Line * i + j + 3], ref imageBytes[Line * i + Line - j - 1]);
            }
        }
    }
    void ImageVerticalMirror(byte[] imageBytes)
    {
        int PixelSize = 4;
        int width = resolution.width;
        int height = resolution.height;
        int Line = width * PixelSize;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height / 2; j++)
            {
                Swap<byte>(ref imageBytes[Line * j + i * PixelSize], ref imageBytes[Line * (height - j - 1) + i * PixelSize]);
                Swap<byte>(ref imageBytes[Line * j + i * PixelSize + 1], ref imageBytes[Line * (height - j - 1) + i * PixelSize + 1]);
                Swap<byte>(ref imageBytes[Line * j + i * PixelSize + 2], ref imageBytes[Line * (height - j - 1) + i * PixelSize + 2]);
                Swap<byte>(ref imageBytes[Line * j + i * PixelSize + 3], ref imageBytes[Line * (height - j - 1) + i * PixelSize + 3]);
            }
        }
    }
    void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }
#else
    public void StartCapture()
    {
    }
    public void StopCapture()
    {
    }
#endif

    private void Update()
    {
        if (onVideoData) { SendVideoData(); }
    }

    void SendVideoData()
    {
        onVideoData = false;
        byte[] imageBytes = ImageConversion.EncodeToJPG(VideoTexture);//帧率降低到30以下
        ARManager.Instance.SendHoloViewData(imageBytes);
    }

    public void OnVideoDataReceive(byte[] data)
    {
        ImageConversion.LoadImage(VideoTexture, data);
        VideoTexture.Apply(false);
    }
}
