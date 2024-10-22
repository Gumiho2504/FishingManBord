using UnityEngine;

public class Boat : MonoBehaviour
{
    public float floatStrength = 0.5f; // Strength of the boat floating effect
    private float originalY;

    void Start()
    {
        originalY = transform.position.y;
    }

    void Update()
    {
        // Apply a floating effect on the boat
        transform.position = new Vector3(transform.position.x, originalY + Mathf.Sin(Time.time) * floatStrength, transform.position.z);
    }
}
