using Racerr.Car.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Racerr.UX.Car
{
    /// <summary>
    /// Player Bar above their car showing their name and health
    /// </summary>
    public class PlayerBar : MonoBehaviour
    {
        [SerializeField] float playerBarMinDownVelocity = -10; // Minimal velocity needed before applying additional displacement to the bar.

        public PlayerCarController Car { get; set; }
        Transform panel;
        RectTransform healthBar;
        Image healthBarImage;
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
            healthBar = panel.transform.Find("Health").GetComponent<RectTransform>();
            healthBarImage = panel.transform.Find("Health").GetComponent<Image>();
        }

        /// <summary>
        /// Update positive of player bar relative to car.
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            if (Car != null && UnityEngine.Camera.main != null)
            {
                panel.forward = UnityEngine.Camera.main.transform.forward;
                float zVelocity = CarRigidbody.velocity.z; // Velocity in the Z axis (plane of the track)
                float normalizedZVelocity = CarRigidbody.velocity.normalized.z; // Normalised between 0 and 1.

                float additionalBarDisplacement;
                if (zVelocity < playerBarMinDownVelocity) // Minimal velocity condition needed before applying additional displacement to the bar.
                {
                    additionalBarDisplacement = -Car.PlayerBarUpDisplacement * normalizedZVelocity; // Apply negative displacement (towards south of screen)
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

        /// <summary>
        /// Physically adjust the size of the health bar in the Player Bar.
        /// </summary>
        /// <param name="health">Value between 0 - 100 for the health</param>
        public void SetHealthBar(int health)
        {
            healthBar.localScale = new Vector3(health / 100f, healthBar.localScale.y, healthBar.localScale.z);

            if (health < 20)
            {
                healthBarImage.color = Color.red;
            }
            else if (health < 50)
            {
                healthBarImage.color = Color.yellow;
            }
            else
            {
                healthBarImage.color = new Color(0, 1, 0, 0.5f); // Light Green
            }
        }
    }
}

