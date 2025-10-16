using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;
using System;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;





public class FishGameController : NetworkBehaviour
{

    private string localPlayerId;

    InputField inputField;
    public static FishGameController instance;

    public GameObject foodPanel, kto;
    public List<GameObject> foodButtons = new List<GameObject>();
    public GameObject boatPre, hookPre;
    public Transform firstPos, secondPos, catchPos;
    public GameObject qrCode;


    public Food[] foods;
    public Transform hook;
    public Transform firstHook;
    public float moveSpeed = 1f;
    public Button backButton;
    public Button fishingButton;
    public GameObject foodChangePanel, fishParent, networkUiPanel, loadingPanel;
    private int money = 10000;
    private int foodPrice = 0;
    public Text moneyText;

    // public Slider slider;

    private Vector3 targetPosition;
    private bool isFishing = false;
    private bool isReturning = true;


    public event EventHandler<OnStartFishingArgs> OnStartFishing;
    public event EventHandler<OnChangeFoodArgs> OnChangeFood;
    public event EventHandler<OnUpHookArgs> OnUpHook;

    public event EventHandler<OnYRobPosChangedArg> OnYRobPosChanged;

    public event EventHandler<OnStopPlayerArg> OnPlayerStop;
    float yPos = 0;
    float xPos = 0;
    private float robUpdateTimer = 0f;
    public class OnStopPlayerArg : EventArgs
    {
        public string id;

    }

    public class OnChangeFoodArgs : EventArgs
    {
        public string id;
        public Sprite sprite;
        public string foodName;
    }

    public class OnStartFishingArgs : EventArgs
    {
        public string id;
        public Vector3 pos;
    }


    public class OnUpHookArgs : EventArgs
    {
        public string id;

    }

    public class OnYRobPosChangedArg : EventArgs
    {
        public string id;
        public float y;
        public float x;
    }



    private void SetLoadingActive(bool isActive, string msg = "")
    {
        loadingPanel.SetActive(isActive);
        loadingPanel.GetComponentInChildren<Text>().text = msg;
    }



