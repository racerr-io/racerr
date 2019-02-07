using Racerr.Car.Core;
using System;
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
        [SerializeField] PlayerCarController car;

        public PlayerCarController Car
        {
            get { return car; }
            set { car = value; }
        }
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
            if (Car != null)
            {
                SpeedText.text = Convert.ToInt32(Car.GetComponent<Rigidbody>().velocity.magnitude * 2) + " KPH";
            }
            else
            {
                SpeedText.text = string.Empty;
            }
            
        }
    }
}
