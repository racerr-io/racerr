﻿using Racerr.Gameplay.Car;
using Racerr.Infrastructure.Server;
using UnityEngine;

namespace Racerr.World.Track
{
    /// <summary>
    /// A simple script which detects when cars move through the checkpoint (box collider)
    /// and notifies the Server Race State, so it can keep track of car positions for the leaderboard.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class TrackPieceCheckpointDetector : MonoBehaviour
    {
        ServerRaceState serverRaceState;

        /// <summary>
        /// When track is spawned, cache the Server Race State.
        /// </summary>
        void Start()
        {
            serverRaceState = FindObjectOfType<ServerRaceState>();
        }

        /// <summary>
        /// Called when an object passes through the checkpoint at the end of the track, represented by a simple On Trigger Box Collider.
        /// The Server Race State keeps track of the position of the cars during the race, so each track notifies it when a car passes through.
        /// </summary>
        /// <param name="collider">The collider (car) the checkpoint touched.</param>
        void OnTriggerEnter(Collider collider)
        {
            if (collider.CompareTag("Car"))
            {
                serverRaceState.NotifyPlayerPassedThroughCheckpoint(collider.GetComponent<CarManager>().OwnPlayer, gameObject);
            }
        }
    }
}