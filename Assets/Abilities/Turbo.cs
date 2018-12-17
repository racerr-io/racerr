using UnityEngine;

public class Turbo : CarAbilityCommon
{
    [SerializeField]
    float m_SpeedMultiplier;

    protected override void ActivateAbilityCore(Car car)
    {
        car.Speed *= m_SpeedMultiplier;
    }

    protected override void DeactivateAbilityCore(Car car)
    {
        car.Speed /= m_SpeedMultiplier;
    }
}
