using Mirror;

public struct FPSServerResponse : NetworkMessage
{
    public long serverId;
    public string uri;
    public string serverName;
    public int currentPlayerCount;
    public int maxPlayerCount;

    public static byte[] Serialize(FPSServerResponse response)
    {
        var writer = new NetworkWriter();
        writer.WriteLong(response.serverId);
        writer.WriteString(response.uri);
        writer.WriteString(response.serverName);
        writer.WriteInt(response.currentPlayerCount);
        writer.WriteInt(response.maxPlayerCount);
        return writer.ToArray();
    }

    public static FPSServerResponse Deserialize(byte[] data)
    {
        var reader = new NetworkReader(data);
        return new FPSServerResponse
        {
            serverId = reader.ReadLong(),
            uri = reader.ReadString(),
            serverName = reader.ReadString(),
            currentPlayerCount = reader.ReadInt(),
            maxPlayerCount = reader.ReadInt()
        };
    }
}