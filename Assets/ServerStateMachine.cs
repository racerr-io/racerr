using Mirror;
using Racerr.MultiplayerService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ServerStateEnum
{
    Race,
    Intermission,
    Idle
}

public interface IState
{
    void Enter();
    void Exit();

    /// <summary>
    /// Manually called by ServerStateMachine every tick using its special LateUpdate function.
    /// Needed as otherwise, we would have to separate each state into its own script in order to have
    /// its own update every tick function.
    /// </summary>
    void ServerUpdate();
}

public class RaceState : IState
{
    public void Enter()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void TransitionToIntermission() {
        ServerStateMachine.Singleton.ChangeState(ServerStateEnum.Intermission);
    }
}

public class IdleState : IState
{
    public void Enter()
    {
    }

    public void Exit()
    {
    }

    public void ServerUpdate() {
        if (playersOnServer.Any(p => p.IsReady))
        {
            TransitionToIntermission();
        }
    }

    void TransitionToIntermission() {
        ServerStateMachine.Singleton.ChangeState(ServerStateEnum.Intermission);    
    }
}

public class IntermissionState : IState
{
    public void Enter()
    {
#if UNITY_EDITOR
        intermissionSecondsRemaining = intermissionTimerSecondsEditor;
#else
        intermissionSecondsRemaining = ReadyPlayers.Count > 1 ? raceTimerSeconds : raceTimerSecondsSinglePlayer;
#endif
        StartCoroutine(IntermissionTimer());

        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }
}

public class ServerStateMachine : NetworkBehaviour
{
    [SyncVar(hook = nameof(ChangeState))] ServerStateEnum stateType = ServerStateEnum.Idle;

    public static ServerStateMachine Singleton;

    /// <summary>
    /// Run when this script is instantiated.
    /// Set up the Singleton variable and ensure only one Server State Machine is
    /// in the scene.
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

    void Start() {
        ChangeState(ServerState.Idle);
    }

    void LateUpdate() {
        if (isServer)
            currentState.ServerUpdate();
    }

    [SyncVar(hook = nameof(ChangeState))] ServerStateEnum stateType;
    IState currentState;

    public void ChangeState(ServerStateEnum stateType)
    {
        currentState?.Exit();

        this.stateType = stateType; // When server changes this line, 
        
        switch (stateType)
        {
            case ServerStateEnum.Race: currentState = new RaceState(); break;
            case ServerStateEnum.Intermission: currentState = new IntermissionState(); break;
            case ServerStateEnum.Idle: currentState = new IdleState(); break;
            default: Debug.LogError("Invalid State"); break;
        }

        currentState.Enter();
    }
}

