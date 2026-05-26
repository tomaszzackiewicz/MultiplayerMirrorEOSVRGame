using Mirror;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerScript : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnDisplayNameChanged))]
        private string displayName;

        [SyncVar(hook = nameof(OnReadyStatusChanged))]
        private bool isReady;

        public bool IsReady => isReady;

        [SerializeField] private TextMeshProUGUI nameText;

        public string DisplayName => displayName;

        public Transform StartPositionTransform { get; set; } = null;

        public override void OnStartServer()
        {
            ChatManager.Instance?.RegisterPlayer(displayName, connectionToClient);
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            var netManager = (FpsNetworkManager)NetworkManager.singleton;
            string newName = netManager.ActiveProfile?.GetLocalPlayerName() ?? $"Player_{Random.Range(1000, 9999)}";

            LobbyButtonController controller = FindAnyObjectByType<LobbyButtonController>();
            if (controller != null)
                controller.ForceRefresh();

            CmdSetDisplayName(newName);
        }

        [Command]
        private void CmdSetDisplayName(string name)
        {
            displayName = name;

            if (SessionFlags.Instance != null)
            {
                SessionFlags.Instance.PlayerDisplayNames[connectionToClient] = name;
            }
        }

        public void SetDisplayName(string name)
        {
            displayName = name;
            nameText.text = name;
        }

        private void OnDisplayNameChanged(string _, string newName)
        {
            if (nameText != null)
                nameText.text = newName;

            // LobbyManager manager = FindAnyObjectByType<LobbyManager>();
            // if (manager != null)
            //     manager.RefreshLobbyManually();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            //DontDestroyOnLoad(gameObject);

            if (isLocalPlayer)
            {
                NetworkClient.Send(new ChatHistoryRequestMessage());
            }

            if (nameText != null)
                nameText.text = displayName;
        }

        [Command]
        public void CmdToggleReadyStatus()
        {
            isReady = !isReady;
        }

        private void OnReadyStatusChanged(bool _, bool newValue)
        {
            // Trigger UI update (np. RefreshLobbyManually)
            // LobbyManager manager = FindAnyObjectByType<LobbyManager>();
            // if (manager != null)
            //     manager.RefreshLobbyManually();
        }

        [Command]
        public void CmdResetReady()
        {
            isReady = false;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == Consts.lobbyScene && isLocalPlayer)
            {
                CmdResetReady();
                CursorController.Instance?.UnlockCursor();
            }
        }
    }
}