    void Awake()
    {
        instance = this;
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnServerClientDisconnect;
        }
    }


    void Start()
    {
        if (!SystemInfo.supportsGyroscope)
        {
            Debug.LogWarning("Gyroscope not supported on this device");
        }
        else
        {
            Input.gyro.enabled = true;
            Input.gyro.updateInterval = 0.2f; // Set update interval to 60 Hz

        }


        backButton.onClick.AddListener(ReturnToStartManual);
        fishingButton.onClick.AddListener(StartFishing);


    }


    void Update()
    {


        if (Input.GetMouseButton(0) && isFishing && !IsServer)
        {
            isFishing = false;
            isReturning = false;
            backButton.gameObject.SetActive(true);
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;



            money -= foodPrice;
            moneyText.text = $"Money = {money}$";

            StartFishingServerRpc(mousePosition.x, mousePosition.y, NetworkManager.Singleton.LocalClientId.ToString());

        }

        if (!IsServer && isReturning == false && !isFishing)
        {
            Vector3 tilt = Input.gyro.rotationRateUnbiased;
            // Use threshold on tilt directly
            if (Mathf.Abs(tilt.x) > 0.1f || Mathf.Abs(tilt.y) > 0.1f)
            {
                yPos = Math.Clamp(yPos + tilt.x * 0.1f, -4f, 4f);
                xPos = Math.Clamp(xPos + tilt.y * 0.1f, -3f, 3f);

                robUpdateTimer += Time.deltaTime;
                if (robUpdateTimer >= 0.1f)
                {
                    robUpdateTimer -= 0.1f; // more precise
                    OnChangeYRobPosServerRpc(localPlayerId, yPos, xPos);
                }
            }



            // Vector3 tilt = Input.gyro.rotationRateUnbiased;
            // if (Mathf.Abs(tilt.x - yPos) > 0.05f)
            // {
            //     yPos = Math.Clamp(yPos + tilt.x * 0.1f, -4f, 4f);
            //     xPos = Math.Clamp(xPos + tilt.y * 0.1f, -3f, 3f);
            //     StartCoroutine(OnRobChangePositionCoroutine());
            // }
        }



    }

    private bool isUpdatingRob = false;
    private IEnumerator OnRobChangePositionCoroutine()
    {
        if (isUpdatingRob) yield break;
        isUpdatingRob = true;
        yield return new WaitForSeconds(0.1f);
        OnChangeYRobPosServerRpc(localPlayerId, yPos, xPos);
        isUpdatingRob = false;
    }



    private Transform pos;
    public override void OnNetworkSpawn()
    {
        // print("client count " + NetworkManager.Singleton.ConnectedClientsList.Count);

        pos = NetworkManager.Singleton.LocalClientId == 1 ? firstPos : secondPos;

        //  print($"id:{NetworkManager.Singleton.LocalClientId} | isClient:{IsClient} | isServer:{IsServer} | isHost:{IsHost} | isOwner:{IsOwner} | isLocalPlayer:{IsLocalPlayer} | isOwnedByServer {IsOwnedByServer}");
        if (IsClient)
        {
            //SpawnPlayerServerRpc(pos.position.x, pos.position.y, pos.position.z, NetworkManager.Singleton.LocalClientId.ToString());
            fishingButton.gameObject.SetActive(IsClient);
            foodPanel.SetActive(IsClient);
            localPlayerId = NetworkManager.Singleton.LocalClientId.ToString();
            moneyText.gameObject.SetActive(true);

        }




        if (IsServer)
        {
            qrCode.SetActive(true);
        }



        if (NetworkManager.Singleton.ConnectedClientsList.Count > 1)
        {
            CloseQrServerRpc();
            InactiveLoadingPanelRpc();

        }
        else
        {
            SetLoadingActive(true, "Waiting for other player to join...");
        }



        NetworkManager.Singleton.OnClientDisconnectCallback += OnServerClientDisconnect;


        //  NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        // NetworkManager.Singleton.OnClientStopped += OnClientStopped;

        // print($"id:{NetworkManager.Singleton.LocalClientId} | isClient:{IsClient} | isServer:{IsServer} | isHost:{IsHost} | isOwner:{IsOwner} | isLocalPlayer:{IsLocalPlayer}");



    }

    [Rpc(SendTo.NotServer)]
    private void InactiveLoadingPanelRpc()
    {
        SpawnPlayerServerRpc(pos.position.x, pos.position.y, pos.position.z, localPlayerId);
        SetLoadingActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CloseQrServerRpc()
    {
        print("close qr");
        qrCode.SetActive(false);
    }

    // Server detects disconnect

    private void OnServerClientDisconnect(ulong clientId)
    {
        print("Server: Client disconnected: " + clientId);
        if (IsServer)
            OnPlayerStop?.Invoke(this, new OnStopPlayerArg { id = clientId.ToString() });

        if (IsClient && clientId.ToSafeString() == localPlayerId)
        {
            ResetSetup(false);
            backButton.gameObject.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void OnServerDisconnectServerRpc(string id)
    {
        print("Server: Client disconnected Rpc: " + id);
        OnPlayerStop?.Invoke(this, new OnStopPlayerArg { id = id });
    }

    // Optional: client cleanup
    private void OnClientStopped(bool obj)
    {
        print("Client stopped locally");
        // Local cleanup only, do NOT call ServerRpc here
    }


    [ServerRpc(RequireOwnership = false)]
    public void OnChangeYRobPosServerRpc(string id, float y, float x)
    {
        OnYRobPosChanged?.Invoke(this, new OnYRobPosChangedArg { id = id, y = y, x = x });
    }







    private int currentFood = 0;
    public void ChangeFoodClick(int i)
    {
        var button = EventSystem.current.currentSelectedGameObject;

        currentFood = i;
        for (int j = 0; j < foodButtons.Count; j++)
        {
            if (j != i)
                foodButtons[j].GetComponent<Image>().color = Color.white;
        }

        button.GetComponent<Image>().color = currentFood == i ? Color.yellow : Color.white;

        foodPrice = foods[i].price;
        ChangeFoodServerRpc(i, NetworkManager.Singleton.LocalClientId.ToString());
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeFoodServerRpc(int i, string id)
    {
        OnChangeFood?.Invoke(this, new OnChangeFoodArgs { id = id, sprite = foods[i].sprite, foodName = foods[i].foodName });

    }



    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(float x, float y, float z, string id)
    {
        var boat = Instantiate(boatPre, new Vector3(x, y, z), Quaternion.identity);
        var boatScript = boat.GetComponent<Boat>();
        boatScript.SetId(id);
        hook = boatScript.rob;
        boatScript.text.text = "id = " + id;
        firstHook = boatScript.first;

    }

    public void StartFishing()
    {
        ResetSetup(false);

    }

    private void ResetSetup(bool isActive)
    {
        isReturning = isActive;
        isFishing = !isActive;
        foodChangePanel.SetActive(isActive);
        fishingButton.gameObject.SetActive(isActive);

    }

    [ServerRpc(RequireOwnership = false)]
    private void StartFishingServerRpc(float x, float y, string id)
    {
        OnStartFishing?.Invoke(this, new OnStartFishingArgs { id = id, pos = new Vector3(x, y, 0) });
    }


    public void ReturnToStartManual()
    {
        isReturning = true;
        backButton.gameObject.SetActive(false);
        ReturnToStartServerRpc(localPlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReturnToStartServerRpc(string id)
    {
        OnUpHook?.Invoke(this, new OnUpHookArgs { id = id });
        //ReturnToStartClientRpc(id);
    }

    [ClientRpc]
    public void ReturnToStartClientRpc(string id, string name)
    {
        if (id == localPlayerId)
        {
            print($"id {id} | name {name}");
            //LeanTween.move(hook.gameObject, firstHook.position, moveSpeed);
            if (name == "")
            {
                ResetSetup(true);
                return;
            }

            Fish fish = FindFirstObjectByType<FishSpawn>().getFishByFoodName(name);

            if (fish)
            {
                var fishPre = Instantiate(fish.gameObject, fish.transform.position, Quaternion.identity);
                fishPre.transform.position = catchPos.position;
                LeanTween.scale(fishPre, Vector3.one * 1f, 0.3f).setEase(LeanTweenType.easeOutBack);

                money += fish.food.price;
                moneyText.text = $"Money = {money}$";
                StartCoroutine(TT(fishPre));
            }




        }


    }
    IEnumerator TT(GameObject fish)
    {

        yield return new WaitForSeconds(2f);
        kto.SetActive(true);
        kto.transform.LeanScale(Vector3.one, 0.5f).setFrom(Vector3.zero).setEaseSpring().setOnComplete(() =>
        {
            fish.LeanMove(kto.transform.position, 1f).setEaseOutBounce();
            fish.LeanScale(Vector3.zero, 1f).setEaseOutElastic().setOnComplete(() =>
            {
                Destroy(fish);

            });
        });

        yield return new WaitForSeconds(1.5f);

        kto.transform.LeanScale(Vector3.zero, 1f).setFrom(Vector3.one).setEaseSpring().setOnComplete(() =>
        {
            kto.SetActive(false);
        });
        ResetSetup(true);

    }

    [ClientRpc]
    public void OnFishEatFoodClientRpc(string id)
    {
        if (id == localPlayerId)
        {
            isReturning = true;
            print("fish eat food");
        }


    }


    public void Leave()
    {
        NetworkManager.Singleton.Shutdown();
        networkUiPanel.SetActive(true);
    }

    

}

