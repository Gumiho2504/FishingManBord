using System;
using Unity.Netcode;
using UnityEngine;

public class Hook : NetworkBehaviour
{
    //  public FishGameController fishGameController;
    [SerializeField] private string foodName;
    private bool isHooked = false;
    [SerializeField] private GameObject food;
    public event Action OnFishEatFood;

    public void SetIsHooked(bool isHooked)
    {
        this.isHooked = isHooked;
    }



    public void setFoodName(string foodName)
    {
        this.foodName = foodName;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        //print("onTrigger enter " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("fish"))
        {
            // print("OnTriggerFish");
            Fish fish = collision.gameObject.GetComponent<Fish>();
            if (fish.food.foodName == foodName && !isHooked)
            {

                food.gameObject.SetActive(false);

                //print("OnTriggerFishMatchFood");
                fish.isEat = true;

                try
                {
                    fish.transform.SetParent(transform);
                    OnFishEatFood?.Invoke();
                    isHooked = true;
                }
                catch (System.Exception e)
                {
                    print(e);
                }

            }


        }
    }


    [ClientRpc(RequireOwnership = false)]
    private void NotifyCatchClientRpc()
    {
        Debug.Log($"You caught");

    }

}
