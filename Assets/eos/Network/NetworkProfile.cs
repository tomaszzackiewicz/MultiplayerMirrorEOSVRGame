using Mirror;
using Player;
using UnityEngine;

public abstract class NetworkProfile : ScriptableObject
{
    [Header("Transport used by this profile")]
    [SerializeField] private GameObject transportPrefab;

    protected GameObject createdTransportGO;

    public abstract string GetLocalPlayerName();

    public abstract void Host(FpsNetworkManager netManager);

    public abstract void Join(string address, FpsNetworkManager netManager);

    public virtual void ConfigureTransport(FpsNetworkManager netManager)
    {
        string logPrefix = $"[Profile:{name}]";

        if (Transport.active != null)
        {
            Debug.Log($"{logPrefix} Transport already active: {Transport.active.name} — skipping configuration.");
            netManager.transport = Transport.active;
            return;
        }

        if (transportPrefab == null)
        {
            Debug.LogError($"{logPrefix} Transport prefab is not assigned!");
            return;
        }

        createdTransportGO = Object.Instantiate(transportPrefab);

        if (createdTransportGO == null)
        {
            Debug.LogError($"{logPrefix} Failed to instantiate transport prefab.");
            return;
        }

        Transport transport = createdTransportGO.GetComponent<Transport>();
        if (transport == null)
        {
            Debug.LogError($"{logPrefix} Prefab does not contain a Transport component!");
            Object.Destroy(createdTransportGO);
            createdTransportGO = null;
            return;
        }

        Transport.active = transport;
        netManager.transport = transport;

        Object.DontDestroyOnLoad(createdTransportGO);

        Debug.Log($"{logPrefix} Transport instantiated and activated: {transport.GetType().Name}");
    }

    public virtual void Cleanup()
    {
        string logPrefix = $"[Profile:{name}]";

        if (createdTransportGO != null)
        {
            Object.Destroy(createdTransportGO);
            Transport.active = null;
            createdTransportGO = null;

            Debug.Log($"{logPrefix} Transport created by profile has been destroyed.");
        }
        else if (Transport.active != null)
        {
            Debug.LogWarning($"{logPrefix} Transport.active exists but was not created by this profile — not destroying.");
        }
        else
        {
            Debug.Log($"{logPrefix} No active transport to clean up.");
        }
    }
}