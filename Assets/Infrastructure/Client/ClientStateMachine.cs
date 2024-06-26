﻿using Racerr.UX.Camera;
using System;
using System.Linq;
using UnityEngine;

namespace Racerr.Infrastructure.Client
{
    /// <summary>
    /// A state machine for the client, with the sole purpose of controlling all aspects of the user interface for the client.
    /// Note that this class destroys itself if started on a device with no graphics card (i.e. the server).
    /// </summary>
    public sealed class ClientStateMachine : MonoBehaviour, IStateMachine
    {
        public static ClientStateMachine Singleton;

        public StateEnum StateType { get; private set; }
        LocalState currentState;

        [SerializeField] PrimaryCamera primaryCamera;
        public PrimaryCamera PrimaryCamera => primaryCamera;

        Player localPlayer;
        public Player LocalPlayer
        {
            get
            {
                if (localPlayer == null)
                {
                    localPlayer = FindObjectsOfType<Player>().SingleOrDefault(player => player.isLocalPlayer);
                }

                return localPlayer;
            }
        }

        /// <summary>
        /// Run when this script is instantiated.
        /// Set up the Singleton variable and ensure only one Client State Machine is in the scene.
        /// </summary>
        void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Debug.LogError("You can only have one Client State Machine in the scene. The extra one has been destroyed.");
                Destroy(this);
            }
        }

        /// <summary>
        /// Entrypoint into Client State Machine. Called after unity initialises all scripts.
        /// Defaults to the Client Start Menu state, which is the first view the user sees when launching the game.
        /// </summary>
        void Start()
        {
            ChangeState(StateEnum.ClientStartMenu);
        }

        /// <summary>
        /// Changes the state of the Client State Machine.
        /// Intended to be PROTECTED - only the Client States should be able to call this from their encapsulated transition methods.
        /// Changes the internal state of the Client State Machine based on the given state type Enum.
        /// </summary>
        /// <param name="stateType">The new state type to be changed to.</param>
        /// <param name="optionalData">Optional data to be passed to the transitioning state.</param>
        public void ChangeState(StateEnum stateType, object optionalData = null)
        {
            if (currentState != null)
            {
                // Only time when the current state will be null is when the client starts.
                currentState.Exit();
                currentState.enabled = false;
            }

            try
            {
                switch (stateType)
                {
                    case StateEnum.Intermission: currentState = GetComponent<ClientIntermissionState>(); break;
                    case StateEnum.Race: currentState = GetComponent<ClientRaceState>(); break;
                    case StateEnum.ClientSpectate: currentState = GetComponent<ClientSpectateState>(); break;
                    case StateEnum.ClientStartMenu: currentState = GetComponent<ClientStartMenuState>(); break;
                    case StateEnum.ClientDeath: currentState = GetComponent<ClientDeathState>(); break;
                    default: throw new InvalidOperationException("Invalid Client ChangeState attempt: " + stateType.ToString());
                }
                SentrySdk.AddBreadcrumb($"Client State Machine for { LocalPlayer?.PlayerName } changed state from { StateType } to { stateType }.");
                StateType = stateType;
            }
            catch (InvalidOperationException e)
            {
                Debug.LogError(e);
            }

            currentState.enabled = true;
            currentState.Enter(optionalData);
        }
    }
}