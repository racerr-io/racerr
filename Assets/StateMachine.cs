using Mirror;
using Racerr.MultiplayerService;
using Racerr.Track;
using System;
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

    public abstract class State : NetworkBehaviour
    {
        public virtual void Enter(object optionalData = null) { }
        public virtual void Exit() { }
    }

    /// <summary>
    /// Race Session State which defines extra interface for handling Race Session Data.
    /// </summary>
    public abstract class RaceSessionState : State
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

    public abstract class StateMachine : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeState))] StateEnum stateType;
        public StateEnum StateType => stateType;
        protected State CurrentState { get; set; }

        /// <summary>
        /// Changes the state of the Server State Machine.
        /// Intended to be PROTECTED - only the Server States should be able to call this from their encapsulated transition methods.
        /// Changes the internal state of the Server State Machine based on the given state type Enum.
        /// </summary>
        /// <param name="stateType">The new state type to be changed to.</param>
        /// <param name="optionalData">Optional data to be passed to the transitioning state.</param>
        public void ChangeState(StateEnum stateType, object optionalData = null)
        {
            if (CurrentState != null)
            {
                // Only time when the current state will be null is when the server starts.
                CurrentState.Exit();
                CurrentState.enabled = false;
            }

            try 
            {
                ChangeStateCore(stateType);
                this.stateType = stateType;

                CurrentState.enabled = true;
                CurrentState.Enter(optionalData);
            } 
            catch (InvalidOperationException e)
            {
                Debug.LogError(e);
            }
        }

        protected abstract void ChangeStateCore(StateEnum stateType);

        /// <summary>
        /// Hook for stateType SyncVar.
        /// Executed on clients when server changes the state.
        /// </summary>
        /// <param name="stateType">Default hook parameter (the updated variable)</param>
        void OnChangeState(StateEnum stateType)
        {
            // Confirm we are only calling this on the client to prevent dupe state calls when testing locally
            if (isClientOnly)
            {
                ChangeState(stateType);
            }
        }
    }
}