using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoveryWatchdog : MonoBehaviour
{
    [SerializeField] private LobbyDiscovery discovery;
    [SerializeField] private float refreshInterval = 5f;

    private Dictionary<long, LobbyServerResponse> discoveredServers = new();

    void Start()
    {
        if (discovery == null)
        {
            Debug.LogError("[WATCHDOG] Nie przypisano komponentu discovery.");
            enabled = false;
            return;
        }

        // Poprawione dopasowanie do UnityEvent<LobbyServerResponse>
        discovery.OnServerFound.AddListener((LobbyServerResponse response) => OnServerFound(response));
        StartCoroutine(WatchDiscovery());
    }

    private void OnServerFound(LobbyServerResponse response)
    {
        discoveredServers[response.serverId] = response;
    }

    private IEnumerator WatchDiscovery()
    {
        while (true)
        {
            discoveredServers.Clear();

            Debug.Log("[WATCHDOG] Odświeżam listę serwerów...");
            discovery.StartDiscovery();

            yield return new WaitForSeconds(refreshInterval);

            if (discoveredServers.Count == 0)
            {
                Debug.Log("[WATCHDOG] Brak serwerów.");
            }
            else
            {
                Debug.Log($"[WATCHDOG] Znaleziono {discoveredServers.Count} serwer(y):");

                foreach (var server in discoveredServers.Values)
                {
                    Debug.Log($"  🟢 {server.name} ({server.playerCount} graczy) @ {server.uri}");
                }
            }

            yield return new WaitForSeconds(refreshInterval);
        }
    }
}