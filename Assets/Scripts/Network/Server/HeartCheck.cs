using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class HeartCheck
{
    SocketServer socketServer;
    int heartOutTime;
    Thread heartThread;
    public HeartCheck(SocketServer server,int time) 
    {
        socketServer = server;
        heartOutTime = time;
        heartThread = new Thread(StartHeartThread);
        heartThread.Start();
    }
    void StartHeartThread() 
    {
        while (heartThread.IsAlive)
        {
            List<UserToken>list= socketServer.GetUserTokenList();
            for (int i = 0; i < list.Count; i++)
            {
                //客户端心跳超时
                if ((DateTime.Now - list[i].HeartTime).TotalSeconds > heartOutTime) 
                {
                    //关闭客户端
                    socketServer.CloseClient(list[i],"heart out time");
                }
            }
            Thread.Sleep(1000);
        }
    }
    /// <summary>
    /// 关闭心跳检测线程
    /// </summary>
    public void Close() 
    {
        heartThread.Abort();
    }
}
