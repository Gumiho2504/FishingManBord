
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Boat : NetworkBehaviour
{
    [SerializeField] private string id;
    public float floatStrength = 0.5f; // Strength of the boat floating effect
    private float originalY;
    public Transform first, rob, food;
    [SerializeField] private Hook hook;
    public bool isFishing = false;
    public Vector3 targetPosition;
    public float moveSpeed = 1f;
    [SerializeField] private LineRenderer lr;
    [SerializeField] private Transform[] points;
    [SerializeField] public TextMesh text;

    public void SetId(string id)
    {
        this.id = id;

    }

    public SpriteRenderer foodSpriteRenderer;


    void Start()
    {

        var netObj = GetComponent<NetworkObject>();
        print("boat id " + netObj.IsOwner + " + local client id " + NetworkManager.Singleton.LocalClientId);
        originalY = transform.position.y;
        FishGameController.instance.OnStartFishing += OnStartFishing;
        FishGameController.instance.OnChangeFood += OnChangeFood;
        FishGameController.instance.OnUpHook += OnUpHook;
        FishGameController.instance.OnYRobPosChanged += OnYRobPosChanged;
        FishGameController.instance.OnPlayerStop += OnPlayerStop;

        hook.OnFishEatFood += OnFishEatFood;

    }

    private void OnPlayerStop(object sender, FishGameController.OnStopPlayerArg e)
    {
        print("play stop " + e.id);
        if (IsOwn(e.id))
        {
            Destroy(gameObject);
        }
    }

    private void OnFishEatFood()
    {


        FishGameController.instance.OnFishEatFoodClientRpc(id);
    }

    private Vector3 targetPos;
    private bool isMoving = false;

    private void OnYRobPosChanged(object sender, FishGameController.OnYRobPosChangedArg e)
    {
        if (!IsOwn(e.id)) return;
        if (isMoving) return;
        if (!isMoving)
        {
            targetPos = new Vector3(e.x, e.y, rob.transform.position.z);
            isMoving = true;
        }


        LeanTween.move(rob.gameObject, targetPos, 0.05f).setOnComplete(() =>
        {
            isMoving = false;
        });
        if (Vector3.Distance(rob.transform.position, targetPos) < 0.01f)
        {
            rob.transform.position = targetPos;
            isMoving = true;
            return;
        }
        //StopAllCoroutines();
        //StartCoroutine(MoveRobSmoothly(rob.gameObject, targetPos));

    }

    private IEnumerator MoveRobSmoothly(GameObject rob, Vector3 targetPosition)
    {
        Vector3 start = rob.transform.position;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            rob.transform.position = Vector3.Lerp(start, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rob.transform.position = targetPosition;
    }


    private void OnDisable()
    {
        FishGameController.instance.OnStartFishing -= OnStartFishing;
        FishGameController.instance.OnChangeFood -= OnChangeFood;
        FishGameController.instance.OnUpHook -= OnUpHook;
        FishGameController.instance.OnYRobPosChanged -= OnYRobPosChanged;
        FishGameController.instance.OnPlayerStop -= OnPlayerStop;
    }


    private void OnDestroy()
    {
        FishGameController.instance.OnStartFishing -= OnStartFishing;
        FishGameController.instance.OnChangeFood -= OnChangeFood;
        FishGameController.instance.OnUpHook -= OnUpHook;
        FishGameController.instance.OnYRobPosChanged -= OnYRobPosChanged;
        FishGameController.instance.OnPlayerStop -= OnPlayerStop;
    }


    private void OnUpHook(object sender, FishGameController.OnUpHookArgs e)
    {
        if (IsOwn(e.id))
        {
            MoveBack();
        }
    }

    private void OnChangeFood(object sender, FishGameController.OnChangeFoodArgs e)
    {
        print("change food " + e.id);
        if (IsOwn(e.id))
        {
            food.gameObject.SetActive(true);
            foodSpriteRenderer.sprite = e.sprite;
            hook.setFoodName(e.foodName);
        }

    }


    private bool IsOwn(string id)
    {
        return this.id == id;
    }

    private void OnStartFishing(object sender, FishGameController.OnStartFishingArgs e)
    {
        isMoving = false;
        print("start fishing + pos " + e.pos + " + player id " + NetworkManager.Singleton.LocalClientId);
        if (IsOwn(e.id))
        {
            hook.SetIsHooked(false);
            //MoveHook(e.pos);
        }
    }

    void Update()
    {
        //floating animation
        //transform.position = new Vector3(transform.position.x, originalY + Mathf.Sin(Time.time) * floatStrength, transform.position.z);


        // line
        for (int i = 0; i < points.Length; i++)
        {
            lr.SetPosition(i, points[i].position);
        }
    }



    void MoveHook(Vector3 position)
    {
        print("move hook " + position);

        LeanTween.move(rob.gameObject, position, moveSpeed);
    }


    private void MoveBack()
    {
        LeanTween.move(rob.gameObject, first.position, moveSpeed).setOnComplete(() =>
       {

           hook.ActiveFood();
           if (hook.transform.childCount > 0)
           {
               string foodName = hook.transform.GetChild(0).GetComponent<Fish>().food.foodName;
               FishGameController.instance.ReturnToStartClientRpc(id, foodName);
               Destroy(hook.transform.GetChild(0).gameObject);
               return;
           }
           FishGameController.instance.ReturnToStartClientRpc(id, "");

       });
    }



}
