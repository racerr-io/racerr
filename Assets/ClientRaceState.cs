using Doozy.Engine.UI;
using UnityEngine;

namespace Racerr.StateMachine.Client
{
    public class ClientRaceState : State
    {
        [SerializeField] UIView raceView;

        public override void Enter(object optionalData = null)
        {
            raceView.Show();
        }

        public override void Exit()
        {
            raceView.Hide();
        }
    }
}