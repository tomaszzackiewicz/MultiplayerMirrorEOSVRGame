using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public static class SceneContext
{
    private static HashSet<string> gameplayScenes;
    private static string lobbyScene;
    private static string endScene;

    public static void Initialize(MapSet mapSet)
    {
        gameplayScenes = new HashSet<string>(
            mapSet.Maps.Select(scenePath => System.IO.Path.GetFileNameWithoutExtension(scenePath))
        );

        lobbyScene = System.IO.Path.GetFileNameWithoutExtension(mapSet.LobbyScene);
        endScene = System.IO.Path.GetFileNameWithoutExtension(mapSet.FinalScene);

        //Debug.Log("➡️ Zainicjalizowano sceny:");
        //Debug.Log($"Lobby: {lobbyScene}");
        //Debug.Log($"End: {endScene}");
        //foreach (var scene in gameplayScenes)
        //    Debug.Log($"Gameplay: {scene}");
    }

    public static bool IsGameplayScene()
        => gameplayScenes.Contains(CurrentSceneName());

    public static bool IsLobbyScene() => CurrentSceneName() == lobbyScene;

    public static bool IsEndScene()
        => CurrentSceneName() == endScene;

    public static string CurrentSceneName()
        => SceneManager.GetActiveScene().name;

    public static bool IsGameplayScene(string sceneName)
    => gameplayScenes.Contains(sceneName);

    public static bool IsLobbyScene(string sceneName)
        => sceneName == lobbyScene;

    public static bool IsEndScene(string sceneName)
        => sceneName == endScene;

}