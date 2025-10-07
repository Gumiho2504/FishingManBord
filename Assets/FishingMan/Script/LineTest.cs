using UnityEngine;

public class LineTest : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    [SerializeField] private LineController line;
    void Start()
    {
        //print(gameObject.name);

    }


    public void SetUpLinePoints(Transform[] points)
    {

       // line.SetUpLine(points);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
