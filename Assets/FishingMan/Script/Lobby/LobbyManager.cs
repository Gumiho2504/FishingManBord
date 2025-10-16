using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

namespace FishingManGame
{
    public enum GameMode
    {
        None, OneVsOne
    }

    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }
        private const string KEY_PLAYER_NAME = "PlayerName";
        private const string KEY_GAME_MODE = "GameMode";
        private const string KEY_LOBBY_NAME = "LobbyName";
        private const string KEY_RELAY_CODE = "RelayCode";
        private const string KEY_START_GAME = "StartGame";
        private string playerName;
        private string lobbyName;
        private int maxPlayers;
        private GameMode gameMode;
        private string lobbyCode;


        [Header("UI")]
        [Header("Prefab")]
        [Tooltip("Lobby Box Prefab")]
        [SerializeField] private GameObject lobbyBox;
        [SerializeField] private GameObject playerBox;
        [Header("Text UI")]
        [SerializeField] private Text playerNameText;
        [Header("Input Field")]
        [SerializeField] private InputField playerNameInputField;
        [SerializeField] private InputField lobbyNameInputField;
        [SerializeField] private InputField maxPlayersInputField;
        [SerializeField] private InputField gameModeInputField;
        [SerializeField] private InputField lobbyCodeInputField;
        [Header("Panel")]
        [SerializeField] private GameObject nameInputPanel;
        [SerializeField] private GameObject lobbyCreatePanel;
        [SerializeField] private GameObject lobbyRoomPanel;
        [SerializeField] private GameObject loadingPanel;
        [Header("Parent GameObject")]

        [SerializeField] private GameObject lobbyListParent;
        [SerializeField] private GameObject playerListParent;
        [Header("Button")]
        [SerializeField] private Button doneButton;
        [SerializeField] private Button createLobbyButton;

        [SerializeField] private Button addLobbyButton;
        [SerializeField] private Button refreshLobbyListButton;

        private Lobby m_joinLobby { get; set; }
        public Lobby CurrentLobby => m_joinLobby;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }





            // input field listeners
            playerNameInputField.onEndEdit.AddListener((string name) => { playerName = name; });
            lobbyNameInputField.onEndEdit.AddListener((string name) => { lobbyName = name; });
            //maxPlayersInputField.onEndEdit.AddListener((string max) => { int.TryParse(max, out maxPlayers); });
            // gameModeInputField.onEndEdit.AddListener((string mode) =>
            // {
            //     if (mode == "OneVsOne")
            //         gameMode = GameMode.OneVsOne;
            //     else
            //         gameMode = GameMode.None;
            // });
            // lobbyCodeInputField.onEndEdit.AddListener((string code) => { lobbyCode = code; });

            // button listeners
            doneButton.onClick.AddListener(OnDoneButtonClicked);
            createLobbyButton.onClick.AddListener(() => { CreateLobby(lobbyName); });
            addLobbyButton.onClick.AddListener(OnAddLobbyClicked);
            refreshLobbyListButton.onClick.AddListener(ListLobbies);
        }

        private async void Start()
        {
            if (!PlayerPrefs.HasKey(KEY_PLAYER_NAME))
            {
                nameInputPanel.SetActive(true);

            }
            else
            {
                playerName = PlayerPrefs.GetString(KEY_PLAYER_NAME);
                playerNameText.text = playerName;
                Initialized();
            }

        }


        private void Update()
        {
            LobbyPoll();
        }



        private async void Initialized()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized) return;

            ShowLoadingPanel(true, "Initializing...");
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
           {
               Debug.Log("Signed in : " + AuthenticationService.Instance.PlayerId);
           };
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            ShowLoadingPanel(false);
            nameInputPanel.SetActive(false);
            playerNameText.text = playerName;
            ListLobbies();
        }




        private async void CreateLobby(string lobbyName)
        {
            try
            {
                ShowLoadingPanel(true, "Creating Lobby...");
                if (lobbyName == null && lobbyName == "")
                {
                    return;
                }
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3); // max 3 clients
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                Debug.Log("Relay join code: " + joinCode);
                var options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Player = GetPlayer(),
                    Data = new Dictionary<string, DataObject>()
                {
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, GameMode.OneVsOne.ToString()) },
                    { KEY_LOBBY_NAME, new DataObject(DataObject.VisibilityOptions.Public, lobbyName) },
                    { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "False") },
                    { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
                };
                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2, options);
                m_joinLobby = lobby;


                // Configure Unity Transport
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                var serverData = AllocationUtils.ToRelayServerData(allocation, "wss");
                transport.SetRelayServerData(serverData);

                Debug.Log("Created lobby : " + lobby.Name + ",Mode" + lobby.Data["GameMode"].Value + ",Code" + lobby.LobbyCode);
                ShowLoadingPanel(false);
                lobbyCreatePanel.SetActive(false);
                lobbyRoomPanel.SetActive(true);
                ListPlayers(m_joinLobby);

                NetworkManager.Singleton.StartHost();

                StartCoroutine(HeartbeatLobbyCoroutine(m_joinLobby, 15));
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }


        }

        public async void JoinLobby(Lobby lobby)
        {
            try
            {

                ShowLoadingPanel(true, "Joining Lobby...");
                var Player = GetPlayer();
                var options = new JoinLobbyByIdOptions
                {
                    Player = Player
                };
                m_joinLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, options);
                string joinCode = m_joinLobby.Data[KEY_RELAY_CODE].Value;
                Debug.Log("Relay join code: " + joinCode);

                StartCoroutine(HeartbeatLobbyCoroutine(m_joinLobby, 15));
                ShowLoadingPanel(false);
                lobbyRoomPanel.SetActive(true);

                ListPlayers(m_joinLobby);
                StartCoroutine(HeartbeatLobbyCoroutine(m_joinLobby, 15));
                JoinAllocation(joinCode);

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private async void JoinAllocation(string joinCode)
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var clientData = AllocationUtils.ToRelayServerData(allocation, "wss");
            transport.SetRelayServerData(clientData);

            NetworkManager.Singleton.StartClient();
        }


        private void ListPlayers(Lobby lobby)
        {
            foreach (var player in lobby.Players)
            {
                //Debug.Log("Player : " + player.Id + ", Name : " + player.Data[KEY_PLAYER_NAME].Value);
                var playerBox = InstantiateToParent(this.playerBox, playerListParent.transform);
                playerBox.GetComponent<PlayerBox>().SetPlayer(player);
                playerBox.GetComponent<PlayerBox>().SetKickButtonVisible(IsLobbyHost() && player.Id != AuthenticationService.Instance.PlayerId);
            }
        }



        private async void ListLobbies()
        {
            try
            {
                ShowLoadingPanel(true, "Loading Lobbies...");
                var options = new QueryLobbiesOptions();
                options.Count = 5;
                options.Filters = new List<QueryFilter>()
                                    {
                                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                                    };
                options.Order = new List<QueryOrder>()
            {
                new QueryOrder(false, QueryOrder.FieldOptions.Created)
            };
                var res = await LobbyService.Instance.QueryLobbiesAsync(options);
                Debug.Log("Total lobbies : " + res.Results.Count);
                foreach (var lobby in res.Results)
                {
                    var lobbyBox = InstantiateToParent(this.lobbyBox, lobbyListParent.transform);
                    lobbyBox.GetComponent<LobbyBox>().SetLobby(lobby);
                    Debug.Log("Lobby : " + lobby.Name + ", Mode : " + lobby.Data["GameMode"].Value + ", Players : " + lobby.Players.Count + "/" + lobby.MaxPlayers + ", Code : " + lobby.LobbyCode);
                }
                ShowLoadingPanel(false);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

        }



        private Player GetPlayer()
        {
            return new Player(AuthenticationService.Instance.PlayerId, null,
            new Dictionary<string, PlayerDataObject>()
            {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
            });

        }


        private void OnAddLobbyClicked()
        {
            lobbyCreatePanel.SetActive(true);
        }

        private void OnDoneButtonClicked()
        {
            if (playerName != null && playerName != "")
            {
                PlayerPrefs.SetString(KEY_PLAYER_NAME, playerName);
                var initializeOptions = new InitializationOptions();
                initializeOptions.SetProfile(playerName);
                Initialized();
            }
        }


        private bool IsLobbyHost()
        {
            return m_joinLobby != null && m_joinLobby.HostId == AuthenticationService.Instance.PlayerId;
        }


        IEnumerator HeartbeatLobbyCoroutine(Lobby lobby, float waitTimeSeconds)
        {
            if (!IsLobbyHost())
            {
                yield break;
            }
            var delay = new WaitForSecondsRealtime(waitTimeSeconds);

            while (true)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
                yield return delay;
            }
        }


        private float lobbyPollTimer;
        private async void LobbyPoll()
        {
            if (m_joinLobby == null || m_joinLobby.Data[KEY_START_GAME].Value == "True") return;
            if (m_joinLobby != null)
            {
                lobbyPollTimer -= Time.deltaTime;
                if (lobbyPollTimer <= 0f)
                {
                    lobbyPollTimer = 3f;
                    m_joinLobby = await LobbyService.Instance.GetLobbyAsync(m_joinLobby.Id);
                    ClearPlayerList();
                    ListPlayers(m_joinLobby);
                    if (m_joinLobby.Players.Count == m_joinLobby.MaxPlayers && IsLobbyHost())
                    {
                        // start game
                        var options = new UpdateLobbyOptions
                        {
                            Data = new Dictionary<string, DataObject>()
                            {
                                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "True") }
                            }
                        };
                        m_joinLobby = await LobbyService.Instance.UpdateLobbyAsync(m_joinLobby.Id, options);
                        Debug.Log("All players joined, starting game...");
                        // load game scene
                        NetworkManager.Singleton.SceneManager.LoadScene("test", UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    }

                }
            }


        }

        private void ShowLoadingPanel(bool isActive, string msg = "")
        {
            loadingPanel.SetActive(isActive);
            if (msg != "")
            {
                loadingPanel.GetComponentInChildren<Text>().text = msg;
            }
        }



        public async void KickPlayer(string playerId)
        {
            try
            {
                if (!IsLobbyHost())
                {
                    Debug.Log("Only host can kick player");
                    return;
                }
                ShowLoadingPanel(true, "Kicking Player...");
                await LobbyService.Instance.RemovePlayerAsync(m_joinLobby.Id, playerId);
                ShowLoadingPanel(false);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }


        private void ClearLobbyList()
        {
            foreach (Transform child in lobbyListParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
        private void ClearPlayerList()
        {
            foreach (Transform child in playerListParent.transform)
            {
                Destroy(child.gameObject);
            }
        }


        private GameObject InstantiateToParent(GameObject prefab, Transform parent)
        {
            var newPrefab = Instantiate(prefab, transform);
            newPrefab.transform.SetParent(parent, false);
            return newPrefab;
        }
    }// end of class



}/// end of namespace

