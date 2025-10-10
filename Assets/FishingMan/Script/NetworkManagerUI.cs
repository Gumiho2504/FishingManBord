using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;
    private void Start()
    {
        serverButton.onClick.AddListener(OnServerButtonClicked);
        hostButton.onClick.AddListener(OnHostButtonClicked);
        clientButton.onClick.AddListener(OnClientButtonClicked);
    }


    private void OnServerButtonClicked()
    {
        NetworkManager.Singleton.StartServer();
        Hide();
    }
    private void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
        Hide();
    }
    private void OnClientButtonClicked()
    {
        NetworkManager.Singleton.StartClient();
        Hide();
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

   
}