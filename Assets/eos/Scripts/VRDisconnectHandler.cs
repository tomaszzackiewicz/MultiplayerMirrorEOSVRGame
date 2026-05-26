using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class VRDisconnectHandler : MonoBehaviour
{
    private EOSLobby _eosLobby;
    public static bool IsLeaving { get; private set; }

    private void Awake()
    {
        _eosLobby = FindFirstObjectByType<EOSLobby>();
    }

    private void OnEnable()
    {
        if (_eosLobby == null) _eosLobby = FindFirstObjectByType<EOSLobby>();
        if (_eosLobby != null)
        {
            _eosLobby.LeaveLobbySucceeded += FinishNetworkShutdown;
        }
    }

    private void OnDisable()
    {
        if (_eosLobby != null)
        {
            _eosLobby.LeaveLobbySucceeded -= FinishNetworkShutdown;
        }
    }

    public void LeaveGame()
    {
        if (IsLeaving || _eosLobby == null) return;

        // Sprawdzamy, czy w ogóle jesteśmy połączeni
        if (!_eosLobby.ConnectedToLobby && !NetworkClient.isConnected)
        {
            Debug.LogWarning("[Disconnect] Nie jesteś w lobby ani nie masz aktywnego połączenia.");
            return;
        }

        IsLeaving = true;
        Debug.Log("[Disconnect] Krok 1: Wychodzenie z EOS Lobby...");

        // Wywołujemy wyjście w EOS. To wyzwoli zdarzenie LeaveLobbySucceeded
        _eosLobby.LeaveLobby();

        // Backup: jeśli EOS nie odpowie (timeout), wymuś zamknięcie Mirror po 4 sekundach
        Invoke(nameof(FinishNetworkShutdown), 4.0f);
    }

    private void FinishNetworkShutdown()
    {
        CancelInvoke(nameof(FinishNetworkShutdown));

        Debug.Log("[Disconnect] Krok 2: Zamykanie Mirror Network...");

        if (NetworkManager.singleton != null)
        {
            if (NetworkServer.active) NetworkManager.singleton.StopHost();
            else if (NetworkClient.isConnected) NetworkManager.singleton.StopClient();
        }

        SceneManager.LoadScene("Lobby");

        IsLeaving = false;
    }
}