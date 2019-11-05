using Mirror;
using Racerr.MultiplayerService;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.Track
{
    /// <summary>
    /// Track Generator - all track generators must inherit from this class.
    /// </summary>
    public abstract class TrackGeneratorCommon : NetworkBehaviour
    {
        [SerializeField] int trackLength = 50;

        public static TrackGeneratorCommon Singleton;
        public bool IsTrackGenerated { get; private set; }
        public bool IsTrackGenerating { get; private set; }
        public List<GameObject> GeneratedTrackPieces { get; } = new List<GameObject>();

        // Create a list of all checkpoints in race using the Generated track pices.
        // We assume all track pieces are either Checkpoint or FinishingLineCheckpoint.
        public GameObject[] CheckpointsInRace { get; private set; }

        /// <summary>
        /// Run when this script is instantiated.
        /// Set up the Singleton variable and ensure only one track generator is created
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
                Debug.LogError("You can only have one track generator in the scene. The extra track generator has been destroyed.");
                Destroy(this);
            }
        }

        /// <summary>
        /// Generate the track for all players on the server.
        /// </summary>
        public void GenerateIfRequired(IReadOnlyCollection<Player> playersToSpawn)
        {
            if (isServer && !IsTrackGenerated)
            {
                IsTrackGenerating = true;
                IReadOnlyList<GameObject> availableTrackPiecePrefabs = Resources.LoadAll<GameObject>("Track Pieces");
                StartCoroutine(GenerateTrack(trackLength, availableTrackPiecePrefabs, playersToSpawn));
            }
        }

        /// <summary>
        /// Called on track generation finish by subclass track generators.
        /// </summary>
        public void FinishTrackGeneration()
        {
            IsTrackGenerating = false;
            IsTrackGenerated = true;

            CheckpointsInRace = GeneratedTrackPieces.Select(trackPiece =>
            {
                GameObject result = trackPiece.transform.Find(TrackPieceComponent.Checkpoint)?.gameObject;

                if (result == null)
                {
                    // Special case for the finishing line, it has a different label.
                    result = trackPiece.transform.Find(TrackPieceComponent.FinishLineCheckpoint).gameObject;
                }

                return result;
            }).ToArray();
        }

        /// <summary>
        /// Destroy the track for all players on the server.
        /// </summary>
        public void DestroyIfRequired()
        {
            if (isServer && IsTrackGenerated)
            {
                GeneratedTrackPieces.ForEach(NetworkServer.Destroy);
                GeneratedTrackPieces.RemoveAll(_ => true);
                IsTrackGenerated = false;
                CheckpointsInRace = null;
            }
        }

        /// <summary>
        /// Generate the track however you like. Track length are passed in from Unity
        /// and available track pieces are prefabs loaded from Resources.
        /// </summary>
        /// <param name="trackLength">Number of Track Pieces this track should be composed of.</param>
        /// <param name="availableTrackPiecePrefabs">Collection of Track Pieces we can Instantiate.</param>
        /// <returns>IEnumerator for Unity coroutine, so that track generation can be done concurrently with main thread (useful for calculating collisions).</returns>
        abstract protected IEnumerator GenerateTrack(int trackLength, IReadOnlyList<GameObject> availableTrackPiecePrefabs, IReadOnlyCollection<Player> playersToSpawn);

        /// <summary>
        /// Each Track Piece has an ending point called 'Link'. This function will return the Transform (position and rotation info) for this link.
        /// </summary>
        /// <param name="trackPiece">Track Piece Game Object</param>
        /// <returns>Track Piece Link Transform</returns>
        protected Transform LoadTrackPieceLinkTransform(GameObject trackPiece)
        {
            Transform tracePieceLinkTransform = trackPiece.transform.Find(TrackPieceComponent.Link);

            if (tracePieceLinkTransform == null)
            {
                Debug.LogError("Track Piece Failure - Unable to load the Track Piece Link from the specified Track Piece. " +
                    $"Every Track Piece prefab requires a child game object called '{ TrackPieceComponent.Link }' which provides information on where to attach the next Track Piece.");
            }

            return tracePieceLinkTransform;
        }
    }
}