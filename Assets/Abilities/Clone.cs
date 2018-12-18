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
    /// Clone the car by instantiating a copy of our car with IsUsersCar = false
    /// </summary>
    protected override void ActivateAbility()
    {
        Car clone = Instantiate(Car);
        clone.IsUsersCar = false;
    }

    /// <summary>
    /// Deactivate ability is not required for this ability.
    /// </summary>
    protected override void DeactivateAbility()
    {
    }
}
