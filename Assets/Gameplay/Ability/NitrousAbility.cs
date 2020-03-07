using Racerr.Gameplay.Car;
using System;
using System.Collections;
using UnityEngine;

namespace Racerr.Gameplay.Ability
{
    /// <summary>
    /// An ability to apply a boost to the car by massively
    /// increasing its acceleration.
    /// </summary>
    [RequireComponent(typeof(CarPhysicsManager))]
    public class NitrousAbility : MonoBehaviour, IAbility
    {
        [SerializeField] float usageConsumption = 2;
        [SerializeField] float usageRegenRate = 3;
        [SerializeField] float accelerationMultiplier = 4;

        CarPhysicsManager carPhysicsManager;
        float originalAcceleration;
        float targetAcceleration;
        const int minimumUsageAllowed = 5;

        public string DisplayName => "Nitrous";
        public float CooldownRemaining { get; private set; }
        public float MaximumCooldown => minimumUsageAllowed / usageRegenRate;
        public float MaximumUsage => 100;
        float usageRemaining;
        public float UsageRemaining
        {
            get => usageRemaining;
            private set => usageRemaining = Math.Max(0, Math.Min(MaximumUsage, value));
        }
        public bool IsActive { get; private set; } = false;
        
        /// <summary>
        /// Initialise the Nitrous ability so it is ready to be used by the car when
        /// it is instantiated.
        /// </summary>
        void Start()
        {
            carPhysicsManager = GetComponent<CarPhysicsManager>();
            UsageRemaining = MaximumUsage;
            originalAcceleration = targetAcceleration = carPhysicsManager.Acceleration;
        }

        /// <summary>
        /// Called every physics tick. This will regenerate some of the usage, 
        /// calculate the cooldown of the ability (the ability will cooldown if the user runs out of usage),
        /// and smoothly lerps the acceleration, so that the effect of the nitrous is more natural.
        /// </summary>
        void FixedUpdate()
        {
            // By doing this multiplcation, UsageRemaining increases by usageRegenRate every second.
            UsageRemaining += usageRegenRate * Time.fixedDeltaTime;

            CooldownRemaining = Math.Max(0, (minimumUsageAllowed - UsageRemaining) / usageRegenRate);
            carPhysicsManager.Acceleration = Mathf.Lerp(carPhysicsManager.Acceleration, targetAcceleration, 0.02f);
        }

        /// <summary>
        /// Activate the nitrous. The user holds the button and this function will be repeatedly activated.
        /// Nitrous lasts for a hardcoded duration and consumes usage. Any more activations of nitrous during
        /// this period will be ignored. When the duration finishes, the nitrous can activate again.
        /// This is all done really fast so this gives the illusion to the user that they are depleting the
        /// nitrous in a continuous manner.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        public IEnumerator Activate()
        {
            if (UsageRemaining > minimumUsageAllowed && !IsActive)
            {
                // Set the nitrous to active and consume usage. Any more nitrous activations will be ignored.
                IsActive = true;
                UsageRemaining -= usageConsumption;

                // Set the target acceleration, and the acceleration will be smoothly increased to this value in
                // FixedUpdate.
                targetAcceleration = accelerationMultiplier * originalAcceleration;

                // Nitrous stays alive for at least this long after it is activated by the user.
                // If you change this value, it will affect how fast the usage is depleted.
                yield return new WaitForSeconds(0.05f); 

                // Set the active state to false. This allows any other Activate()'s to activate the nitrous again.
                IsActive = false;

                // Allow any other Activate()'s to be called.
                yield return null;

                if (!IsActive) 
                {
                    // Only possible if no more Activate()'s have been called, so immediately drop the acceleration,
                    // as we don't wish to lerp it (the effects of no nitrous should be felt immediately).
                    carPhysicsManager.Acceleration = targetAcceleration = originalAcceleration;
                }

                if (UsageRemaining <= minimumUsageAllowed)
                {
                    // Reset the usage to 0 to allow for a consistent cooldown of the nitrous, if they use it all up.
                    UsageRemaining = 0;
                }
            }
        }
    }
}