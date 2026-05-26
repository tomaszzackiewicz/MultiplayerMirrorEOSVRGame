using Mirror;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapSet", menuName = "Settings/Map Set")]
public class MapSet : ScriptableObject
{
    [Scene]
    [SerializeField] private List<string> maps = new();

    [Scene]
    [SerializeField] private string endScene;

    [Scene]
    [SerializeField] private string lobbyScene;

    public IReadOnlyList<string> Maps => maps.AsReadOnly();
    public string FinalScene => endScene;
    public string LobbyScene => lobbyScene;
}