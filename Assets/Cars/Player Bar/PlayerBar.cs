using Racerr.Car.Core;
using TMPro;
using UnityEngine;

namespace Racerr.UX.Car
{
    /// <summary>
    /// Player Bar above their car showing their name and health
    /// </summary>
    public class PlayerBar : MonoBehaviour
    {
        [SerializeField] float playerBarMinDownVelocity = -10;

        public PlayerCarController Car { get; set; }
        Transform panel;

        Rigidbody carRigidBody;
        Rigidbody CarRigidbody => (carRigidBody != null) ? carRigidBody : (carRigidBody = Car?.GetComponent<Rigidbody>());

        /// <summary>
        /// Setup panel and the player's name.
        /// Start is called before the first frame update.
        /// </summary>
        void Start()
        {
            panel = transform.Find("Panel");
            panel.GetComponentInChildren<TextMeshProUGUI>().text = Car.Player.PlayerName;
        }

        /// <summary>
        /// Update positive of player bar relative to car.
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            if (Car != null)
            {
                panel.forward = UnityEngine.Camera.main.transform.forward;
                float zVelocity = CarRigidbody.velocity.z;
                float normalizedZVelocity = CarRigidbody.velocity.normalized.z;

                float additionalBarDisplacement;
                if (zVelocity < playerBarMinDownVelocity)
                {
                    additionalBarDisplacement = -Car.PlayerBarUpDisplacement * normalizedZVelocity;
                }
                else
                {
                    additionalBarDisplacement = 0;
                }

                transform.position = Car.transform.position + new Vector3(0, 0, Car.PlayerBarStartDisplacement + additionalBarDisplacement);
            }
            else
            {
                Destroy(gameObject); // Automatically delete the player bar if the car is destroyed / doesn't exist.
            }
        }
    }
}

