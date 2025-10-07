using System;
using Unity.Netcode;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public bool isEat = false;

    // private NetworkVariable<bool> isFlip = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    //[SerializeField] private SpriteRenderer spriteRenderer;
    public Food food;




    // public override void OnNetworkSpawn()
    // {
    //     //  base.OnNetworkSpawn();
    //     isFlip.OnValueChanged += FlipFish_ValueChanged;

    //     spriteRenderer.flipX = isFlip.Value;
    // }

    // private void FlipFish_ValueChanged(bool previousValue, bool newValue)
    // {
    //     spriteRenderer.flipX = newValue;
    // }


    // // [Rpc(SendTo.Server)]
    // public void SetFlipRpc(bool isFlip)
    // {
    //     spriteRenderer.flipX = isFlip;
    //     this.isFlip.Value = isFlip;
    // }
}
