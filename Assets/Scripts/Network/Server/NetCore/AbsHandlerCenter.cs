using System.Collections;
using System.Collections.Generic;

public abstract class AbsHandlerCenter 
{
    /// <summary>
    /// 客户端连接
    /// </summary>
    /// <param name="token"></param>
    public abstract void ClientConnect(UserToken token);
    /// <summary>
    /// 客户端断开
    /// </summary>
    /// <param name="token"></param>
    /// <param name="error"></param>
    public abstract void ClientClose(UserToken token,string error);
    /// <summary>
    /// 收到客户端消息
    /// </summary>
    /// <param name="token"></param>
    /// <param name="data"></param>
    public abstract void MessageReceive(UserToken token,byte[] data);
}
