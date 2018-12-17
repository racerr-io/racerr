public class Clone : CommonCarAbility
{
    protected override void FixedUpdateCore(bool isKeyPressed)
    {
        if (isKeyPressed)
        {
            ActivateAbility();
        }
    }

    protected override void ActivateAbilityCore(Car car)
    {
        var clone = Instantiate(car);
        clone.IsUsersCar = false;
    }

    protected override void DeactivateAbilityCore(Car car)
    {
        // Not required for this ability
    }
}
