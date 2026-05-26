using TMPro;
using UnityEngine;

public class RoundOverView : MonoBehaviour
{
    [SerializeField] private GameObject roundOverCanvas = null;
    [SerializeField] private TextMeshProUGUI winLoseText = null;
    [SerializeField] private TextMeshProUGUI roundOverTimerText = null;
    [SerializeField] private TextMeshProUGUI currentRoundText = null;

    public TextMeshProUGUI RoundOverTimerText => roundOverTimerText;
    public TextMeshProUGUI CurrentRoundText => currentRoundText;
    public static RoundOverView Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        roundOverCanvas.SetActive(false);
    }

    public void ShowResult(string message)
    {
        winLoseText.text = message;
        roundOverCanvas.SetActive(true);
    }

    public void Hide()
    {
        roundOverCanvas.SetActive(false);
    }

    public void ActivateRoundOverPanel(bool active)
    {
        roundOverCanvas.SetActive(active);
    }
}