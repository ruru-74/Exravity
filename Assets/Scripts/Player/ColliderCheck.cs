using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCheck : MonoBehaviour
{
    [HideInInspector]public PlayerController player;

    private const string ground = "Ground";
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag(ground))//地面に当たったら
        {
            player.IsGround = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag(ground))//地面に当たったら
        {
            player.IsGround = false;
        }
    }
}