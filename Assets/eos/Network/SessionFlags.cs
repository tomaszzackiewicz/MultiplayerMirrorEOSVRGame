using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class SessionFlags : MonoBehaviour
{
    public static SessionFlags Instance;
    [SerializeField] private bool returnedFromGame = false;
    public bool ReturnedFromGame { get => returnedFromGame; set => returnedFromGame = value; }

    private Dictionary<NetworkConnectionToClient, string> playerDisplayNames = new();

    public Dictionary<NetworkConnectionToClient, string> PlayerDisplayNames { get => playerDisplayNames; set => playerDisplayNames = value; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}