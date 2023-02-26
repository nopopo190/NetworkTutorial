using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Coin : NetworkBehaviour
{
    private void Update()
    {
        if(IsServer)
        {
            transform.Rotate(Vector3.up, Space.World);
        }
    }
}
