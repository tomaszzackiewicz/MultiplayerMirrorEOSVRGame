using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerListItemPresenter : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text playerCountText;
    public Image iconImage;

    public void PopulateFromDispatcher(LobbyInfoDispatcher dispatcher)
    {
        nameText.text = dispatcher.Name.Value;
        playerCountText.text = $"{dispatcher.PlayerCount.Value} players";
        iconImage.sprite = dispatcher.Icon.Value;
    }
}