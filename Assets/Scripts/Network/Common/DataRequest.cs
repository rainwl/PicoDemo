public class DataRequest 
{
    //上传用户信息
    public const byte MR_UPDATE_USERINFO = 1;
    //获取在线用户列表
    public const byte MR_GET_CLIENTLIST = 2;
    //上传锚点
    public const byte MR_UPLOAD_ANCHOR = 3;
    //下载锚点
    public const byte MR_DOWNLOAD_ANCHOR = 4;

    //==================================

    //转发数据给所有客户端
    public const byte BROADCAST_ALL = 1;

    //转发数据给除自己外的所有客户端
    public const byte BROADCAST_OTHER = 2;

    //转发数据给某个ID的客户端
    public const byte BROADCAST_BY_ID = 3;

}
