using Mirror;
using Mirror.Discovery;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ServerFoundUnityEvent<TResponseType> : UnityEvent<TResponseType> { };

[DisallowMultipleComponent]
[AddComponentMenu("Network/Lobby Discovery")]
public class LobbyDiscovery : NetworkDiscoveryBase<ServerRequest, LobbyServerResponse>
{

    private Transport currentTransport;
    public int ListenPort => serverBroadcastListenPort;

    public Transport CurrentTransport { get => currentTransport; set => currentTransport = value; }
    private readonly Dictionary<long, LobbyServerResponse> foundServers = new();

    public override void Start()
    {
        base.Start();
        //BroadcastAddress = GetBroadcastAddress().ToString();
        //BroadcastAddress = "255.255.255.255";
        BroadcastAddress = "127.0.0.1";

        StartCoroutine(Init());
    }
    IEnumerator Init()
    {
        yield return null;

        transport = Transport.active;
    }

    #region Server

    protected override LobbyServerResponse ProcessRequest(ServerRequest request, IPEndPoint endpoint)
    {
        try
        {
            var dispatcher = LobbyInfoDispatcher.Instance;

            return new LobbyServerResponse
            {
                serverId = ServerId,
                uri = transport.ServerUri(),
                name = dispatcher.Name.Value,
                iconIndex = dispatcher.IconIndex.Value,
                playerCount = (byte)NetworkServer.connections.Count(kv => kv.Value.identity != null)
            };
        }
        catch (NotImplementedException)
        {
            Debug.LogError($"Transport {transport} does not support network discovery");
            throw;
        }
    }

    #endregion

    #region Client


    protected override ServerRequest GetRequest() => new ServerRequest();

    protected override void ProcessResponse(LobbyServerResponse response, IPEndPoint endpoint)
    {
        response.EndPoint = endpoint;

        response.EndPoint = endpoint;

        LobbyInfoDispatcher.Instance.SetFromServerResponse(response);
        foundServers[response.serverId] = response;

        OnServerFound?.Invoke(response);
    }

    public LobbyServerResponse? GetFirstServer()
    {
        return foundServers.Values.FirstOrDefault();
    }

    #endregion

    public bool IsAdvertising { get; private set; } = false;

    public void StartAdvertising()
    {
        AdvertiseServer();
        IsAdvertising = true;
    }

    public void StopAdvertising()
    {
        StopDiscovery();
        IsAdvertising = false;
    }
}