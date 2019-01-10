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
        [SerializeField] CarController m_Car;

        public CarController Car
        {
            get { return m_Car; }
            set { m_Car = value; }
        }

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
            if (Car != null)
            {
                RPMText.text = Car.CurrentRPM + " RPM";
            }
            else
            {
                RPMText.text = string.Empty;
            }
        }
    }
}
