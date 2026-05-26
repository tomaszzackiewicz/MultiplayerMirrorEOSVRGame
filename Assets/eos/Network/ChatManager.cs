using Mirror;
using Player;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    private readonly List<ChatMessage> chatHistory = new();
    private readonly Dictionary<string, NetworkConnectionToClient> nickToConn = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        NetworkServer.RegisterHandler<ChatMessage>(HandleChatMessage);
        NetworkServer.RegisterHandler<ChatHistoryRequestMessage>(HandleHistoryRequest);
    }

    public void RegisterPlayer(string nickname, NetworkConnectionToClient conn)
    {
        if (!string.IsNullOrWhiteSpace(nickname))
        {
            nickToConn[nickname] = conn;
        }
    }

    private void HandleChatMessage(NetworkConnectionToClient senderConn, ChatMessage msg)
    {
        var player = senderConn.identity?.GetComponent<PlayerScript>();
        string senderNick = player != null ? player.DisplayName : $"Player {senderConn.connectionId}";

        msg.sender = senderNick;

        if (msg.isPrivate && nickToConn.TryGetValue(msg.recipient, out var targetConn))
        {
            senderConn.Send(msg);     // dla nadawcy
            targetConn.Send(msg);     // dla odbiorcy
        }
        else
        {
            chatHistory.Add(msg);
            NetworkServer.SendToAll(msg);
        }
    }

    private void HandleHistoryRequest(NetworkConnectionToClient conn, ChatHistoryRequestMessage _)
    {
        foreach (var msg in chatHistory)
        {
            conn.Send(msg);
        }
    }
}