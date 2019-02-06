using UnityEngine;

namespace Racerr.Car.Core
{
    /// <summary>
    /// Control the brake light redender on a car
    /// </summary>
    public class BrakeLight : MonoBehaviour
    {
        [SerializeField] CarController Car;

        Renderer Renderer { get; set; }

        /// <summary>
        /// Get the Brake Light Renderer about Instantiation.
        /// </summary>
        void Start()
        {
            Renderer = GetComponent<Renderer>();
        }

        /// <summary>
        /// Enable the Renderer when the car is braking, disable it otherwise.
        /// </summary>
        void Update()
        {
            Renderer.enabled = Car.BrakeInput > 0f;
        }
    }
}
