using UnityEngine;

namespace Racerr.Car.SkyCar
{
    /// <summary>
    /// This script is specific to the SkyCar.
    /// It controls the suspension hub to make it move with the wheel are it goes over bumps
    /// </summary>
    public class Suspension : MonoBehaviour
    {
        [SerializeField] GameObject m_Wheel; // The wheel that the script needs to referencing to get the postion for the suspension

        Vector3 TargetOriginalPosition;
        Vector3 Origin;

        /// <summary>
        /// Acquire suspension hub and wheel position infomration.
        /// </summary>
        void Start()
        {
            TargetOriginalPosition = m_Wheel.transform.localPosition;
            Origin = transform.localPosition;
        }

        /// <summary>
        /// For each frame update the position of the suspesion hub relative to the wheel.
        /// </summary>
        void Update()
        {
            transform.localPosition = Origin + (m_Wheel.transform.localPosition - TargetOriginalPosition);
        }
    }
}
