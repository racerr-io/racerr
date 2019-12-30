using Racerr.Infrastructure.Server;
using System.Linq;
using System.Collections.Generic;

namespace Racerr.Infrastructure.Client
{
    /// <summary>
    /// A state for the client when they are spectating the race. They will spectate the race
    /// either when they join the server for the first time or they are dead.
    /// </summary>
    public class ClientSpectateState : LocalState
    {
        Player playerBeingSpectated;
        IEnumerable<Player> playersInRace = null;

        /// <summary>
        /// Upon entering the spectate state on the client, set the first player in the race to be spectated.
        /// </summary>
        /// <param name="optionalData">Should be null</param>
        public override void Enter(object optionalData = null)
        {
            playersInRace = FindObjectsOfType<Player>().Where(player => player != null && !player.IsDead && !player.PosInfo.IsFinished);
        }

        /// <summary>
        /// Called every physics tick to monitor the server state. If the server has changed to intermission,
        /// it means we can join the next race! Hence, update our UI to the Intermission State.
        /// If the player we are spectating leaves the game or dies, we refind the new first player to spectate.
        /// </summary>
        protected override void FixedUpdate()
        {
            if (ServerStateMachine.Singleton.StateType == StateEnum.Intermission)
            {
                TransitionToIntermission();
            }
            else if (playerBeingSpectated == null || playerBeingSpectated.IsDead)
            {
                SetPlayerBeingSpectated();
            }
        }

        void TransitionToIntermission()
        {
            ClientStateMachine.Singleton.ChangeState(StateEnum.Intermission);
        }

        /// <summary>
        /// Find the players in the race that we can spectate.
        /// We can assume this to be non-empty as the race would already be finished otherwise and we would transition to intermission state.
        /// From the players in the race, we choose the first player to spectate.
        /// </summary>
        void SetPlayerBeingSpectated()
        {
            playersInRace = playersInRace.Where(player => player != null && !player.IsDead && !player.PosInfo.IsFinished);
            playerBeingSpectated = playersInRace.First();
            ClientStateMachine.Singleton.SetPlayerCameraTarget(playersInRace.First().CarManager.transform);
            ClientStateMachine.Singleton.SetMinimapCameraTarget(playersInRace.First().CarManager.transform);
        }
    }
}