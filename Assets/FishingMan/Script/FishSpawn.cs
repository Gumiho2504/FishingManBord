using UnityEngine;
using System.Collections;
public class FishSpawn : MonoBehaviour
{
    public GameObject[] fishPrefab;      // Prefab to clone (the fish)
    public Transform[] spawnPoint;       // Start position of the fish
    public Transform endPoint;         // End position of the fish
    public float moveSpeed = 2f;       // Speed at which the fish will move
    public float minSpawnInterval = 0.5f; // Minimum time between spawning fish
    public float maxSpawnInterval = 2f;   // Maximum time between spawning fish
    public Vector3 minArea;
    public Vector3 maxArea;
    private float spawnTimer;

    private void Start()
    {
       
        spawnTimer = GetRandomSpawnInterval();
        StartCoroutine(SpawnFishOverTime());
    }

    private IEnumerator SpawnFishOverTime()
    {
        while (true)
        {
  yield return new WaitForSeconds(spawnTimer);

            
            SpawnAndMoveFish();

           
            spawnTimer = GetRandomSpawnInterval();
        }
    }

    
    private void SpawnAndMoveFish()
    {
        Transform starPos = spawnPoint[Random.Range(0,spawnPoint.Length)];
        GameObject clonedFish = Instantiate(fishPrefab[Random.Range(0, fishPrefab.Length)], starPos.position, starPos.rotation);

        clonedFish.LeanScale(Vector3.one * Random.Range(0.15f, 0.5f),0.2f);

        StartCoroutine(MoveFish(clonedFish));
    }

    private IEnumerator MoveFish(GameObject fish)
    {
        float x = Random.Range(minArea.x, maxArea.x);
        float y = Random.Range(minArea.y, maxArea.y);
        Vector3 areaPos = new Vector3(x, y, 0);
        Vector3 direction;

        if (areaPos.x > fish.transform.position.x) direction = areaPos - fish.transform.position;
        else
            direction = fish.transform.position - areaPos;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        print($"{angle}");

        fish.transform.rotation = Quaternion.Euler(0, 0, angle);
      


//move to area

        if (fish.transform.position.x > areaPos.x)
        {
            fish.GetComponent<SpriteRenderer>().flipX = true;
        }
        else {

            fish.GetComponent<SpriteRenderer>().flipX = false;
        }

        while (Vector3.Distance(fish.transform.position, areaPos ) > 0.1f && !fish.GetComponent<Fish>().isEat)
        {
            fish.transform.position = Vector3.MoveTowards(fish.transform.position, areaPos, moveSpeed * Time.deltaTime);
            yield return null;  
        }


        yield return new WaitForSeconds(Random.Range(1, 3));

//back


        Transform endPos = spawnPoint[Random.Range(0, spawnPoint.Length)];
        Vector3 d;
        if (endPos.position.x > fish.transform.position.x) d = endPos.position - fish.transform.position;
        else d = fish.transform.position - endPos.position;
        angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        fish.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (endPos.position.x > areaPos.x && fish.GetComponent<SpriteRenderer>().flipX == true)
        {
            fish.GetComponent<SpriteRenderer>().flipX = false;
        }
        else if(endPos.position.x < areaPos.x && fish.GetComponent<SpriteRenderer>().flipX == false)
        {
            fish.GetComponent<SpriteRenderer>().flipX = true;
        }


        while (Vector3.Distance(fish.transform.position, endPos.position) > 0.1f && !fish.GetComponent<Fish>().isEat)
        {
            fish.transform.position = Vector3.MoveTowards(fish.transform.position, endPos.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if(fish != null && !fish.GetComponent<Fish>().isEat)
        {
            Destroy(fish);
        }
       
    }

   
    private float GetRandomSpawnInterval()
    {
        return Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}
