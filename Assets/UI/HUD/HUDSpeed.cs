using Racerr.Car.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Racerr.UX.HUD
{
    /// <summary>
    /// Displays speed of the car
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class HUDSpeed : MonoBehaviour
    {
        [SerializeField] CarController Car;

        Text SpeedText { get; set; }

        /// <summary>
        /// Grabs text component
        /// </summary>
        void Start()
        {
            SpeedText = GetComponent<Text>();
        }

        /// <summary>
        /// Update text component every frame with the new speed.
        /// </summary>
        void Update()
        {
            SpeedText.text = $"{(int)Car.CurrentSpeed} {Car.SpeedTypeMetric}";
        }
    }
}
