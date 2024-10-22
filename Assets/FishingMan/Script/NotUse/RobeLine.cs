using UnityEngine;

public class RobeLine : MonoBehaviour
{
    Rob2DCreator rope;
    LineRenderer line;

    private void Awake()
    {
        rope = GetComponent<Rob2DCreator>();
        line = GetComponent<LineRenderer>();
        line.enabled = true;
        line.positionCount = rope.segements.Length;
        print(rope.segements.Length);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        line.positionCount = rope.segements.Length;
        for (int i = 0; i < rope.segements.Length; i++)
        {
            line.SetPosition(i, rope.segements[i].position);
            print(rope.segements.Length);
        }
    }
}
