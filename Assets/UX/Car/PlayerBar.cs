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
        [SerializeField] float playerBarMinDrivingDownVelocity = -10; // Minimal velocity needed before applying additional displacement to the bar.

        public float XDisplacement { get; set; }
        public float XDisplacementDrivingDown { get; set; }
        public float YDisplacement { get; set; }
        public float Scale { get; set; }
        public CarManager Car { get; set; }
        Transform panel;
        RectTransform healthBar; // The rectangle of the health bar
        Image healthBarImage; // The thing/image inside the rectangle, in our case just simple colours.
        Rigidbody carRigidBody;
        Rigidbody CarRigidbody => (carRigidBody != null) ? carRigidBody : (carRigidBody = Car?.GetComponent<Rigidbody>());

        /// <summary>
        /// Setup panel and the player's name.
        /// Start is called before the first frame update.
        /// </summary>
        void Start()
        {
            panel = transform.Find("Panel");
            panel.GetComponentInChildren<TextMeshProUGUI>().text = Car.OwnPlayer.PlayerName;
            healthBar = panel.transform.Find("Health").GetComponent<RectTransform>();
            healthBarImage = panel.transform.Find("Health").GetComponent<Image>();
            name = $"Player Bar - {Car.OwnPlayer.PlayerName}";
        }

        /// <summary>
        /// Update positive of player bar relative to car.
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            if (Car != null && UnityEngine.Camera.main != null)
            {
                transform.localScale = new Vector3(Scale, Scale, Scale);
                panel.forward = UnityEngine.Camera.main.transform.forward;
                float zVelocity = CarRigidbody.velocity.z; // Velocity in the Z axis (plane of the track)
                float normalizedZVelocity = CarRigidbody.velocity.normalized.z; // Normalised between 0 and 1.

                float additionalBarDisplacement;
                if (zVelocity < playerBarMinDrivingDownVelocity) // Minimal velocity condition needed before applying additional displacement to the bar.
                {
                    additionalBarDisplacement = -XDisplacementDrivingDown * normalizedZVelocity; // Apply negative displacement (towards south of screen)
                }
                else
                {
                    additionalBarDisplacement = 0;
                }

                transform.position = Car.transform.position + new Vector3(0, YDisplacement, XDisplacement + additionalBarDisplacement);
            }
        }

        /// <summary>
        /// Update health bar every physics tick. We do it here since health can only change as a result
        /// of collisions, and collision stuff is calculated during FixedUpdate.
        /// </summary>
        void FixedUpdate()
        {
            if (Car != null && UnityEngine.Camera.main != null)
            {
                UpdateHealthBar();
            }
            else
            {
                Destroy(gameObject); // Automatically delete the player bar if the car is destroyed / doesn't exist.
            }
        }

        /// <summary>
        /// Physically adjust the size of the health bar in the Player Bar by polling
        /// the player's health. If the attached car is a zombie car then it must mean
        /// the car is dead, so in that case the health is 0.
        /// </summary>
        void UpdateHealthBar()
        {
            int health = 0;
            if (!Car.IsZombie)
            {
                health = Car.OwnPlayer.Health;
            }

            healthBar.localScale = new Vector3(health / (float)Car.MaxHealth, healthBar.localScale.y, healthBar.localScale.z);
            float halfMaxHealth = Car.MaxHealth / 2f;

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

