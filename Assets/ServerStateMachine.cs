using Mirror;
using Racerr.MultiplayerService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ServerState
{
    Race,
    Intermission,
    NoPlayers
}

public class ServerStateMachine : NetworkBehaviour
{
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

    [SyncVar(hook = nameof(OnServerStateChanged))] ServerState state = ServerState.NoPlayers;
    public ServerState State => state;

    [Server]
    public void ChangeState(ServerState newState)
    {
        OnServerStateChanged(newState);
    }

    void OnServerStateChanged(ServerState newState)
    {
        switch (state)
        {
            case ServerState.Race: ExitRaceState(); break;
            case ServerState.Intermission: ExitIntermissionState(); break;
            case ServerState.NoPlayers: ExitNoPlayersState(); break;
            default: Debug.LogError("Invalid State"); break;
        }

        state = newState;

        switch (state)
        {
            case ServerState.Race: EnterRaceState(); break;
            case ServerState.Intermission: EnterIntermissionState(); break;
            case ServerState.NoPlayers: EnterNoPlayersState(); break;
            default: Debug.LogError("Invalid State"); break;
        }
    }

    void EnterIntermissionState()
    {
        RaceSessionManager.Singleton.StartIntermissionTimer();
    }

    void ExitIntermissionState()
    {

    }

    void EnterRaceState()
    {

    }

    void ExitRaceState()
    {

    }

    void EnterNoPlayersState()
    {

    }

    void ExitNoPlayersState()
    {

    }
}
