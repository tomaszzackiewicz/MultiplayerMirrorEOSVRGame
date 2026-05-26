using Mirror;
using Player;
using UnityEngine;

public class ServerJoinButton : MonoBehaviour
{
    public string ipAddress;

    public void JoinServer()
    {
        FpsNetworkManager manager = NetworkManager.singleton as FpsNetworkManager;
        if (manager == null)
        {
            Debug.LogError("Brak FPSNetworkManager w scenie!");
            return;
        }

        manager.networkAddress = ipAddress;
        manager.StartClient();
        Debug.Log("Próba po³¹czenia z serwerem: " + ipAddress);
    }
}