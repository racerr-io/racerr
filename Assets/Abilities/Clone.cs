using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clone : MonoBehaviour {

    Car Car { get; set; }

    public void CloneCar()
    {
        Car = GetComponentInParent<Car>();
        var clone = Instantiate(Car);
        clone.IsUsersCar = false;
    }
}
