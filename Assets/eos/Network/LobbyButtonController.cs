using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LobbyButtonController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button leaveLobbyButton;

        private void Start()
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool isServer = NetworkServer.active;
            bool isClient = NetworkClient.active;
            bool isLocalPlayerAvailable = NetworkClient.localPlayer != null;

            if (startGameButton != null)
                startGameButton.interactable = isServer && isClient;

            if (readyButton != null)
                readyButton.interactable = isLocalPlayerAvailable;

            if (leaveLobbyButton != null)
                leaveLobbyButton.interactable = true; //isClient || isServer;
        }

        public void ForceRefresh() => UpdateButtonStates();
    }
}