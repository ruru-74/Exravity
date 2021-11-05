using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadEnd : MonoBehaviour
{
    public GunController gun;
    public void RelaodEnd()
    {
        gun.endReload = true;
    }
}
