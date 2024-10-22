using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class FishGameController : MonoBehaviour
{
    int money = 1000;
    int foodPrice = 0;
    public SpriteRenderer foodSpriteRenderer;
    public Food hookFood;
    
    public Food[] foods;
    public Transform hook;        
    public Transform firstHook;   
    public float moveSpeed = 1f;  
    public Button backButton;
    public Button fisingButton;
    public GameObject foodChangePanel,fishParent;
    public Text moneyText;

    private Vector3 targetPosition; 
    private bool isFishing = false; 
    private bool isReturning = false;


    public void ChangeFood(int i) {
        hookFood = foods[i];
        foodSpriteRenderer.sprite = foods[i].sprite;
        
        foodPrice = foods[i].price;
        

    }

   


    void Start()
    {
     
        hook.localPosition = firstHook.localPosition;
        hookFood = foods[0];
       
        backButton.onClick.AddListener(ReturnToStartManual);
        fisingButton.onClick.AddListener(StartFishing);
        moneyText.text = $"Money = {money}$";
    }



    void Update()
    {
        
        if (Input.GetMouseButton(0) && isFishing)
        {
            isFishing = false;
            backButton.gameObject.SetActive(true);
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

          
            targetPosition = new Vector3(mousePosition.x, mousePosition.y, 0);

          
            MoveHook();
        }
    }

    
    void MoveHook()
    {
       
        LeanTween.move(hook.gameObject, targetPosition, moveSpeed);
    }

    
    IEnumerator ReturnToStart()
    {
        
        LeanTween.move(hook.gameObject, firstHook.position, moveSpeed).setOnComplete(() =>
        {
           
            isReturning = false;
           
        });

        yield return isReturning;
    }

   
    public void StartFishing()
    {
        isFishing = true;  
        isReturning = false;
        money -= foodPrice;
        foodChangePanel.SetActive(false);
        fisingButton.gameObject.SetActive(false);
        moneyText.text = $"Money = {money}$";
    }

  
    public void ReturnToStartManual()
    {
        isReturning = true;
        backButton.gameObject.SetActive(false);
       
        StartCoroutine(TT());
    }


    IEnumerator TT()
    {
        yield return ReturnToStart();
        yield return new WaitForSeconds(2f);

        for(int i = 0; i < fishParent.transform.childCount; i++)
        {
            Fish fish = fishParent.transform.GetChild(i).GetComponent<Fish>();
            money += fish.food.price;
            moneyText.text = $"Money = {money}$";
            yield return new WaitForSeconds(0.3f);
            Destroy(fish.gameObject);
        }

        yield return new WaitForSeconds(1f);
        foodChangePanel.SetActive(true);
        fisingButton.gameObject.SetActive(true);

    }
}
