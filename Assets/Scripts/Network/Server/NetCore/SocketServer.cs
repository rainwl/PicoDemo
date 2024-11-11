using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class SocketServer 
{
    Socket server;
    //最大连接数
    int maxClient;
    //客户端对象池
    Queue<UserToken> userTokenPool;
    //当前连接的客户端列表
    List<UserToken> userTokenList= new List<UserToken>();

    //当前连接的客户端数量
    int count;
    //连接信号量
    Semaphore acceptClentSemaphore;
    //客户端userId索引值
    int userIdIndex;
    //消息处理中心
    AbsHandlerCenter handerCenter;
    public SocketServer(AbsHandlerCenter center) 
    {
        handerCenter = center;
        server = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
    }
    public List<UserToken> GetUserTokenList() 
    {
        return userTokenList;
    }
    /// <summary>
    /// 启动服务器
    /// </summary>
    /// <param name="max"></param>
    /// <param name="port"></param>
    public void Start(int max,int port) 
    {
        maxClient = max;
        userTokenPool = new Queue<UserToken>(maxClient);
        acceptClentSemaphore = new Semaphore(maxClient, maxClient);
        for (int i = 0; i < maxClient; i++)
        {
            UserToken tokent = new UserToken(this,handerCenter);
            userTokenPool.Enqueue(tokent);
        }
        server.Bind(new IPEndPoint(IPAddress.Any,port));
        server.Listen(2);
        StartAccept(null);
    }
    public void Stop() 
    {
        try
        {
            for (int i = 0; i < userTokenList.Count; i++)
            {
                userTokenList[i].Close();
            }
            userTokenList.Clear();
            userTokenPool.Clear();
            server.Close();
            server = null;
        }
        catch (Exception e)
        {
            Console.WriteLine("stop server error:"+e.Message);
        }

    }

    /// <summary>
    /// 开始异步接收客户端连接
    /// </summary>
    /// <param name="e"></param>
    void StartAccept(SocketAsyncEventArgs e) 
    {
        if (e == null)
        {
            e = new SocketAsyncEventArgs();
            e.Completed += AcceptCompleted;
        }
        else 
        {
            e.AcceptSocket = null;
        }

        if (!server.AcceptAsync(e)) 
        {
            ProcessAccept(e);
        }
    }

    /// <summary>
    /// 异步连接完成的回调
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
    {
        ProcessAccept(e);
    }

    /// <summary>
    /// 处理客户端连接
    /// </summary>
    /// <param name="e"></param>
    void ProcessAccept(SocketAsyncEventArgs e) 
    {
        if (server == null) return;

        if (count >= maxClient) 
        {
            Console.WriteLine("accept client is full,waitting...");
        }
        //信号量-1
        acceptClentSemaphore.WaitOne();
        //客户端连接数+1
        Interlocked.Add(ref count,1);

        UserToken token = userTokenPool.Dequeue();
        token.IsUsing = true;
        token.Client = e.AcceptSocket;
        token.ConnectTime = DateTime.Now;
        token.HeartTime = DateTime.Now;
        token.UserId = userIdIndex++;
        token.UserName = "Temp:"+token.Client.RemoteEndPoint;

        userTokenList.Add(token);
        //通知消息处理中心有客户端连接
        handerCenter.ClientConnect(token);
        //开始接收客户端的消息
        token.StartReceive();
        //继续异步接收客户端连接
        StartAccept(e);
    }

    /// <summary>
    /// 客户端断开连接
    /// </summary>
    /// <param name="token"></param>
    public void CloseClient(UserToken token,string error) 
    {
        if (!token.IsUsing || token.Client == null) return;

        //通知消息处理中心有客户端断开
        handerCenter.ClientClose(token,error);

        token.Close();
        userTokenList.Remove(token);
        //信号量+1
        acceptClentSemaphore.Release();
        userTokenPool.Enqueue(token);
        //客户端连接数-1
        Interlocked.Add(ref count,-1);
    }
}
