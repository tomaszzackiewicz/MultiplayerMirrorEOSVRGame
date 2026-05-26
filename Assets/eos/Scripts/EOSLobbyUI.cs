using System.Collections.Generic;
using Epic.OnlineServices.Lobby;
using EpicTransport;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Attribute = Epic.OnlineServices.Lobby.Attribute;

public class EOSLobbyUI : MonoBehaviour
{
    private EOSLobby _eosLobby;

    public NetworkManager manager;

    [Header("Handlers")]
    public VRDisconnectHandler disconnectHandler;

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject lobbyListPanel;
    public GameObject currentLobbyPanel;

    [Header("Main Menu Elements")]
    public TMP_InputField lobbyNameInput;
    public Button createLobbyButton;
    public Button findLobbiesButton;

    [Header("Lobby List Elements")]
    public Transform lobbyListContainer;
    public GameObject lobbyEntryPrefab;
    public Button backToMenuButton;

    [Header("Current Lobby Elements")]
    public TMP_Text currentLobbyNameText;
    public Transform playerListContainer;
    public GameObject playerEntryPrefab;
    public Button exitGameButton;
    public Button startGameButton; // NOWOŚĆ: Przycisk startu dla Hosta

    private List<Attribute> _lobbyData = new List<Attribute>();
    private const string LobbyNameKey = "LobbyName";

    private void Awake()
    {
        manager = FindFirstObjectByType<NetworkManager>();
        _eosLobby = FindFirstObjectByType<EOSLobby>();
        SetupButtons();
    }

    private void Start()
    {
        // 1. Wymuszenie czystego startu UI
        SwitchPanel(mainMenuPanel);

        // Ukrywamy przycisk startu na początku
        if (startGameButton != null) startGameButton.gameObject.SetActive(false);
    }

    private void SetupButtons()
    {
        createLobbyButton.onClick.AddListener(CreateLobby);
        findLobbiesButton.onClick.AddListener(() => _eosLobby.FindLobbies());

        if (exitGameButton != null)
            exitGameButton.onClick.AddListener(ExitGame);

        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartActualGame);

        backToMenuButton.onClick.AddListener(() => SwitchPanel(mainMenuPanel));
    }

    private void OnEnable()
    {
        _eosLobby.CreateLobbySucceeded += OnCreateLobbySuccess;
        _eosLobby.JoinLobbySucceeded += OnJoinLobbySuccess;
        _eosLobby.FindLobbiesSucceeded += OnFindLobbiesSuccess;
        _eosLobby.LeaveLobbySucceeded += OnLeaveLobbySuccess;
    }

    private void OnDisable()
    {
        _eosLobby.CreateLobbySucceeded -= OnCreateLobbySuccess;
        _eosLobby.JoinLobbySucceeded -= OnJoinLobbySuccess;
        _eosLobby.FindLobbiesSucceeded -= OnFindLobbiesSuccess;
        _eosLobby.LeaveLobbySucceeded -= OnLeaveLobbySuccess;
    }

    #region EOS Callbacks

    private void OnCreateLobbySuccess(List<Attribute> attributes)
    {
        _lobbyData = attributes;
        UpdateLobbyUI();
        SwitchPanel(currentLobbyPanel);

        // Uruchamiamy Hosta. Mirror nie zmieni sceny, bo Online Scene = Menu Scene.
        manager.StartHost();

        // Pokazujemy przycisk Start, bo jesteśmy Hostem
        if (startGameButton != null) startGameButton.gameObject.SetActive(true);
    }

    private void OnJoinLobbySuccess(List<Attribute> attributes)
    {
        _lobbyData = attributes;

        Attribute hostAddressAttribute = attributes.Find((x) => x.Data.HasValue && x.Data.Value.Key == EOSLobby.hostAddressKey);
        if (hostAddressAttribute.Data.HasValue)
        {
            manager.networkAddress = hostAddressAttribute.Data.Value.Value.AsUtf8;
            UpdateLobbyUI();
            SwitchPanel(currentLobbyPanel);
            manager.StartClient();
        }

        // Klient nie widzi przycisku Start
        if (startGameButton != null) startGameButton.gameObject.SetActive(false);
    }

    private void OnFindLobbiesSuccess(List<LobbyDetails> lobbiesFound)
    {
        foreach (Transform child in lobbyListContainer) Destroy(child.gameObject);

        foreach (var lobby in lobbiesFound)
        {
            GameObject entry = Instantiate(lobbyEntryPrefab, lobbyListContainer);
            Attribute? nameAttr = new Attribute();
            var options = new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = LobbyNameKey };
            lobby.CopyAttributeByKey(ref options, out nameAttr);

            string displayName = nameAttr?.Data?.Value.AsUtf8 ?? "Unknown Lobby";

            var memberOptions = new LobbyDetailsGetMemberCountOptions();
            uint memberCount = lobby.GetMemberCount(ref memberOptions);

            entry.GetComponentInChildren<TMP_Text>().text = $"{displayName} ({memberCount} p)";
            entry.GetComponentInChildren<Button>().onClick.AddListener(() => _eosLobby.JoinLobby(lobby, new[] { LobbyNameKey }));
        }
        SwitchPanel(lobbyListPanel);
    }

    private void OnLeaveLobbySuccess()
    {
        SwitchPanel(mainMenuPanel);
    }

    #endregion

    private void CreateLobby()
    {
        string name = string.IsNullOrEmpty(lobbyNameInput.text) ? "New Lobby" : lobbyNameInput.text;
        _eosLobby.CreateLobby(4, LobbyPermissionLevel.Publicadvertised, false, new[]
        {
            new AttributeData { Key = LobbyNameKey, Value = name }
        });
    }

    public void UpdateLobbyUI()
    {
        var nameAttr = _lobbyData.Find(x => x.Data.HasValue && x.Data.Value.Key == LobbyNameKey);
        currentLobbyNameText.text = "Lobby: " + (nameAttr.Data?.Value.AsUtf8 ?? "N/A");

        // Odświeżanie listy graczy
        Invoke(nameof(RefreshPlayerList), 0.5f);
    }

    private void RefreshPlayerList()
    {
        if (this == null || playerListContainer == null) return;

        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        if (_eosLobby.ConnectedLobbyDetails == null) return;

        var memberOptions = new LobbyDetailsGetMemberCountOptions();
        uint playerCount = _eosLobby.ConnectedLobbyDetails.GetMemberCount(ref memberOptions);

        for (int i = 0; i < playerCount; i++)
        {
            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
            entry.GetComponentInChildren<TMP_Text>().text = $"Player {i + 1}";
        }
    }

    // TA FUNKCJA PRZEŁĄCZA SCENĘ DLA WSZYSTKICH
    public void StartActualGame()
    {
        if (NetworkServer.active)
        {
            Debug.Log("Host starts the game!");
            // Wpisz nazwę swojej sceny z grą
            manager.ServerChangeScene("GameScene");
        }
    }

    private void SwitchPanel(GameObject targetPanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(targetPanel == mainMenuPanel);
        if (lobbyListPanel != null) lobbyListPanel.SetActive(targetPanel == lobbyListPanel);
        if (currentLobbyPanel != null) currentLobbyPanel.SetActive(targetPanel == currentLobbyPanel);
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}