using UnityEngine;

namespace Racerr.Car.Core
{
    /// <summary>
    /// Reads user inputs and manipulates them so that Car.Move() can be called.
    /// </summary>
    [RequireComponent(typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {
        CarController Car { get; set; }

        /// <summary>
        /// Get the Car about Instantiation.
        /// </summary>
        void Awake()
        {
            Car = GetComponent<CarController>();

#if !UNITY_EDITOR && !UNITY_WEBGL
            Debug.LogError("Car User Control is running on an unsupported platform.");
#endif
        }

        /// <summary>
        /// Called every physics tick to get the users inputs and move the car.
        /// </summary>
        void FixedUpdate()
        {
            if (Car.IsUsersCar)
            {
                float h = Input.GetAxis("Horizontal");
                float v = Input.GetAxis("Vertical");
                float handbrake = Input.GetAxis("Jump");
                Car.Move(h, v, v, handbrake);
            }
        }
    }
}
