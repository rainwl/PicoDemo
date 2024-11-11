
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class MessageCodec 
{
    /// <summary>
    /// 消息类的序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public static byte[] ObjectToBytes<T>(T t) 
    {
        if (t == null) return null;
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        formatter.Serialize(stream,t);
        return stream.ToArray();
    }

    /// <summary>
    /// 消息类的反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static T BytesToObject<T>(byte[] data) 
    {
        if (data == null) return default(T);
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream(data);
        return (T)formatter.Deserialize(stream);
    }
}
