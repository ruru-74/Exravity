using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndCamera : MonoBehaviour
{
    private void FixedUpdate()
    {
        this.transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y + -10f*Time.deltaTime, 0));
    }
}