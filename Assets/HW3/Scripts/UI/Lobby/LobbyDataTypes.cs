namespace HW3.Scripts
{
    public enum LobbyState
    {
        NotConnected,
        ConnectingToLobby,
        InLobby,
        ConnectingToSession,
        CreatingNcp,
        InSession
    }

    public class LobbyManagerUtils
    {
        public static string GetLobbyName(int option) => $"lobby{option}";
    }
}
