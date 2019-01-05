using Racerr.Car.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Racerr.UX.HUD
{
    /// <summary>
    /// Displays RPM for the Car
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class HUDRPM : MonoBehaviour
    {
        [SerializeField] CarController Car;

        Text RPMText { get; set; }

        /// <summary>
        /// Grabs text component
        /// </summary>
        void Start()
        {
            RPMText = GetComponent<Text>();
        }

        /// <summary>
        /// Update text component every frame with the new RPM.
        /// </summary>
        void Update()
        {
            RPMText.text = Car.CurrentRPM + " RPM";
        }
    }
}
