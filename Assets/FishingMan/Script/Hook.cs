using UnityEngine;

public class Hook : MonoBehaviour
{
    public FishGameController fishGameController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("fish"))
        {
            Fish fish = collision.gameObject.GetComponent<Fish>();
            if(fish.food.foodName == fishGameController.hookFood.foodName) {
                fish.isEat = true;
                fish.transform.SetParent(gameObject.transform);
            }

           
        }
    }
   
}
