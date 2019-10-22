using Mirror;
using Racerr.MultiplayerService;
using Racerr.Track;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ServerStateEnum
{
    Race,
    Intermission,
    Idle
}

public abstract class State : NetworkBehaviour
{
    public virtual void Enter(object optionalData = null) {}
    public virtual void Exit() {}
}

/// <summary>
/// Race Session State which defines extra interface for handling Race Session Data.
/// </summary>
public abstract class RaceSessionState : State
{
    /// <summary>
    /// Data passed around between Race and Intermission states (Intermission needs the most recent race data to be displayed).
    /// </summary>
    public class RaceSessionData
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
    public void RemovePlayer(Player player) {
        raceSessionData.PlayersInRace.Remove(player);
        raceSessionData.FinishedPlayers.Remove(player);
    }
}

public class RaceState : RaceSessionState
{
    /// <summary>
    /// Initialises brand new race session data independant of previous race sessions.
    /// Then starts generating the track, which will then start the race.
    /// </summary>
    public override void Enter(object optionalData = null)
    {
        raceSessionData = new RaceSessionData();
        StartCoroutine(TrackGenThenStartRace());
    }

    /// <summary>
    /// Coroutine for race starting, since we need to wait for the track 
    /// to be generated before executing more code (simulating a semaphore).
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    [Server]
    IEnumerator TrackGenThenStartRace()
    {
        TrackGeneratorCommon.Singleton.GenerateIfRequired();
        while (!TrackGeneratorCommon.Singleton.IsTrackGenerated) yield return null;

        StartRace();
    }

    /// <summary>
    /// Procedure to actually setup and start the race.
    /// Called only after track is generated.
    /// </summary>
    void StartRace() 
    {
        Vector3 currPosition = new Vector3(0, 1, 10);
        raceSessionData.PlayersInRace.AddRange(ServerStateMachine.Singleton.ReadyPlayers);

        foreach (Player player in raceSessionData.PlayersInRace)
        {
            player.CreateCarForPlayer(currPosition);
            player.PositionInfo = new PlayerPositionInfo();
            currPosition += new Vector3(0, 0, 10);
        }

        //raceStartTime = NetworkTime.time;
        //isCurrentlyRacing = true;
    }
    
    /// <summary>
    /// Called every game tick.
    /// Checks whether or not to transition to intermission state, based on if the race is finished or empty.
    /// </summary>
    void LateUpdate() 
    {
        bool isRaceFinished = raceSessionData.FinishedPlayers.Count + raceSessionData.DeadPlayers.Count == raceSessionData.PlayersInRace.Count;
        bool isRaceEmpty = raceSessionData.PlayersInRace.Count == 0;

        if (isRaceEmpty)
        {
            TransitionToIdle();
        }
        else if (isRaceFinished) 
        {
            TransitionToIntermission();
        }
    }

    public void TransitionToIntermission()
    {
        ServerStateMachine.Singleton.ChangeState(ServerStateEnum.Intermission, raceSessionData);
    }

    public void TransitionToIdle()
    {
        ServerStateMachine.Singleton.ChangeState(ServerStateEnum.Idle);
    }
}

public class IdleState : State
{
    /// <summary>
    /// Called every game tick.
    /// Checks whether or not to transition to intermission state, based on if the server has any connected players.
    /// </summary>
    void LateUpdate()
    {
        if (ServerStateMachine.Singleton.PlayersOnServer.Any(p => p.IsReady))
        {
            TransitionToIntermission();
        }
    }

    void TransitionToIntermission()
    {
        ServerStateMachine.Singleton.ChangeState(ServerStateEnum.Intermission);    
    }
}

public class IntermissionState : RaceSessionState
{
    [SerializeField] int intermissionTimerSecondsEditor;
    [SerializeField] int intermissionTimerSeconds;
    [SerializeField] int intermissionTimerSecondsSinglePlayer;

    [SyncVar] int intermissionSecondsRemaining;

    /// <summary>
    /// Transition function called on entering the intermission state.
    /// Initialises the race session data of the race that just finished, OR null if transitioned from idle state.
    /// Initialises the duration of the intermission based on whether the game is in the Unity Editor or not.
    /// Immediately begins the intermission timer countdown.
    /// </summary>
    /// <param name="raceSessionData">Data of the race that just finished, OR null if transitioned from idle state.</param>
    public override void Enter(object raceSessionData)
    {
        this.raceSessionData = raceSessionData as RaceSessionData;
#if UNITY_EDITOR
        intermissionSecondsRemaining = intermissionTimerSecondsEditor;
#else
        intermissionSecondsRemaining = ServerStateMachine.Singleton.ReadyPlayers.Count > 1 ? 
            ServerStateMachine.Singleton.intermissionTimerSeconds : 
            ServerStateMachine.Singleton.intermissionTimerSecondsSinglePlayer;
#endif
        StartCoroutine(IntermissionTimer());
    }

