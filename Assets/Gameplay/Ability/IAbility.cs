using System.Collections;

namespace Racerr.Gameplay.Ability
{
    /// <summary>
    /// Interface for all abilities. You should implement this interface
    /// along with the MonoBehaviour class and attach it to a script. This way,
    /// each ability will be a Unity script which can be dragged and dropped
    /// easily on car prefabs.
    /// </summary>
    public interface IAbility
    {
        /// <summary>
        /// The name of the ability we wish to display to the user.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// The amount of time in seconds remaining before the ability can be used again.
        /// </summary>
        float CooldownRemaining { get; }

        /// <summary>
        /// Maximum time possible in seconds for the cooldown of this ability.
        /// </summary>
        float MaximumCooldown { get; }

        /// <summary>
        /// The amount of usage points this ability has remaining.
        /// If this does not apply to the ability, then set this and MaximumUsage
        /// to 0.
        /// </summary>
        float UsageRemaining { get; }

        /// <summary>
        /// Maximum usage points for this ability.
        /// </summary>
        float MaximumUsage { get; }

        /// <summary>
        /// Whether the ability is currently active or not.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Activate the ability. This function is called asynchronously using
        /// Unity coroutines. The ability must be able to function asynchronously.
        /// </summary>
        IEnumerator Activate();
    }
}