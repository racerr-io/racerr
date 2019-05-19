using UnityEngine

namespace Racerr.Car.Abilities
{
    /// <summary>
    /// Turbo ability - make your car faster.
    /// </summary>
    public class Turbo : CarAbilityCommon
    {
        [SerializeField] float speedMultiplier;

        /// <summary>
        /// Multiply the cars speed by the specified multiplier.
        /// </summary>
        protected override void ActivateAbility()
        {
            Car.MultiplySpeed(speedMultiplier);
        }

        /// <summary>
        /// Divide the cars speed by the specified multiplier (essentially bringing it back to original speed).
        /// </summary>
        protected override void DeactivateAbility()
        {
            Car.MultiplySpeed(1 / speedMultiplier);
        }
    }
}