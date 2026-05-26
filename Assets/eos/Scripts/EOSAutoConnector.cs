using UnityEngine;
using Mirror;
using EpicTransport;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class EOSAutoConnector : MonoBehaviour
{
    public static EOSAutoConnector Instance { get; private set; }

    [Header("Settings")]
    public string lobbyBucketName = "MyVRGame_v1";
    public bool alwaysAutoStart = false;

    [Header("UI Panels")]
    public GameObject PanelStart;
    public GameObject PanelStop;

    [Header("UI Elements")]
    public TMP_Text infoText;
    public Button buttonHost;
    public Button buttonClient;
    public Button buttonStop;
    public Button buttonAuto;

    private string currentLobbyId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        buttonHost.onClick.AddListener(ButtonHost);
        buttonClient.onClick.AddListener(ButtonClient);
        buttonStop.onClick.AddListener(ButtonStop);
        // buttonAuto.onClick.AddListener(ButtonAuto);

        SetupCanvas();

        if (alwaysAutoStart)
        {
            ButtonAuto();
        }
    }

    public void ButtonHost()
    {
        if (!EOSSDKComponent.Initialized) { SetupInfoText("EOS Not Ready!"); return; }

        SetupInfoText("Starting EOS Host...");

        NetworkManager.singleton.networkAddress = EOSSDKComponent.LocalUserProductIdString;
        NetworkManager.singleton.StartHost();

        CreateEOSLobby();
    }

    public void ButtonClient()
    {
        if (!EOSSDKComponent.Initialized) { SetupInfoText("EOS Not Ready!"); return; }

        SetupInfoText("Searching for EOS Lobbies...");
        StartCoroutine(SearchAndJoin());
    }

    public void ButtonAuto()
    {
        StartCoroutine(AutoStartRoutine());
    }

    public void ButtonStop()
    {
        SetupInfoText("Stopping Network and Clearing Lobby...");

        if (!string.IsNullOrEmpty(currentLobbyId))
        {
            var leaveOptions = new LeaveLobbyOptions { LobbyId = currentLobbyId, LocalUserId = EOSSDKComponent.LocalUserProductId };
            EOSSDKComponent.GetLobbyInterface().LeaveLobby(ref leaveOptions, null, (ref LeaveLobbyCallbackInfo data) =>
            {
                currentLobbyId = null;
            });
        }

        if (NetworkServer.active && NetworkClient.isConnected)
            NetworkManager.singleton.StopHost();
        else if (NetworkClient.isConnected)
            NetworkManager.singleton.StopClient();
        else if (NetworkServer.active)
            NetworkManager.singleton.StopServer();

        Invoke(nameof(SetupCanvas), 0.5f);
    }

    private IEnumerator AutoStartRoutine()
    {
        SetupInfoText("Auto: Searching for servers...");

        yield return StartCoroutine(SearchAndJoin(true));
    }

    private IEnumerator SearchAndJoin(bool autoHostIfNotFound = false)
    {
        var lobbyInterface = EOSSDKComponent.GetLobbyInterface();
        var searchOptions = new CreateLobbySearchOptions { MaxResults = 10 };
        lobbyInterface.CreateLobbySearch(ref searchOptions, out LobbySearch search);

        var filter = new LobbySearchSetParameterOptions
        {
            Parameter = new Epic.OnlineServices.Lobby.AttributeData
            {
                Key = "BucketId",
                Value = new Epic.OnlineServices.Lobby.AttributeDataValue { AsUtf8 = lobbyBucketName }
            },
            ComparisonOp = ComparisonOp.Equal
        };
        search.SetParameter(ref filter);

        bool searchFinished = false;
        uint foundCount = 0;

        var findOptions = new LobbySearchFindOptions { LocalUserId = EOSSDKComponent.LocalUserProductId };

        search.Find(ref findOptions, null, (ref LobbySearchFindCallbackInfo data) =>
        {
            if (data.ResultCode == Result.Success)
            {
                var countOptions = new LobbySearchGetSearchResultCountOptions();
                foundCount = search.GetSearchResultCount(ref countOptions);
            }
            searchFinished = true;
        });

        yield return new WaitUntil(() => searchFinished);

        if (foundCount > 0)
        {
            SetupInfoText($"Found {foundCount} lobby. Connecting...");
            ConnectToFirstFound(search);
        }
        else
        {
            if (autoHostIfNotFound)
            {
                SetupInfoText("No lobbies found. Hosting...");
                ButtonHost();
            }
            else
            {
                SetupInfoText("No lobbies found.");
            }
        }
    }

    private void ConnectToFirstFound(LobbySearch search)
    {
        var copyOptions = new LobbySearchCopySearchResultByIndexOptions { LobbyIndex = 0 };
        search.CopySearchResultByIndex(ref copyOptions, out LobbyDetails details);

        var attrOptions = new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = "HostPUID" };

        if (details.CopyAttributeByKey(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? attr) == Result.Success)
        {
            string hostId = attr.Value.Data.Value.Value.AsUtf8;

            NetworkManager.singleton.networkAddress = hostId;
            NetworkManager.singleton.StartClient();
            SetupCanvas();
        }
    }

    private void CreateEOSLobby()
    {
        var options = new CreateLobbyOptions
        {
            LocalUserId = EOSSDKComponent.LocalUserProductId,
            MaxLobbyMembers = 8,
            PermissionLevel = LobbyPermissionLevel.Publicadvertised,
            PresenceEnabled = true,
            BucketId = lobbyBucketName
        };

        EOSSDKComponent.GetLobbyInterface().CreateLobby(ref options, null, (ref CreateLobbyCallbackInfo data) =>
        {
            if (data.ResultCode == Result.Success)
            {
                currentLobbyId = data.LobbyId;
                UpdateLobbyWithHostID(data.LobbyId);
            }
        });
    }

    private void UpdateLobbyWithHostID(string lobbyId)
    {
        var modOptions = new UpdateLobbyModificationOptions { LobbyId = lobbyId, LocalUserId = EOSSDKComponent.LocalUserProductId };
        EOSSDKComponent.GetLobbyInterface().UpdateLobbyModification(ref modOptions, out LobbyModification mod);

        var attr = new LobbyModificationAddAttributeOptions
        {
            Attribute = new Epic.OnlineServices.Lobby.AttributeData
            {
                Key = "HostPUID",
                Value = new Epic.OnlineServices.Lobby.AttributeDataValue { AsUtf8 = EOSSDKComponent.LocalUserProductIdString }
            },
            Visibility = LobbyAttributeVisibility.Public
        };
        mod.AddAttribute(ref attr);

        var updateOptions = new UpdateLobbyOptions { LobbyModificationHandle = mod };
        EOSSDKComponent.GetLobbyInterface().UpdateLobby(ref updateOptions, null, (ref UpdateLobbyCallbackInfo data) =>
        {
            SetupCanvas();
        });
    }

    public void SetupCanvas()
    {
        bool isActive = NetworkClient.active || NetworkServer.active;
        PanelStart.SetActive(!isActive);
        PanelStop.SetActive(isActive);
    }

    public void SetupInfoText(string text)
    {
        if (infoText != null) infoText.text = text;
        Debug.Log(text);
    }
}