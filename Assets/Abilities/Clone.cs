/// <summary>
/// Clone ability - Clone your car for fun!
/// </summary>
public class Clone : CarAbilityCommon
{
    /// <summary>
    /// If key is pressed, clone the car.
    /// </summary>
    /// <param name="isKeyPressed"></param>
    protected override void FixedUpdateCore(bool isKeyPressed)
    {
        if (isKeyPressed)
        {
            ActivateAbility();
        }
    }

    /// <summary>
    /// Clone the car by instantiating a copy of our car with IsUsersCar = false
    /// </summary>
    /// <param name="car"></param>
    protected override void ActivateAbility()
    {
        Car clone = Instantiate(Car);
        clone.IsUsersCar = false;
    }

    /// <summary>
    /// Deactivate ability is not required for this ability.
    /// </summary>
    /// <param name="car"></param>
    protected override void DeactivateAbility()
    {
    }
}
