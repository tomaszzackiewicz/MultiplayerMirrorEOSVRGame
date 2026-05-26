using Mirror;
using System.Linq;
using UnityEngine;

public class LobbyInfoDispatcher : SingletonMono<LobbyInfoDispatcher>
{
    [SerializeField] private ServerIconLibrary serverIconLibrary; // referencja do ScriptableObject

    public SDispatcher<string> Name = new();
    public SDispatcher<int> IconIndex = new();
    public SDispatcher<string> Address = new();
    public SDispatcher<int> PlayerCount = new();
    public SDispatcher<string> LobbyRuntime = new();
    public SDispatcher<Sprite> Icon = new();

    protected override void Awake()
    {
        base.Awake();
        InitializeDefaults();

        // Automatyczne odwzorowanie indexu na ikonê
        IconIndex.PipeTo(Icon, serverIconLibrary.GetIcon);
    }

    private void InitializeDefaults()
    {
        Name.SetValue("LAN Lobby");
        IconIndex.SetValue(0);
        Address.SetValue("127.0.0.1");
        PlayerCount.SetValue(1);
        LobbyRuntime.SetValue("");
        Icon.SetValue(serverIconLibrary != null ? serverIconLibrary.GetIcon(0) : null);
    }

    public void SetFromServerResponse(LobbyServerResponse response)
    {
        Name.Value = response.name;
        IconIndex.Value = response.iconIndex;
        Address.Value = response.uri.Host;
        PlayerCount.Value = response.playerCount;

        // Alternatywa, jeœli nie chcesz u¿ywaæ PipeTo():
        // Icon.Value = iconLibrary.GetIcon(response.iconIndex);
    }

    public void UpdateFromLocalState()
    {
        Name.Value = Name.Value; // wymuszenie aktualizacji
        PlayerCount.Value = (byte)NetworkServer.connections.Count(c => c.Value.identity != null);
    }

    public LobbyServerResponse ToServerResponse(Transport transport, long serverId)
    {
        return new LobbyServerResponse
        {
            serverId = serverId,
            uri = transport.ServerUri(),
            name = Name.Value,
            iconIndex = IconIndex.Value,
            playerCount = (byte)PlayerCount.Value
        };
    }
}