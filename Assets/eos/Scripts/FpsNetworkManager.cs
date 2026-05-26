using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class FpsNetworkManager : NetworkManager
    {
        [Header("Custom References")]
        [SerializeField] private GameObject playerLobbyPrefab;
        [SerializeField] private GameObject playerGamePrefab;
        [Header("Network Profile")]
        [SerializeField] private NetworkProfile networkProfile;

        [Header("Maps")]
        [SerializeField] private int numberOfRounds = 1;
        [SerializeField] private MapSet mapSet = null;
        [SerializeField] private bool isGameMapsLooped = true;

        private MapHandler mapHandler = null;
        private bool mapHandlerInitialized = false;

        public MapHandler MapHandler => mapHandler;
        public NetworkProfile NetworkProfile => networkProfile;
        public string RoomName { get; set; } = "DefaultRoom";
        public bool IsPrivate { get; set; } = false;

        public bool IsPrivateServer()
        {
            return IsPrivate;
        }

        public string GetRoomName()
        {
            return RoomName;
        }

        private List<PlayerScript> playersList = new List<PlayerScript>();
        public IReadOnlyList<PlayerScript> Players => playersList.AsReadOnly();

        public static event Action<List<PlayerScript>> OnPlayerListUpdated;

        private void Log(string message, LogLevel level = LogLevel.Info)
        {
            GameLogger.Instance?.Log(message, level);
        }

        public NetworkProfile ActiveProfile { get; private set; }

        public override void Start()
        {
            base.Start();

            SetActiveProfile(networkProfile);
        }

        public void SetActiveProfile(NetworkProfile profile)
        {
            if (profile == null)
            {
                Log("[FpsNetworkManager] Profile is null — cannot configure transport.");
                return;
            }

            ActiveProfile = profile;
            ActiveProfile.ConfigureTransport(this);
        }

        public override void Awake()
        {
            base.Awake();

            SceneContext.Initialize(mapSet);
        }

        public void SetNetworkAddress(string address)
        {
            networkAddress = address;
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            RoleType connectionRole = conn == NetworkServer.localConnection ? RoleType.Server : RoleType.Client;
            Transform startPos = GetAvailableStartPosition(connectionRole);
            bool isLobbyScene = SceneContext.IsLobbyScene();

            GameObject prefab = (isLobbyScene) ? playerLobbyPrefab : playerGamePrefab;
            GameObject player = (startPos != null) ? Instantiate(prefab, startPos.position, startPos.rotation) : Instantiate(prefab);

            NetworkServer.AddPlayerForConnection(conn, player);

            var playerScript = player.GetComponent<PlayerScript>();
            if (playerScript != null)
            {
                playerScript.StartPositionTransform = startPos;
                playersList.Add(playerScript);
                OnPlayerListUpdated?.Invoke(playersList);
            }

            if (startPos != null)
            {
                var spawnState = startPos.GetComponent<NetworkSpawnState>();
                if (spawnState != null)
                {
                    spawnState.SetOccupied(true);
                }
            }
        }

        private Transform GetAvailableStartPosition(RoleType role)
        {
            var allPositions = FindObjectsByType<CustomNetworkStartPosition>(FindObjectsSortMode.None);

            foreach (var pos in allPositions)
            {
                if (pos.RoleType != role) continue;

                var state = pos.GetComponent<NetworkSpawnState>();
                if (state != null && !state.IsOccupied)
                {
                    return pos.transform;
                }
            }

            return null;
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (conn.identity != null)
            {
                var playerScript = conn.identity.GetComponent<PlayerScript>();
                if (playerScript != null)
                {
                    playersList.Remove(playerScript);
                    OnPlayerListUpdated?.Invoke(playersList);
                }
            }

            base.OnServerDisconnect(conn);
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Log("[Client] Disconnected — failed to connect to the server.", LogLevel.Warning);
            UnityEngine.SceneManagement.SceneManager.LoadScene(Consts.lobbyScene);
        }

        public override void OnStopHost()
        {
            LobbyDiscovery discovery = FindAnyObjectByType<LobbyDiscovery>();
            if (discovery != null)
                discovery?.StopDiscovery();

            base.OnStopHost();

            UnityEngine.SceneManagement.SceneManager.LoadScene(Consts.lobbyScene);
        }

        public override void OnStopClient()
        {
            LobbyDiscovery discovery = FindAnyObjectByType<LobbyDiscovery>();
            if (discovery != null)
                discovery?.StopDiscovery();

            base.OnStopClient();
        }

        public void LeaveGame()
        {
            if (NetworkServer.active || NetworkClient.isConnected)
                StopHost();
            else
                StopClient();

            CleanupPlayers();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
        }

        public void LeaveLobby()
        {
            StartCoroutine(SafeLeaveCoroutine());
        }

        private IEnumerator SafeLeaveCoroutine()
        {
            if (NetworkServer.active || NetworkClient.isConnected)
                StopHost();
            else
                StopClient();

            yield return new WaitUntil(() => !NetworkClient.active && !NetworkServer.active);

            CleanupPlayers();
        }

        public void StartGame()
        {
            if (NetworkServer.active)
            {
                Debug.Log("mapHandler.NextMap() " + mapHandler.CurrentMap);
                ServerChangeScene(mapHandler.CurrentMap);
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (mapHandlerInitialized) return;
            mapHandlerInitialized = true;

            mapHandler = new MapHandler(mapSet, numberOfRounds, isGameMapsLooped);

            mapHandler.OnRoundAdvanced += (mapName, roundNum) =>
            {
                RoundTimer.Instance.SetRoundData(mapName, roundNum);
            };

            mapHandler.OnMapChanged += sceneName =>
            {
                if (NetworkServer.active)
                {
                    Debug.Log($"Change the current scene to: {sceneName}");
                    ServerChangeScene(sceneName);
                }
            };

            mapHandler.OnNoMoreMaps += () =>
            {
                if (NetworkServer.active)
                {
                    ServerChangeScene(mapHandler.LobbyScene);
                }
            };
        }

        public void ReturnToLobby()
        {
            if (!NetworkServer.active) return;

            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn != null && conn.identity != null)
                {
                    NetworkServer.Destroy(conn.identity.gameObject);
                }
            }

            CleanupPlayers();
            ServerChangeScene(Consts.lobbyScene);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            string chosenScene = mapHandler.CurrentMap;
            if (SceneContext.IsGameplayScene(chosenScene))
            {
                var newPlayersList = new List<PlayerScript>();

                foreach (var conn in NetworkServer.connections.Values)
                {
                    RoleType connectionRole = conn == NetworkServer.localConnection ? RoleType.Server : RoleType.Client;

                    Transform startPos = GetAvailableStartPosition(connectionRole);
                    Vector3 spawnPos = startPos?.position ?? Vector3.zero;
                    Quaternion spawnRot = startPos?.rotation ?? Quaternion.identity;

                    if (startPos == null)
                        Debug.LogWarning($"Brak dostępnych punktów startowych dla roli: {connectionRole}. Spawnowanie na (0,0,0)");

                    GameObject player = Instantiate(playerGamePrefab, spawnPos, spawnRot);

                    if (conn.identity != null)
                        NetworkServer.ReplacePlayerForConnection(conn, player, ReplacePlayerOptions.Destroy);
                    else
                        NetworkServer.AddPlayerForConnection(conn, player);

                    var netStuff = player.GetComponent<NetworkStuff>();
                    if (netStuff != null)
                    {
                        netStuff.StartPositionTransform = startPos;

                        if (SessionFlags.Instance != null && SessionFlags.Instance.PlayerDisplayNames.TryGetValue(conn, out string savedName))
                        {
                            netStuff.SetDisplayName(savedName);
                        }
                    }

                    var script = player.GetComponent<PlayerScript>();
                    if (script != null)
                        newPlayersList.Add(script);

                    if (startPos != null)
                    {
                        var spawnState = startPos.GetComponent<NetworkSpawnState>();
                        if (spawnState != null)
                            spawnState.SetOccupied(true);
                    }
                }

                playersList = newPlayersList;
                OnPlayerListUpdated?.Invoke(playersList);
            }
        }

        private void CleanupPlayers()
        {
            playersList.Clear();
            OnPlayerListUpdated?.Invoke(playersList);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();

            if (NetworkClient.isConnected && NetworkClient.connection != null && NetworkClient.connection.identity == null)
            {
                Log("[Client] Adding player on client side.");
                NetworkClient.AddPlayer();
            }
            else
            {
                Log("[Client] Player already exists — not adding again.");
            }
        }
    }
}