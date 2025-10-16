using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBox : MonoBehaviour
{
    [SerializeField] private Text lobbyNameText;
    [SerializeField] private Text playersText;
    [SerializeField] private Button joinButton;
    private Lobby lobby;

    private void Awake()
    {
        joinButton.onClick.AddListener(() =>
        {
            FishingManGame.LobbyManager.Instance.JoinLobby(lobby);
        });
    }

    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
        playersText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
    }
}
