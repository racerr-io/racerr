using Mirror;
using Racerr.MultiplayerService;
using Racerr.Track;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.StateMachine
{
    public enum StateEnum
    {
        #region Shared States
        Race,
        Intermission,
        #endregion
        
        #region Server Only States
        ServerIdle,
        #endregion

        #region Client Only States
        ClientStartMenu,
        ClientSpectate
        #endregion
    }

    public interface IState
    {
        void Enter(object optionalData = null);
        void Exit();
    }

    public abstract class NetworkedState : NetworkBehaviour, IState
    {
        public virtual void Enter(object optionalData = null) { }
        public virtual void Exit() { }
        protected virtual void FixedUpdate() { }
    }

    public abstract class LocalState : MonoBehaviour, IState
    {
        public virtual void Enter(object optionalData = null) { }
        public virtual void Exit() { }
        protected virtual void FixedUpdate() { }
    }

    /// <summary>
    /// Race Session State which defines extra interface for handling Race Session Data.
    /// </summary>
    public abstract class RaceSessionState : NetworkedState
    {
        /// <summary>
        /// Data passed around between Race and Intermission states (Intermission needs the most recent race data to be displayed).
        /// </summary>
        public sealed class RaceSessionData
        {
            public List<Player> PlayersInRace { get; } = new List<Player>();
            public List<Player> FinishedPlayers { get; } = new List<Player>();
            public IReadOnlyCollection<Player> DeadPlayers => PlayersInRace.Where(p => p.IsDead).ToArray();
            public IEnumerable<Player> PlayersInRaceOrdered
            {
                get
                {
                    return PlayersInRace
                        .OrderBy(player => player.PositionInfo.FinishingTime)
                        .ThenByDescending(player => player.PositionInfo.Checkpoints.Count)
                        .ThenBy(player =>
                        {
                            Vector3? currCarPosition = player.Car?.transform.position;
                            GameObject[] checkpointsInRace = TrackGeneratorCommon.Singleton.CheckpointsInRace;
                            if (currCarPosition == null || checkpointsInRace == null)
                            {
                            // For some reason the player has no car or the race hasn't started,
                            // so let's just be safe rather than crash.
                            return float.PositiveInfinity;
                            }

                        // checkpointsInRace is sorted in the order of the checkpoints in the race,
                        // so to grab the next checkpoint for this car we use the checkpoint count for this player as an index.
                        int nextCheckpoint = player.PositionInfo.Checkpoints.Count;
                            Vector3 nextCheckpointPosition = checkpointsInRace[nextCheckpoint].transform.position;
                            return Vector3.Distance(currCarPosition.Value, nextCheckpointPosition);
                        });
                }
            }
        }

        protected RaceSessionData raceSessionData;
        public void RemovePlayer(Player player)
        {
            raceSessionData.PlayersInRace.Remove(player);
            raceSessionData.FinishedPlayers.Remove(player);
        }
    }

    public interface IStateMachine
    {
        StateEnum StateType { get; }
        void ChangeState(StateEnum stateType, object optionalData = null);
    }
}