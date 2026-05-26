using Mirror;
using System;
using System.Net;

public struct LobbyServerResponse : NetworkMessage, IEquatable<LobbyServerResponse>
{
    [NonSerialized]
    public IPEndPoint EndPoint;

    public Uri uri;
    public long serverId;
    public string name;
    public int iconIndex;
    public byte playerCount;

    public bool Equals(LobbyServerResponse other)
    {
        return serverId == other.serverId && uri == other.uri;
    }
}