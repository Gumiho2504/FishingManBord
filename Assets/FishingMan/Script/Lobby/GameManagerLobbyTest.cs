using Unity.Netcode;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerLobbyTest : NetworkBehaviour
{
    public GameObject playerPrefab;
    public Text playerIdText;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnPlayers();
        }





    }

    private void SpawnPlayers()
    {
        print("spawn players : " + NetworkManager.Singleton.ConnectedClientsList.Count);
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {

            var spawnPos = new Vector3(Random.Range(-4, 4), Random.Range(-4, 4), 0);
            var player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);

            SetPlayerIdRpc(client.ClientId);

            //SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByName("test"));

        }

        //SceneManager.UnloadScene(SceneManager.GetSceneByName("room"));

    }


    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerIdRpc(ulong playerId)
    {
        playerIdText.text = playerId.ToString();
    }

    // [Rpc(SendTo.ClientsAndHost)]
    // private void MovePlayerToSceneRpc(GameObject player)
    // {

    // }
}
