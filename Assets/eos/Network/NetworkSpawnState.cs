using Mirror;

public class NetworkSpawnState : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnOccupiedChanged))]
    public bool isOccupied;

    public bool IsOccupied => isOccupied;

    [Server]
    public void SetOccupied(bool value)
    {
        isOccupied = value;
    }

    private void OnOccupiedChanged(bool oldValue, bool newValue)
    {
        // Tu możesz np. zmienić kolor spawn pointa, pokazać ikonę itd.
    }
}