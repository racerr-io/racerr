using Racerr.Car.Core;
using System.Linq;
using UnityEngine;

namespace Racerr.Car.Abilities
{
    /// <summary>
    /// Clone ability - Clone your car for fun!
    /// </summary>
    public class Clone : CarAbilityCommon
    {
        /// <summary>
        /// If key is pressed, clone the car. (Called every physics tick)
        /// </summary>
        /// <param name="isKeyPressed">Whether the user has pressed the key on the keyboard.</param>
        protected override void FixedUpdateCore(bool isKeyPressed)
        {
            if (isKeyPressed)
            {
                ActivateAbility();
            }
        }

        /// <summary>
        /// Clone the car by instantiating a copy of our car with IsUsersCar = false.
        /// Calculate the longest part of the car in the z direction, and then place the cloned car z units behind
        /// our current location (not neccessarily behind the boot of the car).
        /// </summary>
        protected override void ActivateAbility()
        {
            float longestColliderLength = Car.GetComponentsInChildren<Collider>().Max(c => c.bounds.size.z);
            Vector3 newPosition = Car.transform.position - new Vector3(0, 0, longestColliderLength);
            CarController clone = Instantiate(Car);
            clone.transform.position = newPosition;
            clone.IsUsersCar = false;
        }

        /// <summary>
        /// Deactivate ability is not required for this ability.
        /// </summary>
        protected override void DeactivateAbility()
        {
        }
    }
}