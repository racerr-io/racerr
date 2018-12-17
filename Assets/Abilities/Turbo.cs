using UnityEngine;

/// <summary>
/// Turbo ability - make your car faster.
/// </summary>
public class Turbo : CarAbilityCommon
{
    [SerializeField]
    float m_SpeedMultiplier;

    /// <summary>
    /// Multiply the cars speed by the specified multiplier.
    /// </summary>
    /// <param name="car"></param>
    protected override void ActivateAbility()
    {
        Car.Speed *= m_SpeedMultiplier;
    }

    /// <summary>
    /// Divide the cars speed by the specified multiplier (essentially bringing it back to original speed).
    /// </summary>
    /// <param name="car"></param>
    protected override void DeactivateAbility()
    {
        Car.Speed /= m_SpeedMultiplier;
    }
}
