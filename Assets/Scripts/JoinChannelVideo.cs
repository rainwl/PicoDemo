using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using MixedReality.Toolkit.UX;
using UnityEngine.Android;
using UnityEngine.Serialization;

public class JoinChannelVideo : MonoBehaviour
{
    private const string AppID = "8b0180132a3b49fb813181cb23b92962";
    private const string ChannelName = "WangLin";
    private const string Token = "";
    private VideoSurface _remoteView;
    private IRtcEngine _rtcEngine;
    private readonly ArrayList _permissionList = new() { Permission.Camera, Permission.Microphone };

    public PressableButton joinButton;
    public PressableButton leaveButton;

    private void Start()
    {
        SetupVideoSDKEngine();
        InitEventHandler();
        SetupUI();
        PreviewSelf();
    }

    private void Update()
    {
        CheckPermissions();
    }

    private void OnApplicationQuit()
    {
        if (_rtcEngine == null) return;
        Leave();
        _rtcEngine.Dispose();
        _rtcEngine = null;
    }

    private void CheckPermissions()
    {
        foreach (string permission in _permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission);
            }
        }
    }

    private void PreviewSelf()
    {
        _rtcEngine.EnableVideo();
        _rtcEngine.StartPreview();
    }

    private void SetupUI()
    {
        joinButton.OnClicked.AddListener(Join);
        leaveButton.OnClicked.AddListener(Leave);
        var go = GameObject.Find("RemoteView");
        _remoteView = go.AddComponent<VideoSurface>();
        go.transform.Rotate(0.0f, 0.0f, -180.0f);
    }

    private void SetupVideoSDKEngine()
    {
        _rtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        var context = new RtcEngineContext
        {
            appId = AppID,
            channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT
        };
        _rtcEngine.Initialize(context);
    }

    private void InitEventHandler()
    {
        var handler = new UserEventHandler(this);
        _rtcEngine.InitEventHandler(handler);
    }

    private void Join()
    {
        var options = new ChannelMediaOptions();
        options.publishMicrophoneTrack.SetValue(true);
        options.publishCameraTrack.SetValue(true);
        options.autoSubscribeAudio.SetValue(true);
        options.autoSubscribeVideo.SetValue(true);
        options.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        _rtcEngine.JoinChannel(Token, ChannelName, 0, options);
    }

    private void Leave()
    {
        Debug.Log("Leaving _channelName");
        _rtcEngine.StopPreview();
        _rtcEngine.LeaveChannel();
        _remoteView.SetEnable(false);
    }

    private class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly JoinChannelVideo _videoSample;

        internal UserEventHandler(JoinChannelVideo videoSample)
        {
            _videoSample = videoSample;
        }

        // 发生错误回调
        public override void OnError(int err, string msg)
        {
        }

        // 本地用户成功加入频道时，会触发该回调
        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
        }

        // SDK 接收到第一帧远端视频并成功解码时，会触发 OnUserJoined 回调
        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            // 设置远端视频显示
            _videoSample._remoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            // 开始视频渲染
            _videoSample._remoteView.SetEnable(true);
            Debug.Log("Remote user joined");
        }

        // 远端用户离开当前频道时会触发该回调
        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample._remoteView.SetEnable(false);
            Debug.Log("Remote user offline");
        }
    }
}