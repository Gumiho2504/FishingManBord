using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    private float speed = 5f;

    private void Update()
    {

        if (!IsOwner) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (h != 0 || v != 0)
            Debug.Log($"Owner {OwnerClientId} moving: ({h}, {v})");


        transform.Translate(new Vector3(h, v, 0) * speed * Time.deltaTime);

    }

}

