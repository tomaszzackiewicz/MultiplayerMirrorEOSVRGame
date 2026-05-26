using UnityEngine;

public enum RoleType { None = 0, Server = 1, Client = 2 }

public static class Consts
{

    //Tags
    public const string playerTag = "Player";

    //Scenes
    public const string mainMenuScene = "MainMenuScene";
    public const string lobbyScene = "LobbyScene";
    public const string gameScene = "GameScene";
    public const string gameScene2 = "GameScene2";

    //Server
    public const string localhost = "localhost";

    private static WaitForSeconds wait01 = new WaitForSeconds(0.1f);
    private static WaitForSeconds wait02 = new WaitForSeconds(0.2f);
    private static WaitForSeconds wait05 = new WaitForSeconds(0.5f);
    private static WaitForSeconds wait1 = new WaitForSeconds(1.0f);
    private static WaitForSeconds wait2 = new WaitForSeconds(2.0f);
    private static WaitForSeconds wait3 = new WaitForSeconds(3.0f);
    private static WaitForSeconds wait4 = new WaitForSeconds(4.0f);
    private static WaitForSeconds wait5 = new WaitForSeconds(5.0f);
    private static WaitForSeconds wait10 = new WaitForSeconds(10.0f);

    public static WaitForSeconds Wait01 => wait01;
    public static WaitForSeconds Wait02 => wait02;
    public static WaitForSeconds Wait05 => wait05;
    public static WaitForSeconds Wait1 => wait1;
    public static WaitForSeconds Wait2 => wait2;
    public static WaitForSeconds Wait3 => wait3;
    public static WaitForSeconds Wait4 => wait4;
    public static WaitForSeconds Wait5 => wait5;
    public static WaitForSeconds Wait10 => wait10;
}
