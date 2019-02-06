using Racerr.Car.Core;
using UnityEngine;

namespace Racerr.Car.SkyCar
{
    /// <summary>
    /// this script is specific to the supplied SkyCar which has mudguards over the front wheels
    /// which have to turn with the wheels when steering is applied.
    /// </summary>
    public class Mudguard : MonoBehaviour
    {
        [SerializeField] CarController CarController; // car controller to get the steering angle
        Quaternion OriginalRotation { get; set; }

        /// <summary>
        /// Keep track of the original rotaation of the mudguard.
        /// </summary>
        void Start()
        {
            OriginalRotation = transform.localRotation;
        }

        /// <summary>
        /// Called every frame to update the rotation of the mudguard.
        /// </summary>
        void Update()
        {
            transform.localRotation = OriginalRotation*Quaternion.Euler(0, CarController.CurrentSteerAngle, 0);
        }
    }
}
