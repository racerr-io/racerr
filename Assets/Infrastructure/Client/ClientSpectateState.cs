using Racerr.Infrastructure.Server;
using System.Collections.Generic;
using System.Linq;

namespace Racerr.Infrastructure.Client
{
    /// <summary>
    /// A state for the client when they are spectating the race. They will spectate the race
    /// either when they join the server for the first time or they are dead.
    /// </summary>
    public class ClientSpectateState : LocalState
    {
        IEnumerable<Player> playersInRace = null;
        Player playerBeingSpectated;

        /// <summary>
        /// Upon entering the spectate state on the client, find all the players we can spectate.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            playersInRace = FindObjectsOfType<Player>().Where(player => player != null && !player.IsDead && !player.PosInfo.IsFinished);
        }

        /// <summary>
        /// Called every physics tick. Updates who we are spectating, then checks if we should transition to a new client state.
        /// </summary>
        protected override void FixedUpdate()
        {
            SetPlayerBeingSpectatedIfRequired();
            CheckToTransition();
        }

        /// <summary>
        /// Find a player in the race that we can spectate. From the players in the race, we choose the first player to spectate, ensuring
        /// they haven't finished, died or left the server.
        /// </summary>
        void SetPlayerBeingSpectatedIfRequired()
        {
            if (playerBeingSpectated == null || playerBeingSpectated.IsDead)
            {
                // playersInRace could be empty, as there is a small window of time where everyone has died/finished but the Server State Machine
                // has not transitioned to intermission yet.
                playersInRace = playersInRace.Where(player => player != null && !player.IsDead && !player.PosInfo.IsFinished);
                playerBeingSpectated = playersInRace.FirstOrDefault(); 
                ClientStateMachine.Singleton.SetPlayerCameraTarget(playerBeingSpectated?.CarManager.transform);
            }
        }

        /// <summary>
        /// If the server has changed to intermission, it means we can join the next race! Hence, transition the client to
        /// intermission state.
        /// </summary>
        void CheckToTransition()
        {
            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                TransitionToIntermission();
            }
        }

        void TransitionToIntermission()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.Intermission);
        }
    }
}