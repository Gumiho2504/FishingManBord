using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private InputField joinCodeInput;
    [SerializeField] private GameObject panel;
    private string joinCode = "";
    private void Start()
    {
        serverButton.onClick.AddListener(OnServerButtonClicked);
        hostButton.onClick.AddListener(OnHostButtonClicked);
        clientButton.onClick.AddListener(OnClientButtonClicked);
        joinCodeInput.onValueChanged.AddListener((value) => { joinCode = value; });
    }


    private void OnServerButtonClicked()
    {
        //NetworkManager.Singleton.StartServer();
        TransportController.instance.StartSever();
        Hide();
    }
    private void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
        Hide();
    }
    private void OnClientButtonClicked()
    {
        if (string.IsNullOrEmpty(joinCode)) return;
        TransportController.instance.StartClient(joinCode);
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        panel.SetActive(false);
    }


}