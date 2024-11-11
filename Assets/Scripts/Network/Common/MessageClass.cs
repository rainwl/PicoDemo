
using System;

[Serializable]
public class ClientInfo 
{
    public int UserId;
    public string UserName;
    public int UserType;
    public string UserIP;
}

[Serializable]
public class BroadcastInfo
{
    public int Type;
    public int UserId;
    public int PeerId;
    public byte[] Data; 
}
public enum BroadcastType 
{
    UpdateTransform,
    CrteateAssetByID,
    RemoveAssetByID,
    RemoveAllAsset,
    PlayAniNextByID,
    PlyAniLastByID,
    RequestCallibration,
    ConfirmCallibration,
    CallibrationData,
    StartHoloview,
    StopHoloview,
    HoloviewData,
    DockByID,
    UndockByID,
}

public enum Result
{
    None,
    Success,
    Fail,
}