using Mirror;

public struct ChatMessage : NetworkMessage
{
    public string sender;
    public string text;
    public bool isPrivate;
    public string recipient; // tylko jeœli isPrivate == true
}