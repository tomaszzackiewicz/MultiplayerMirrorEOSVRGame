using Mirror;
using Player;
using TMPro;
using UnityEngine;

public class RoundTimer : NetworkBehaviour
{
    public float timeBetweenRounds = 5f;

    [SyncVar(hook = nameof(OnTimerChanged))]
    private float timer;
    [SyncVar(hook = nameof(OnRoundChanged))]
    private int currentRound = 1;

    [SyncVar(hook = nameof(OnMapChanged))]
    private string currentMap;

    public int CurrentRound => currentRound;
    public string CurrentMap => currentMap;

    [SyncVar]
    private bool isCounting = false;

    private TextMeshProUGUI roundMessageText = null;
    private TextMeshProUGUI currentRoundText = null;
    private FpsNetworkManager netManager = null;

    public static RoundTimer Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        netManager = (FpsNetworkManager)NetworkManager.singleton;
    }

    private void Start()
    {
        roundMessageText = RoundOverView.Instance?.RoundOverTimerText;
        currentRoundText = RoundOverView.Instance?.CurrentRoundText;
    }

    private void Update()
    {
        if (!isClient || roundMessageText == null || !isCounting) return;

        roundMessageText.text = timer > 0
            ? $"<b>Round Over</b> - New Round in <color=#FFD700>{Mathf.Ceil(timer)}</color>"
            : "";
        if (currentRoundText)
        {
            currentRoundText.text = currentRound.ToString();
        }
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (!isCounting) return;

        timer -= Time.fixedDeltaTime;

        if (timer <= 0f)
        {
            timer = 0f;
            isCounting = false;

            RpcBeginNewRound();


            netManager.MapHandler.AdvanceRound();
        }
    }

    [ClientRpc]
    private void RpcBeginNewRound()
    {
        if (!NetworkClient.localPlayer) return;

        if (NetworkClient.localPlayer.TryGetComponent(out HealthBar hb))
        {
            RoundOverView.Instance?.ActivateRoundOverPanel(false);
            hb.BeginNewRound();
        }

        RoundOverView.Instance?.Hide();
    }

    [Server]
    public void StartNewRoundCountdown()
    {
        timer = timeBetweenRounds;
        isCounting = true;
    }

    private void OnTimerChanged(float oldVal, float newVal)
    {
        // Mo¿esz tu dodaæ dŸwiêk lub efekt
    }

    [ClientRpc]
    public void RpcShowResult(string message)
    {
        RoundOverView.Instance?.ShowResult(message);
    }

    [TargetRpc]
    public void TargetShowWin(NetworkConnection target)
    {
        RoundOverView.Instance?.ShowResult("You Won!");
    }

    [TargetRpc]
    public void TargetShowLoss(NetworkConnection target)
    {
        RoundOverView.Instance?.ShowResult("You Lost!");
    }

    [Server]
    public void EndRoundForAll(NetworkIdentity winner)
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity == null) continue;

            if (conn.identity == winner)
                TargetShowWin(conn);
            else
                TargetShowLoss(conn);
        }

        StartNewRoundCountdown();
    }

    private void OnRoundChanged(int oldVal, int newVal)
    {
        if (currentRoundText != null)
            currentRoundText.text = newVal.ToString();
    }

    private void OnMapChanged(string oldMap, string newMap)
    {
        Debug.Log($"Nowa mapa to: {newMap}");
    }

    public void SetRoundData(string map, int round)
    {
        if (!isServer) return;

        currentRound = round;
        currentMap = map;
    }
}