using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Network.Client
{
    public class SocketClient
    {
        Socket client;
        AutoResetEvent connectAutoResetEvent;

        //接收数据缓存区
        List<byte> receiveBuffer = new List<byte>();

        //客户端异步接收
        SocketAsyncEventArgs receiveSAEA;

        //客户端异步发送
        Queue<SocketAsyncEventArgs> sendSAEAQueue = new Queue<SocketAsyncEventArgs>();

        //数据发送缓存队列
        Queue<byte[]> sendBufferQueue = new Queue<byte[]>();
        private bool _isReading;

        //连接服务器结果的委托
        public delegate void ConnectDg(bool result);

        public ConnectDg OnConnect;

        //断开与服务器连接的委托
        public delegate void DisconnectDg();

        public DisconnectDg OnDisconnect;

        //接收服务器数据的委托
        public delegate void ReceiveDataDg(byte[] data);

        public ReceiveDataDg OnReceiveData;

        public void ConnectServer(string ip, int port)
        {
            if (Connected) return;

            var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            client = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            connectAutoResetEvent = new AutoResetEvent(false);

            var e = new SocketAsyncEventArgs();
            e.UserToken = client;
            e.RemoteEndPoint = endPoint;
            e.Completed += Connect_Completed;

            client.ConnectAsync(e);
            connectAutoResetEvent.WaitOne(2000);
            OnConnect?.Invoke(Connected);

            if (Connected)
            {
                receiveSAEA = new SocketAsyncEventArgs();
                receiveSAEA.RemoteEndPoint = endPoint;
                receiveSAEA.Completed += ReceiveAsync_Completed;
                var buffer = new byte[10240];
                receiveSAEA.SetBuffer(buffer, 0, buffer.Length);

                StartReceive();
            }
        }

        private void StartReceive()
        {
            if (!client.ReceiveAsync(receiveSAEA))
            {
                ProcessReceive(receiveSAEA);
            }
        }

        private void ReceiveAsync_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        public bool Connected => client is { Connected: true };


        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            connectAutoResetEvent.Set();
        }


        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                var data = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, 0, data, 0, e.BytesTransferred);
                receiveBuffer.AddRange(data);
                if (!_isReading)
                {
                    _isReading = true;
                    ReadData();
                }

                //继续接收数据
                StartReceive();
            }
            else
            {
                //断开连接
                Close();
            }
        }

        /// <summary>
        /// 读取缓存区数据
        /// </summary>
        void ReadData()
        {
            //分包、粘包（数据包=4个字节int+实际数据包的长度）
            if (receiveBuffer.Count <= 4)
            {
                _isReading = false;
                return;
            }

            byte[] lengthBytes = receiveBuffer.GetRange(0, 4).ToArray();
            int length = BitConverter.ToInt32(lengthBytes, 0);
            if (receiveBuffer.Count - 4 < length)
            {
                _isReading = false;
                return;
            }

            byte[] data = receiveBuffer.GetRange(4, length).ToArray();

            lock (receiveBuffer)
            {
                receiveBuffer.RemoveRange(0, 4 + length);
            }

            //将数据交到应用层去处理
            OnReceiveData?.Invoke(data);
            //递归继续读取数据
            ReadData();
        }

        /// <summary>
        /// 向客户端发送数据
        /// </summary>
        /// <param name="data"></param>
        public void Send(byte[] data)
        {
            if (client == null) return;
            if (data == null) return;
            sendBufferQueue.Enqueue(data);
            if (!isSending)
            {
                isSending = true;
                HandlerSend();
            }
        }

        bool isSending;

        /// <summary>
        /// 处理数据发送
        /// </summary>
        void HandlerSend()
        {
            try
            {
                lock (sendBufferQueue)
                {
                    if (sendBufferQueue.Count == 0)
                    {
                        isSending = false;
                        return;
                    }

                    SocketAsyncEventArgs send = GetSendSAEA();
                    if (send == null) return;
                    byte[] data = sendBufferQueue.Dequeue();
                    send.SetBuffer(data, 0, data.Length);
                    if (!client.SendAsync(send))
                    {
                        ProcessSend(send);
                    }

                    HandlerSend();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("send error:" + e.Message);
            }
        }

        void ProcessSend(SocketAsyncEventArgs e)
        {
            //发送成功
            if (e.SocketError == SocketError.Success)
            {
                //回收
                sendSAEAQueue.Enqueue(e);
                HandlerSend();
            }
            else
            {
                //断开连接
                Close();
            }
        }

        //已创建的发送异步对象数量
        int sendCount;

        /// <summary>
        /// 获取发送异步对象
        /// </summary>
        /// <returns></returns>
        SocketAsyncEventArgs GetSendSAEA()
        {
            if (sendSAEAQueue.Count == 0)
            {
                if (sendCount >= 100) return null;

                SocketAsyncEventArgs send = new SocketAsyncEventArgs();
                send.Completed += Send_Completed;
                send.UserToken = this;
                sendCount++;
                return send;
            }
            else
            {
                return sendSAEAQueue.Dequeue();
            }
        }

        /// <summary>
        /// 异步发送完成的回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e);
        }

        /// <summary>
        /// 关闭与服务器的连接
        /// </summary>
        public void Close()
        {
            if (!Connected) return;

            sendBufferQueue.Clear();
            receiveBuffer.Clear();
            _isReading = false;
            isSending = false;

            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                client = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("client close error:" + e.Message);
            }

            receiveSAEA.Completed -= ReceiveAsync_Completed;
            foreach (var item in sendSAEAQueue)
            {
                item.Completed -= Send_Completed;
            }

            OnDisconnect?.Invoke();
        }
    }
}