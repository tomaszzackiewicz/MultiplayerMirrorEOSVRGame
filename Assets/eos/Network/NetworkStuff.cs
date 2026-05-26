using Mirror;
using UnityEngine;

namespace Player
{
    public class NetworkStuff : NetworkBehaviour
    {
        [SerializeField] private GameObject fpsCamera = null, tpMesh = null, tpModelWeapon = null, afterDeathCamera = null;
        [SerializeField] private Animator tpAnimator = null;
        //[SerializeField] private Movement _movement;
        [SerializeField] private CharacterController _characterController;
        //[SerializeField] private Fire fire;
        [SerializeField] private GameObject nameCanvas = null;

        [SyncVar(hook = nameof(OnDisplayNameChanged))]
        private string displayName;

        public string DisplayName => displayName;
        public Transform StartPositionTransform { get; set; } = null;

        private void Start()
        {
            if (isLocalPlayer)
            {
                fpsCamera.SetActive(true);
                tpMesh.SetActive(false);
                tpModelWeapon.SetActive(false);
                nameCanvas.SetActive(false);
            }
            else
            {
                fpsCamera.SetActive(false);
                tpMesh.SetActive(true);
                tpModelWeapon.SetActive(true);
                nameCanvas.SetActive(true);
            }
        }

        public void ActivateFirstPerson()
        {
            fpsCamera.SetActive(true);
            afterDeathCamera.SetActive(false);
            tpMesh.SetActive(false);
            nameCanvas.SetActive(false);
            tpAnimator.SetBool("isDead", false);
            tpAnimator.SetBool("isWalking", false);
        }

        public void ActivateThirdPerson()
        {
            fpsCamera.SetActive(false);
            afterDeathCamera.SetActive(true);
            tpMesh.SetActive(true);
            nameCanvas.SetActive(true);
            tpAnimator.SetBool("isDead", true);
            tpAnimator.SetBool("isWalking", false);
        }

        public void EnableControls(bool enabled)
        {
            //_movement.enabled = enabled;
            _characterController.enabled = enabled;
            // if (enabled) fire.enabled = true;
            // else fire.enabled = false;
        }

        public override void OnStopServer()
        {
            if (StartPositionTransform != null)
            {
                var spawnState = StartPositionTransform.GetComponent<NetworkSpawnState>();
                if (spawnState != null)
                {
                    spawnState.SetOccupied(false);
                }
            }
        }

        public void SetDisplayName(string name)
        {
            displayName = name;
        }

        private void OnDisplayNameChanged(string _, string newValue)
        {
            if (TryGetComponent(out NameTag tag))
            {
                tag.SetText(newValue);
            }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            LocalCameraHolder.LocalCamera = fpsCamera.GetComponent<Camera>();
        }

    }
}