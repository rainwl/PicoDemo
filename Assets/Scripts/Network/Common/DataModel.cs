public class DataModel 
{
    public byte Type { get; set; } 
    public byte Request { get; set; }
    public byte[] Message { get; set; }

    public DataModel(byte type,byte request,byte[] message=null) 
    {
        Type = type;
        Request = request;
        Message = message;
    }
    public DataModel() { }

}
