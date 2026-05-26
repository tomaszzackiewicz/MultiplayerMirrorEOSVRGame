using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;

    public void SetText(string text)
    {
        if (nameText != null)
            nameText.text = text;
    }
}