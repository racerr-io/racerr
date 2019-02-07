using Racerr.Car.Core;
using UnityEngine;

namespace Racerr.Car.Abilities
{
    /// <summary>
    /// Common class for abilities - all abilities must inherit from this class.
    /// </summary>
    public abstract class CarAbilityCommon : MonoBehaviour
    {
        [SerializeField] KeyCode key;

        bool IsKeyPressed { get; set; }
        protected CarController Car { get; private set; }
        protected bool IsActivated { get; set; }

        /// <summary>
        /// Grab the Car script attached to the GameObject this ability is attached to.
        /// </summary>
        void Start()
        {
            Car = GetComponent<CarController>();

            if (Car == null)
            {
                Destroy(this);
                Debug.LogError("Car Ability Intialisation Failure - Please ensure you attached this ability script to a GameObject which has the Car script. " +
                    "The ability has been removed from the attached object for safety.");
            }
        }

        /// <summary>
        /// Called every frame to update the key pressed status of the ability for the user's car only.
        /// </summary>
        void Update()
        {
            if (Car.isLocalPlayer)
            {
                IsKeyPressed = Input.GetKeyDown(key);
            }
        }

        /// <summary>
        /// Update the status of the ability every physics tick on the user's car only.
        /// </summary>
        void FixedUpdate()
        {
            if (Car.isLocalPlayer)
            {
                FixedUpdateCore(IsKeyPressed);
            }
        }

        /// <summary>
        /// Called every physics tick (everytime FixedUpdate() is called).
        /// Depending on the ability you want to implement you may need to override this.
        /// </summary>
        /// <param name="isKeyPressed">Whether the user has pressed the key on the keyboard.</param>
        protected virtual void FixedUpdateCore(bool isKeyPressed)
        {
            if (isKeyPressed)
            {
                if (!IsActivated)
                {
                    IsActivated = true;
                    ActivateAbility();
                }
                else if (IsActivated)
                {
                    IsActivated = false;
                    DeactivateAbility();
                }
            }
        }

        /// <summary>
        /// The implementation to activate the ability - affects the user's car and the environment.
        /// </summary>
        abstract protected void ActivateAbility();

        /// <summary>
        /// The implementation to deactivate the ability - affects the user's car and the environment.
        /// </summary>
        abstract protected void DeactivateAbility();
    }
}