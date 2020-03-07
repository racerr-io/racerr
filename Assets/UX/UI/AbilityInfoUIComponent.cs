using Racerr.Gameplay.Ability;
using System;
using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// Shows the current status of the ability attached to the
    /// player's car.
    /// </summary>
    public class AbilityInfoUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI displayNameTMP;
        [SerializeField] TextMeshProUGUI infoTMP;

        /// <summary>
        /// Given an ability, extract information from it and display
        /// usage or cooldown information to the user, depending on the
        /// state of the ability.
        /// </summary>
        /// <param name="ability">Ability.</param>
        public void UpdateAbilityInfo(IAbility ability)
        {
            if (ability == null)
            {
                displayNameTMP.text = infoTMP.text = null;
                return;
            }

            displayNameTMP.text = ability.DisplayName;

            if (ability.IsActive)
            {
                if (ability.UsageRemaining == ability.MaximumUsage)
                {
                    infoTMP.text = "Activated";
                }
                else
                {
                    infoTMP.text = $"{ Math.Round(ability.UsageRemaining) }/{ Math.Round(ability.MaximumUsage) }";
                }
            }
            else
            {
                if (ability.CooldownRemaining > 0)
                {
                    infoTMP.text = $"Cooldown: { ability.CooldownRemaining.ToString("N1") } seconds";
                }
                else if (ability.UsageRemaining == ability.MaximumUsage)
                {
                    infoTMP.text = "<  SPACE  >";
                }
                else
                {
                    infoTMP.text = $"{ Math.Round(ability.UsageRemaining) }/{ Math.Round(ability.MaximumUsage) }";
                }
            }
        }
    }
}