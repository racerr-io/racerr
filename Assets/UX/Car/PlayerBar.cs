using Racerr.Gameplay.Car;
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

        public CarManager CarManager { get; set; }
        Transform panel;
        RectTransform healthBar; // The rectangle of the health bar
        Image healthBarImage; // The thing/image inside the rectangle, in our case just simple colours.
        Rigidbody carRigidBody;
        Rigidbody CarRigidbody => (carRigidBody != null) ? carRigidBody : (carRigidBody = CarManager?.GetComponent<Rigidbody>());

        /// <summary>
        /// Setup panel and the player's name.
        /// Start is called before the first frame update.
        /// </summary>
        void Start()
        {
            panel = transform.Find("Panel");
            panel.GetComponentInChildren<TextMeshProUGUI>().text = CarManager.OwnPlayer.PlayerName;
            healthBar = panel.transform.Find("Health").GetComponent<RectTransform>();
            healthBarImage = panel.transform.Find("Health").GetComponent<Image>();
            SetHealthBar(CarManager.OwnPlayer.Health);
        }

        /// <summary>
        /// Update positive of player bar relative to car.
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            if (CarManager != null && UnityEngine.Camera.main != null)
            {
                panel.forward = UnityEngine.Camera.main.transform.forward;
                float zVelocity = CarRigidbody.velocity.z; // Velocity in the Z axis (plane of the track)
                float normalizedZVelocity = CarRigidbody.velocity.normalized.z; // Normalised between 0 and 1.

                float additionalBarDisplacement;
                if (zVelocity < playerBarMinDownVelocity) // Minimal velocity condition needed before applying additional displacement to the bar.
                {
                    additionalBarDisplacement = -CarManager.PlayerBarUpDisplacement * normalizedZVelocity; // Apply negative displacement (towards south of screen)
                }
                else
                {
                    additionalBarDisplacement = 0;
                }

                transform.position = CarManager.transform.position + new Vector3(0, 0, CarManager.PlayerBarStartDisplacement + additionalBarDisplacement);
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
            healthBar.localScale = new Vector3(health / (float)CarManager.MaxHealth, healthBar.localScale.y, healthBar.localScale.z);
            float halfMaxHealth = CarManager.MaxHealth / 2f;

            if (health < halfMaxHealth)
            {
                healthBarImage.color = Color.Lerp(Color.red, Color.yellow, health / halfMaxHealth);
            }
            else
            {
                healthBarImage.color = Color.Lerp(Color.yellow, new Color(0, 0.7f, 0), (health - halfMaxHealth)/halfMaxHealth);
            }
        }
    }
}

