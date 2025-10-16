using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBox : MonoBehaviour
{
    [SerializeField] private Text playerNameText;
    [SerializeField] private Button kickButton;
    private Player player;


    private void Awake()
    {
        kickButton.onClick.AddListener(OnKickPlayer);
    }





    public void SetPlayer(Player player)
    {
        this.player = player;
        playerNameText.text = player.Data["PlayerName"].Value;

    }


    public void SetKickButtonVisible(bool visible)
    {
        kickButton.gameObject.SetActive(visible);
    }



    private void OnKickPlayer()
    {
        if (player != null)
        {
            FishingManGame.LobbyManager.Instance.KickPlayer(player.Id);
        }
    }






}
