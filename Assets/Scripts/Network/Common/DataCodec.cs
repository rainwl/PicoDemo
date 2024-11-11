using System;

public static class DataCodec
{
    /// <summary>
    /// 数据模型序列化
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static byte[] Encode(DataModel model) 
    {
        int messageLength = model.Message == null ? 0 : model.Message.Length;
        byte[] lengthBytes = BitConverter.GetBytes(2 + messageLength);
        //消息=4字节长度+Type+Request+Message
        byte[] newData = new byte[4 + 2 + messageLength];
        byte[] trData = new byte[] { model.Type, model.Request };
        //数据长度
        Array.Copy(lengthBytes,0, newData,0, lengthBytes.Length);
        //消息类型+请求类型
        Array.Copy(trData,0, newData,4, trData.Length);
        //消息数据
        if (model.Message != null) 
        {
            Array.Copy(model.Message,0, newData,6, model.Message.Length);
        }
        return newData;
    }

    /// <summary>
    /// 数据模型序反列化
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static DataModel Decode(byte[] data) 
    {
        DataModel model = new DataModel();
        //消息类型
        model.Type = data[0];
        //请求类型
        model.Request = data[1];
        //消息数据
        if (data.Length > 2) 
        {
            byte[] message = new byte[data.Length-2];
            Array.Copy(data,2,message,0,data.Length-2);
            model.Message = message;
        }
        return model;
    }
}
