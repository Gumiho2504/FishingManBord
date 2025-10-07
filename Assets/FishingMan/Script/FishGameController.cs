using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;
using System;
using Unity.VisualScripting;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;



public class FishGameController : NetworkBehaviour
{

    private string localPlayerId;


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
    public GameObject foodChangePanel, fishParent;
    private int money = 10000;
    private int foodPrice = 0;
    public Text moneyText;

    public Slider slider;

    private Vector3 targetPosition;
    private bool isFishing = false;
    private bool isReturning = false;


    public event EventHandler<OnStartFishingArgs> OnStartFishing;
    public event EventHandler<OnChangeFoodArgs> OnChangeFood;
    public event EventHandler<OnUpHookArgs> OnUpHook;
    public event EventHandler<OnCalculateFishArg> OnCalculateFish;

    public event EventHandler<OnYRobPosChangedArg> OnYRobPosChanged;

    public event EventHandler<OnStopPlayerArg> OnPlayerStop;
    float yPos = 0;
    public class OnStopPlayerArg : EventArgs
    {
        public string id;

    }
    public class OnCalculateFishArg : EventArgs
    {
        public string id;
        public Fish fish;
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


        backButton.onClick.AddListener(ReturnToStartManual);
        fishingButton.onClick.AddListener(StartFishing);
        slider.onValueChanged.AddListener((value) =>
        {
            OnChangeYRobPosServerRpc(localPlayerId, value);
        });

    }







    public override void OnNetworkSpawn()
    {

        var pos = NetworkManager.Singleton.LocalClientId == 1 ? firstPos : secondPos;

        //  print($"id:{NetworkManager.Singleton.LocalClientId} | isClient:{IsClient} | isServer:{IsServer} | isHost:{IsHost} | isOwner:{IsOwner} | isLocalPlayer:{IsLocalPlayer} | isOwnedByServer {IsOwnedByServer}");
        if (IsClient)
        {
            SpawnPlayerServerRpc(pos.position.x, pos.position.y, pos.position.z, NetworkManager.Singleton.LocalClientId.ToString());
            fishingButton.gameObject.SetActive(IsClient);
            foodPanel.SetActive(IsClient);
            localPlayerId = NetworkManager.Singleton.LocalClientId.ToString();


            moneyText.gameObject.SetActive(true);


            NetworkManager.Singleton.OnClientDisconnectCallback += OnServerClientDisconnect;


        }


        if (IsServer)
        {

            qrCode.SetActive(true);
        }



        if (NetworkManager.Singleton.ConnectedClientsList.Count > 1)
        {
            if (IsServer)
            {
                qrCode.SetActive(false);
            }

        }


        //  NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        // NetworkManager.Singleton.OnClientStopped += OnClientStopped;

        // print($"id:{NetworkManager.Singleton.LocalClientId} | isClient:{IsClient} | isServer:{IsServer} | isHost:{IsHost} | isOwner:{IsOwner} | isLocalPlayer:{IsLocalPlayer}");



    }

    // Server detects disconnect

    private void OnServerClientDisconnect(ulong clientId)
    {
        print("Server: Client disconnected: " + clientId);
        // string playerId = GetPlayerIdFromClientId(clientId); // map clientId → playerId
        OnServerDisconnectServerRpc(localPlayerId);
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
    public void OnChangeYRobPosServerRpc(string id, float y)
    {
        OnYRobPosChanged?.Invoke(this, new OnYRobPosChangedArg { id = id, y = y });
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








    void Update()
    {

        if (Input.GetMouseButton(0) && isFishing && !IsServer)
        {
            isFishing = false;
            backButton.gameObject.SetActive(true);
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            slider.gameObject.SetActive(true);
            slider.value = mousePosition.y;

            money -= foodPrice;
            moneyText.text = $"Money = {money}$";
            //var hook = Instantiate(hookPre, mousePosition, Quaternion.identity);
            //targetPosition = new Vector3(mousePosition.x, mousePosition.y, 0);
            StartFishingServerRpc(mousePosition.x, mousePosition.y, NetworkManager.Singleton.LocalClientId.ToString());

        }



    }



    // [ServerRpc(RequireOwnership = false)]
    // void HookFishServerRpc()
    // {
    //     MoveHook();
    // }


    // void MoveHook()
    // {

    //     LeanTween.move(hook.gameObject, targetPosition, moveSpeed);
    // }






    public void StartFishing()
    {
        ResetSetup(false);

    }

    private void ResetSetup(bool isActive)
    {
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
            Fish fish = FindFirstObjectByType<FishSpawn>().getFishByFoodName(name);
            var fishPre = new GameObject();
            if (fish)
            {
                fishPre = Instantiate(fish.gameObject, fish.transform.position, Quaternion.identity);
                fishPre.transform.position = catchPos.position;
                LeanTween.scale(fishPre, Vector3.one * 1f, 0.3f).setEase(LeanTweenType.easeOutBack);

                money += fish.food.price;
                moneyText.text = $"Money = {money}$";

            }

            StartCoroutine(TT(fishPre));
        }


    }
    IEnumerator TT(GameObject fish)
    {
        //yield return ReturnToStart();

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
        // for (int i = 0; i < fishParent.transform.childCount; i++)
        // {
        //     Fish fish = fishParent.transform.GetChild(i).GetComponent<Fish>();
        //     // money += fish.food.price;
        //     // moneyText.text = $"Money = {money}$";
        //     yield return new WaitForSeconds(0.3f);
        //     Destroy(fish.gameObject);
        // }

        //yield return new WaitForSeconds(1f);
        //foodChangePanel.SetActive(true);
        //fishingButton.gameObject.SetActive(true);

    }

    [ClientRpc]
    public void OnFishEatFoodClientRpc(string id)
    {
        if (id == localPlayerId)
        {
            slider.gameObject.SetActive(false);
            print("fish eat food");
        }


    }


    IEnumerator ReturnToStart()
    {

        LeanTween.move(hook.gameObject, firstHook.position, moveSpeed).setOnComplete(() =>
        {

            isReturning = false;

        });

        yield return isReturning;
    }


}

