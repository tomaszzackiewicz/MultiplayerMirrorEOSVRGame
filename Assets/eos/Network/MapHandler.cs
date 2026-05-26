using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MapHandler
{
    private readonly IReadOnlyList<string> maps;
    private readonly int roundsPerMap;
    private readonly bool loopMaps;
    private readonly MapSet mapSet;
    private List<string> shuffledMaps;
    private int roundsOnCurrentMap;

    public string CurrentMap { get; private set; }
    public int CurrentRound => roundsOnCurrentMap;
    public bool ReadyForMapChange => roundsOnCurrentMap >= roundsPerMap;
    public bool NoMoreMaps => shuffledMaps.Count == 0;
    public string LobbyScene => mapSet.LobbyScene;
    public string FinalScene => mapSet.FinalScene;

    public event Action<string> OnMapChanged;
    public event Action<string, int> OnRoundAdvanced;
    public event Action OnNoMoreMaps;

    public MapHandler(MapSet set, int roundsPerMap, bool loopMaps)
    {
        mapSet = set;
        maps = set.Maps;
        this.roundsPerMap = roundsPerMap;
        this.loopMaps = loopMaps;

        ResetMapPool();
        LoadNextMap(); // tu ustawiamy pierwszą mapę i rundę 1
    }

    public void AdvanceRound()
    {
        if (string.IsNullOrEmpty(CurrentMap)) return;

        if (roundsOnCurrentMap >= roundsPerMap)
        {
            LoadNextMap();
            return;
        }

        roundsOnCurrentMap++;
        OnRoundAdvanced?.Invoke(CurrentMap, roundsOnCurrentMap);
    }

    private void LoadNextMap()
    {
        if (shuffledMaps.Count == 0)
        {
            if (loopMaps)
            {
                ResetMapPool();
            }
            else
            {
                CurrentMap = null;
                OnNoMoreMaps?.Invoke();
                return;
            }
        }

        string mapPath = shuffledMaps[UnityEngine.Random.Range(0, shuffledMaps.Count)];
        shuffledMaps.Remove(mapPath);
        CurrentMap = ExtractSceneName(mapPath);

        roundsOnCurrentMap = 0;
        AdvanceRound();

        OnMapChanged?.Invoke(CurrentMap);
    }

    private string ExtractSceneName(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    private void ResetMapPool()
    {
        shuffledMaps = maps.ToList();
    }
}