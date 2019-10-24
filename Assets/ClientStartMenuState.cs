using Doozy.Engine.UI;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientStartMenuState : State
    {
        [SerializeField] UIView startMenuView;

        public override void Enter(object optionalData = null)
        {
            startMenuView.Show();
        }

        public override void Exit()
        {
            startMenuView.Hide();
        }
    }
}