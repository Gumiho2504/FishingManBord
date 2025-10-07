using System;
using Unity.Netcode;
using UnityEngine;

public class LineController : NetworkBehaviour
{
    private LineRenderer lr;
    private Transform[] points;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }


    bool isClientStated = false;

    public override void OnNetworkSpawn()
    {
        // NetworkManager.Singleton.OnClientStarted += () =>
        // {
        //     if (IsClient)
        //     {
        //         isClientStated = true;
        //     }
        // };
    }




    public void SetUpLine(Transform[] points)
    {
        lr.positionCount = points.Length;
        this.points = points;
    }




    private void Update()
    {
        // if (isClientStated)
        // {
        //     for (int i = 0; i < points.Length; i++)
        //     {
        //         lr.SetPosition(i, points[i].position);
        //     }
        // }

    }
}
