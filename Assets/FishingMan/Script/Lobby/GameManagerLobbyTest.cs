using Unity.Netcode;
using UnityEngine;
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
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var spawnPos = new Vector3(Random.Range(-4, 4), 0, Random.Range(-4, 4));
            var player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);

            playerIdText.text = client.ClientId.ToString();
        }
    }
}
