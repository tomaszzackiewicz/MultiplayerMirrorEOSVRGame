using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class LobbyManager : MonoBehaviour
    {
        [Header("UI – Player List")]
        [SerializeField] private LobbyPresenter lobbyPresenter;
        [SerializeField] private LobbyDiscovery discovery;

        private FpsNetworkManager netManager = null;
        private bool returningHome = false;

        public static LobbyManager Instance { get; private set; }

        private void Log(string message, LogLevel level = LogLevel.Info)
        {
            GameLogger.Instance?.Log(message, level);
        }

        private void Awake()
        {
            netManager = (FpsNetworkManager)NetworkManager.singleton;
            Instance = this;
        }

        private void Start()
        {
            Log("Select an option...");

            if (NetworkClient.active)
            {
                InvokeRepeating(nameof(RefreshLobbyManually), 1.0f, 1.5f);
            }

            netManager.LeaveLobby();
        }

        private void OnEnable()
        {
            lobbyPresenter.onHostLobby += OnHostLobby;
            lobbyPresenter.onJoinLobby += OnJoinLobby;
            lobbyPresenter.onCloseAddressPanel += OnCloseAddressPanel;
            lobbyPresenter.onJoin += OnJoinButton;
            lobbyPresenter.onStartGame += OnStartGame;
            lobbyPresenter.onLeaveGame += OnLeaveGame;
            lobbyPresenter.onToggleReady += OnToggleReady;
            lobbyPresenter.onReturnToMenu += OnReturnToMenu;
            lobbyPresenter.onRejoinGame += OnRejoinGame;
        }

        private void OnDisable()
        {
            lobbyPresenter.onHostLobby -= OnHostLobby;
            lobbyPresenter.onJoinLobby -= OnJoinLobby;
            lobbyPresenter.onCloseAddressPanel -= OnCloseAddressPanel;
            lobbyPresenter.onJoin -= OnJoinButton;
            lobbyPresenter.onStartGame -= OnStartGame;
            lobbyPresenter.onLeaveGame -= OnLeaveGame;
            lobbyPresenter.onToggleReady -= OnToggleReady;
            lobbyPresenter.onReturnToMenu -= OnReturnToMenu;
            lobbyPresenter.onRejoinGame -= OnRejoinGame;
        }

        public void InitializeDiscovery()
        {
            if (NetworkServer.active && discovery != null)
            {
                //discovery.CurrentTransport = Transport.active;
                discovery.AdvertiseServer();
            }
        }

        public void Update()
        {
            if (NetworkServer.active && !discovery.IsAdvertising)
            {
                discovery.AdvertiseServer();
            }
            else
            {
                returningHome = false;
            }
        }

        private void OnHostLobby()
        {
            Log("Creating lobby...");
            netManager.NetworkProfile.Host(netManager);
            StartCoroutine(DelayedDiscoveryStart());
        }

        private IEnumerator DelayedDiscoveryStart()
        {
            yield return new WaitForSeconds(0.5f); // lub yield return null; dla 1 klatki
            InitializeDiscovery();
        }

        public void OnJoinLobby()
        {
            string address = lobbyPresenter.GetAddressInput();

            if (string.IsNullOrEmpty(address))
            {
                Log("Please enter a network address.");
                return;
            }

            if (!IsValidAddress(address))
            {
                Log("Invalid address format.");
                return;
            }

            Log("Connecting...");
            netManager.NetworkProfile.Join(address, netManager);
            UIManager.Instance?.ShowLobby();
        }

        private bool IsValidAddress(string address)
        {
            return
                Regex.IsMatch(address, @"^(\d{1,3}\.){3}\d{1,3}$") ||         // IPv4
                Regex.IsMatch(address, @"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$") ||  // domain
                address.Equals("localhost", System.StringComparison.OrdinalIgnoreCase);
        }

        public void OnCloseAddressPanel()
        {
            UIManager.Instance?.ShowLandingPage();
        }

        public void OnJoinButton()
        {
            UIManager.Instance?.ShowEnterAddressPanel();
            lobbyPresenter.SetAddressInputText("localhost");
        }

        public void OnStartGame()
        {
            netManager.StartGame();
            Log("Starting game...");
        }

        public void OnLeaveGame()
        {
            netManager.LeaveGame();
            Log("Leaving lobby...");
        }

        public void RefreshLobbyManually()
        {
            List<PlayerScript> players = new(FindObjectsByType<PlayerScript>(FindObjectsSortMode.None));
            lobbyPresenter.UpdateLobbyUI(players);
        }

        public void OnToggleReady()
        {
            if (NetworkClient.connection != null && NetworkClient.connection.identity != null)
            {
                PlayerScript local = NetworkClient.connection.identity.GetComponent<PlayerScript>();
                if (local != null)
                {
                    local.CmdToggleReadyStatus();
                }
            }
        }

        public void OnReturnToMenu()
        {
            NetworkManager.singleton.StopHost();
            SceneManager.LoadScene("MainMenuScene");
        }

        public void OnRejoinGame()
        {
            if (NetworkClient.isConnected)
            {
                Log("[Client] Already connected — loading gameplay scene.");
                string chosenScene = netManager.MapHandler.CurrentMap;
                SceneManager.LoadScene(chosenScene);
            }
            else
            {
                Log("[Client] Not connected — connecting to host first.");
                StartCoroutine(ReconnectAndJoin());
            }
        }

        private IEnumerator ReconnectAndJoin()
        {
            NetworkManager.singleton.StartClient();

            float timeout = 5f;
            float elapsed = 0f;
            while (!NetworkClient.isConnected && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (NetworkClient.isConnected)
            {
                if (netManager?.MapHandler == null)
                {
                    Log("❌ MapHandler not available. Cannot reconnect.");
                    yield break;
                }

                Log("[Client] Reconnected — loading gameplay scene.");
                string chosenScene = netManager.MapHandler.CurrentMap;
                SceneManager.LoadScene(chosenScene);
            }
            else
            {
                Log("[Client] Timeout — failed to connect to host.");
            }
        }

        private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string chosenScene = netManager.MapHandler.CurrentMap;
            if (scene.name == chosenScene)
            {
                SceneManager.sceneLoaded -= OnGameSceneLoaded;

                if (NetworkClient.isConnected && NetworkClient.connection != null && NetworkClient.connection.identity == null)
                {
                    Log("[Client] Adding player on client side.");
                    NetworkClient.AddPlayer();
                }
                else
                {
                    Log("[Client] Player already exists — skipping add.");
                }
            }
        }
    }
}