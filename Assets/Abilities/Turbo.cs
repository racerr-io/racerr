using UnityEngine;

public class Turbo : MonoBehaviour {

    [SerializeField]
    float m_SpeedMultiplier;
   
    Car Car { get; set; }

	void OnEnable()
    {
        Car = GetComponentInParent<Car>();
        Car.Speed *= m_SpeedMultiplier;
    }

    void OnDisable()
    {
        Car.Speed /= m_SpeedMultiplier;
    }
}
