using UnityEngine;

[CreateAssetMenu(fileName = "ServerIconLibrary", menuName = "Settings/Server Icon Library")]
public class ServerIconLibrary : ScriptableObject
{
    public Sprite[] icons;

    public Sprite GetIcon(int index)
    {
        if (index >= 0 && index < icons.Length)
            return icons[index];
        return null;
    }
}