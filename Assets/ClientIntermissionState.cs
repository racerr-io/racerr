using Doozy.Engine.UI;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientIntermissionState : State
    {
        [SerializeField] UIView intermissionView;

        public override void Enter(object optionalData = null)
        {
            intermissionView.Show();
        }

        public override void Exit()
        {
            intermissionView.Hide();
        }
    }
}