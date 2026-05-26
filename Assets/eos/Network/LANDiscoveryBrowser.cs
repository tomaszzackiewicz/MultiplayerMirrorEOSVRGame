using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LANDiscoveryBrowser : MonoBehaviour
{
    [Header("Discovery")]
    [SerializeField] private LobbyDiscovery discovery;

    [Header("UI")]
    [SerializeField] private GameObject serverListItemPrefab;
    [SerializeField] private Transform serverListRoot;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button joinButton;

    private readonly List<GameObject> serverEntries = new();
    private readonly HashSet<long> displayedServerIds = new();


    private void Start()
    {
        if (discovery != null)
        {
            discovery.OnServerFound.AddListener(OnServerFound);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshServerList);
        }

        if (joinButton != null)
        {
            joinButton.onClick.AddListener(JoinSelectedServer);
            //joinButton.interactable = false;
        }
    }

    public void RefreshServerList()
    {
        Debug.Log("[DISCOVERY UI] Odświeżanie listy serwerów...");

        foreach (var entry in serverEntries)
        {
            Destroy(entry);
        }
        serverEntries.Clear();
        displayedServerIds.Clear();

        addressInput.text = "";

        if (discovery != null)
        {
            Debug.Log("[DISCOVERY UI] Transport przypisany ręcznie: " + discovery.CurrentTransport?.GetType().Name);
            Debug.Log("[DISCOVERY UI] Wywołuję discovery.StartDiscovery()");
            discovery.StartDiscovery();
        }
        else
        {
            Debug.LogWarning("[DISCOVERY UI] Brak przypisanego discovery!");
        }
    }

    public void OnServerFound(LobbyServerResponse response)
    {
        if (displayedServerIds.Contains(response.serverId))
            return;

        displayedServerIds.Add(response.serverId);

        // ✳️ Uzupełnij dane w dispatcherze
        LobbyInfoDispatcher.Instance.SetFromServerResponse(response);

        GameObject entry = Instantiate(serverListItemPrefab, serverListRoot);

        // 🔗 Przekaż adres do przycisku
        ServerJoinButton join = entry.GetComponent<ServerJoinButton>();
        join.ipAddress = response.uri.Host;

        // 🎨 Przekaż dane do UI z dispatchera
        ServerListItemPresenter presenter = entry.GetComponent<ServerListItemPresenter>();
        presenter.PopulateFromDispatcher(LobbyInfoDispatcher.Instance);

        // 🟢 Obsłuż kliknięcie
        Button button = entry.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(join.JoinServer);
    }

    public void JoinSelectedServer()
    {
        string address = addressInput.text;

        if (!string.IsNullOrEmpty(address))
        {
            NetworkManager.singleton.networkAddress = address;
            NetworkManager.singleton.StartClient();
            Debug.Log($"[DISCOVERY UI] Dołączam do {address}");
        }
        else
        {
            Debug.LogWarning("[DISCOVERY UI] Brak wybranego adresu serwera!");
        }
    }

    private void OnDestroy()
    {
        if (discovery != null)
        {
            discovery.OnServerFound.RemoveListener(OnServerFound);
        }
    }
}