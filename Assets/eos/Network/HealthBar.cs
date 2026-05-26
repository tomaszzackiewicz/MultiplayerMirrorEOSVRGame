using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class HealthBar : NetworkBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI healthText = null;
        [SerializeField] private Slider healthSlider = null;

        [Header("References")]
        [SerializeField] private NetworkStuff netStuff = null;

        private Vector3 startPosition;

        [SyncVar(hook = nameof(OnHealthChanged))]
        private float health = 100f;

        #region Unity Lifecycle

        void Start()
        {
            startPosition = transform.position;

            // Zainicjalizuj UI – dla wszystkich
            UpdateHealthUI(health);
        }

        #endregion

        #region Obrażenia

        [Server]
        public void TakeDamage(float damage)
        {
            if (health <= 0) return; // już martwy

            health -= damage;
            if (health < 0)
                health = 0;

            // SyncVar sam odpali hooka
        }

        private void OnHealthChanged(float oldHealth, float newHealth)
        {
            UpdateHealthUI(newHealth);

            if (newHealth <= 0)
            {
                if (isOwned)
                {
                    netStuff.EnableControls(false);
                    CursorController.Instance?.UnlockCursor();
                    netStuff.ActivateThirdPerson();
                    Debug.Log("💀 Player died");
                }
            }
        }

        private void UpdateHealthUI(float value)
        {
            if (healthText != null)
                healthText.text = ((int)value).ToString();

            if (healthSlider != null)
                healthSlider.value = value;
        }

        #endregion

        #region Nowa runda / reset gracza

        public void BeginNewRound()
        {
            if (!isOwned) return;

            CursorController.Instance?.LockCursor();
            CmdBeginNewRound();
        }

        [Command]
        private void CmdBeginNewRound()
        {
            health = 100f;

            RpcResetPlayerState(startPosition);
        }

        [ClientRpc]
        private void RpcResetPlayerState(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;

            if (isOwned)
            {
                netStuff.EnableControls(false);
                netStuff.ActivateFirstPerson();
                netStuff.EnableControls(true);
            }

            UpdateHealthUI(health);
        }

        #endregion

        public float GetHealth() => health;
    }
}
