using Mirror;

public struct ServerRequest : NetworkMessage
{
    public int placeholder;

    public static byte[] Serialize(ServerRequest request)
    {
        var writer = new NetworkWriter();
        writer.WriteInt(request.placeholder);
        return writer.ToArray();
    }

    public static ServerRequest Deserialize(byte[] data)
    {
        var reader = new NetworkReader(data);
        return new ServerRequest
        {
            placeholder = reader.ReadInt()
        };
    }
}