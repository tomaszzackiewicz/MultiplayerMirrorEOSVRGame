using Mirror;
using UnityEngine;

public class CustomNetworkStartPosition : NetworkStartPosition
{
    [SerializeField] private RoleType roleType = RoleType.None;

    public RoleType RoleType => roleType;
}