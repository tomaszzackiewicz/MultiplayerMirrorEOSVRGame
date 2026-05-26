using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Epic.OnlineServices.Lobby;
using Mirror;
using Attribute = Epic.OnlineServices.Lobby.Attribute;

public class VRMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EOSLobby eosLobby;
    [SerializeField] private NetworkManager networkManager;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject lobbyListPanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private Transform lobbyListContainer;
    [SerializeField] private GameObject lobbyEntryPrefab;

    private void OnEnable()
    {
        eosLobby.CreateLobbySucceeded += OnCreateLobbySuccess;
        eosLobby.CreateLobbyFailed += OnOperationFailed;
        eosLobby.JoinLobbySucceeded += OnJoinLobbySuccess;
        eosLobby.JoinLobbyFailed += OnOperationFailed;

        eosLobby.FindLobbiesSucceeded += OnLobbiesFound;
    }

    private void OnDisable()
    {
        eosLobby.CreateLobbySucceeded -= OnCreateLobbySuccess;
        eosLobby.CreateLobbyFailed -= OnOperationFailed;
        eosLobby.JoinLobbySucceeded -= OnJoinLobbySuccess;
        eosLobby.JoinLobbyFailed -= OnOperationFailed;

        eosLobby.FindLobbiesSucceeded -= OnLobbiesFound;
    }

    private void Start()
    {


        UpdateStatus("Ready to connect...");
    }

    #region Public Buttons (Podepnij pod przyciski w Inspektorze)

    public void UI_HostGame()
    {
        // Sprawdzamy czy nazwa nie jest pusta
        string name = string.IsNullOrWhiteSpace(lobbyNameInput.text) ? "VR Session" : lobbyNameInput.text;

        UpdateStatus("Creating lobby...");
        ShowPanel(loadingPanel);

        // Tworzymy tablicę atrybutów - SDK samo zajmie się ich widocznością
        // jeśli metoda CreateLobby w EOSLobby.cs jest poprawnie napisana.
        AttributeData[] lobbyAttributes = new[]
        {
        new AttributeData { Key = "LobbyName", Value = name }
    };

        // Wywołanie metody z Twojego EOSLobby
        eosLobby.CreateLobby(4, LobbyPermissionLevel.Publicadvertised, false, lobbyAttributes);
    }

    public void UI_FindGames()
    {
        UpdateStatus("Searching for lobbies...");
        ShowPanel(loadingPanel);

        // Wydłużony timeout do 20 sekund
        CancelInvoke(nameof(SearchTimeout));
        Invoke(nameof(SearchTimeout), 5f);

        eosLobby.FindLobbies();
    }

    private void SearchTimeout()
    {
        UpdateStatus("<color=yellow>Search timeout. No response from EOS.</color>");
        Invoke(nameof(UI_BackToMain), 3.0f);
    }

    #endregion

    #region EOS Callbacks

    private void OnCreateLobbySuccess(List<Attribute> attributes)
    {
        UpdateStatus("Lobby Created! Starting Host...");
        networkManager.StartHost();
        ShowPanel(null);
    }

    private void OnLobbiesFound(List<LobbyDetails> foundLobbies)
    {
        // 1. Czyścimy kontener (jak w HUD)
        foreach (Transform child in lobbyListContainer) Destroy(child.gameObject);

        foreach (LobbyDetails lobby in foundLobbies)
        {
            // 2. Pobieramy atrybut nazwy dokładnie jak w przykładzie
            Attribute? nameAttr = new Attribute();
            var copyOptions = new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = "LobbyName" };
            lobby.CopyAttributeByKey(ref copyOptions, out nameAttr);

            // 3. Pobieramy liczbę graczy
            var memberOptions = new LobbyDetailsGetMemberCountOptions();
            uint count = lobby.GetMemberCount(ref memberOptions);

            if (count > 0)
            {
                string name = nameAttr.HasValue ? nameAttr.Value.Data.Value.Value.AsUtf8 : "Unknown";

                // 4. Tworzymy przycisk i przekazujemy klucze przy Join
                GameObject btn = Instantiate(lobbyEntryPrefab, lobbyListContainer);
                btn.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    // To jest kluczowe: przekazujemy "LobbyName" w tablicy
                    eosLobby.JoinLobby(lobby, new[] { "LobbyName" });
                });
            }
        }
    }

    // W VRMenuController.cs
    private void OnJoinLobbySuccess(List<Attribute> attributes)
    {
        // Szukamy klucza hostAddressKey zdefiniowanego w EOSLobby
        Attribute hostAddressAttribute = attributes.Find((x) =>
            x.Data.HasValue && x.Data.Value.Key == EOSLobby.hostAddressKey);

        if (hostAddressAttribute.Data.HasValue)
        {
            VRNetworkManager.singleton.networkAddress = hostAddressAttribute.Data.Value.Value.AsUtf8;
            VRNetworkManager.singleton.StartClient();
        }
    }

    private void OnOperationFailed(string error)
    {
        UpdateStatus("<color=red>Error: " + error + "</color>");
        ShowPanel(mainPanel);
    }

    #endregion

    private void ShowPanel(GameObject panelToShow)
    {
        if (mainPanel != null) mainPanel.SetActive(mainPanel == panelToShow);
        if (lobbyListPanel != null) lobbyListPanel.SetActive(lobbyListPanel == panelToShow);
        if (loadingPanel != null) loadingPanel.SetActive(loadingPanel == panelToShow);
    }

    private void UpdateStatus(string message)
    {
        statusText.text = message;
        Debug.Log($"[MenuStatus]: {message}");
    }

    public void UI_BackToMain()
    {
        UpdateStatus("Ready...");

        // Czyścimy listę lobby, żeby nie zajmowała pamięci, gdy jej nie widzimy
        if (lobbyListContainer != null)
        {
            foreach (Transform child in lobbyListContainer)
                Destroy(child.gameObject);
        }

        ShowPanel(mainPanel);
    }
}