    /// <summary>
    /// Coroutine for counting down the intermission timer.
    /// When timer reaches 0, forces a state change depending on whether or not there are players.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    [Server]
    IEnumerator IntermissionTimer()
    {
        while (intermissionSecondsRemaining > 0)
        {
            yield return new WaitForSeconds(1);
            intermissionSecondsRemaining--;
        }

        // Intermission Timer fully finished - now we transition to states based on whether or not there are players.
        if (raceSessionData.PlayersInRace.Any())
        {
            TransitionToRace();
        }
        else
        {
            TransitionToIdle();
        }
    }

    void TransitionToRace() {
        ServerStateMachine.Singleton.ChangeState(ServerStateEnum.Race);
    }

    void TransitionToIdle() {
        ServerStateMachine.Singleton.ChangeState(ServerStateEnum.Idle);
    }
}

public class ServerStateMachine : NetworkBehaviour
{
    public static ServerStateMachine Singleton;

    [SyncVar(hook = nameof(OnChangeState))] ServerStateEnum stateType;
    State currentState;

    List<Player> playersInServer = new List<Player>();
    public IReadOnlyCollection<Player> PlayersOnServer => playersInServer;
    public IReadOnlyCollection<Player> ReadyPlayers => playersInServer.Where(p => p.IsReady).ToArray();

    /// <summary>
    /// Run when this script is instantiated.
    /// Set up the Singleton variable and ensure only one Server State Machine is in the scene.
    /// </summary>
    void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Debug.LogError("You can only have one Server State Machine in the scene. The extra one has been destroyed.");
            Destroy(this);
        }
    }

    /// <summary>
    /// Entrypoint into Server State Machine. Called after unity initialises all scripts.
    /// Defaults to an Idle server state.
    /// </summary>
    void Start()
    {
        ChangeState(ServerStateEnum.Idle);
    }

    /// <summary>
    /// Add a new player to the Server State.
    /// </summary>
    /// <param name="playerGameObject">Player Game Object.</param>
    [Server]
    public void AddNewPlayer(GameObject playerGameObject)
    {
        Player player = playerGameObject.GetComponent<Player>();
        playersInServer.Add(player);
    }

    /// <summary>
    /// Remove an existing player from the Server State, and potential Race Session.
    /// </summary>
    /// <param name="playerGameObject">Player Game Object.</param>
    [Server]
    public void RemovePlayer(GameObject playerGameObject)
    {
        Player player = playerGameObject.GetComponent<Player>();
        playersInServer.Remove(player);
        (currentState as RaceSessionState)?.RemovePlayer(player);
    }

    /// <summary>
    /// Changes the state of the Server State Machine.
    /// Intended to be PROTECTED - only the Server States should be able to call this from their encapsulated transition methods.
    /// Changes the internal state of the Server State Machine based on the given state type Enum.
    /// </summary>
    /// <param name="stateType">The new state type to be changed to.</param>
    /// <param name="optionalData">Optional data to be passed to the transitioning state.</param>
    public void ChangeState(ServerStateEnum stateType, object optionalData = null)
    {
        if (currentState != null)
        {
            // Only time when the current state will be null is when the server starts.
            currentState.Exit();
            currentState.enabled = false;
        }
       
        this.stateType = stateType;
        
        switch (stateType)
        {
            case ServerStateEnum.Race: currentState = GetComponent<RaceState>(); break;
            case ServerStateEnum.Intermission: currentState = GetComponent<IntermissionState>(); break;
            case ServerStateEnum.Idle: currentState = GetComponent<IdleState>(); break;
            default: Debug.LogError("Invalid State"); break;
        }

        currentState.enabled = true;
        currentState.Enter(optionalData);
    }

    /// <summary>
    /// Hook for stateType SyncVar.
    /// Executed on clients when server changes the state.
    /// </summary>
    /// <param name="stateType">Default hook parameter (the updated variable)</param>
    public void OnChangeState(ServerStateEnum stateType)
    {
        ChangeState(stateType);
    }
}

