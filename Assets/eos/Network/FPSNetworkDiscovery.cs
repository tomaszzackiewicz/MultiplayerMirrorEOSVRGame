using Mirror;
using Mirror.Discovery;
using Player;
using System;
using System.Net;
using UnityEngine;
using UnityEngine.Events;

public class FPSNetworkDiscovery : NetworkDiscoveryBase<ServerRequest, FPSServerResponse>
{
    public long serverId = System.DateTime.Now.Ticks;
    [Serializable] private class ServerFoundEvent : UnityEvent<FPSServerResponse> { }

    [SerializeField] private ServerFoundEvent onServerFoundEvent;


    //Stored required properties.
    private FpsNetworkManager networkManager;
    private Transport currentTransport;
    public int ListenPort => serverBroadcastListenPort;

    public Transport CurrentTransport { get => currentTransport; set => currentTransport = value; }

    public override void Start()
    {
        base.Start();
        //BroadcastAddress = GetBroadcastAddress().ToString();
        //BroadcastAddress = "255.255.255.255";
        BroadcastAddress = "127.0.0.1";
        Debug.Log($"[DISCOVERY-HOST] Start() — endpoint, port: {serverBroadcastListenPort}, transport: {Transport.active?.GetType().Name}");
    }

    protected override ServerRequest GetRequest()
    {
        return new ServerRequest { placeholder = 42 };
    }

    protected override FPSServerResponse ProcessRequest(ServerRequest request, IPEndPoint sender)
    {
        Debug.Log($"[DISCOVERY HOST] Otrzymano zapytanie od {sender}, placeholder = {request.placeholder}");

        return new FPSServerResponse
        {
            serverId = serverId,
            serverName = "Test Lobby",
            currentPlayerCount = 1,
            maxPlayerCount = 4,
            uri = Transport.active.ServerUri().ToString()
        };
    }

    protected override void ProcessResponse(FPSServerResponse response, IPEndPoint sender)
    {
        Debug.Log($"[CLIENT] Odpowiedź z hosta: {response.serverName}, uri: {response.uri}");

        try
        {
            var parsedUri = new Uri(response.uri);
            var builder = new UriBuilder(parsedUri)
            {
                Host = sender.Address.ToString()
            };
            response.uri = builder.Uri.ToString();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DISCOVERY CLIENT] Błąd podczas parsowania URI: {e.Message}");
        }

        OnServerFound?.Invoke(response);
    }

    private IPAddress GetBroadcastAddress()
    {
        var localIP = GetLocalIPv4();
        if (localIP == null) return IPAddress.Broadcast;

        byte[] ipBytes = localIP.GetAddressBytes();
        byte[] maskBytes = new byte[] { 255, 255, 255, 0 }; // domyślna maska klasy C

        byte[] broadcastBytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            broadcastBytes[i] = (byte)(ipBytes[i] | (maskBytes[i] ^ 255));
        }

        return new IPAddress(broadcastBytes);
    }

    private IPAddress GetLocalIPv4()
    {
        foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                continue;

            var ipProps = ni.GetIPProperties();
            foreach (var addr in ipProps.UnicastAddresses)
            {
                if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(addr.Address))
                {
                    return addr.Address;
                }
            }
        }
        return null;
    }

}