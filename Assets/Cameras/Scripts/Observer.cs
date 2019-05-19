using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observer : MonoBehaviour
{
    void Update()
    {
        Vector3 direction = Player.LocalPlayer.Car.position - transform.position + Vector3.up;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit raycastHit;
    }
}
