using UnityEngine;
using NaughtyAttributes;
public class Rob2DCreator : MonoBehaviour
{
    [SerializeField,Range(2,50)] int segementCount = 2;
    [SerializeField] private GameObject hingePre;
    public Transform pointA;
    public Transform pointB;
    public HingeJoint2D hingeJoint2D;
    [HideInInspector] public Transform[] segements;

    
    Vector2 GetSegementPosition(int segementIndex)
    {
        Vector2 posA = pointA.position;
        Vector2 posB = pointB.position;
        float fraction = 1f/ (float) segementCount;
        return Vector2.Lerp(posA, posB, fraction * segementIndex);
    }

    [Button]
    void GenerateRope()
    {
        segements = new Transform[segementCount];
        for(int i = 0; i < segementCount; i++)
        {
            var currJoint = Instantiate(hingeJoint2D, GetSegementPosition(i), Quaternion.identity);
            currJoint.transform.SetParent(gameObject.transform);
            segements[i] = currJoint.transform;
            if(i > 0) {
                int prevIndex = i - 1;
                currJoint.GetComponent<HingeJoint2D>().connectedBody = segements[prevIndex].GetComponent<Rigidbody2D>();
            }
           
        }
    }

    [Button]
    void DeleteSegement()
    {
        if(transform.childCount > 0)
        {
            for(int i = transform.childCount -1; i>= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        segements = null;
    }

    private void OnDrawGizmos()
    {
        
        if (pointA == null || pointB == null) return;
        Gizmos.color = Color.green;
        for(int i = 0; i < segementCount; i++)
        {
            Vector2 posAtIndex = GetSegementPosition(i);
            Gizmos.DrawSphere(posAtIndex, 0.1f);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
