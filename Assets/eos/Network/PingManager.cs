using Mirror;
using UnityEngine;

public class PingManager : NetworkBehaviour
{
    private float lastPingTime;
    public float currentPing;

    [ClientCallback]
    void Update()
    {
        if (isClient && Time.time - lastPingTime > 2f)
        {
            lastPingTime = Time.time;
            CmdSendPing(Time.time);
        }
    }

    [Command]
    void CmdSendPing(float clientTime)
    {
        TargetReceivePong(connectionToClient, clientTime);
    }

    [TargetRpc]
    void TargetReceivePong(NetworkConnection target, float clientTime)
    {
        currentPing = (Time.time - clientTime) * 1000f; // w milisekundach
    }
}