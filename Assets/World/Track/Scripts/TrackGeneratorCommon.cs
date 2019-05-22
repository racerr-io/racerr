using Mirror;
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
        public bool IsTrackGenerated { get; protected set; }
        public List<GameObject> GeneratedTrackPieces { get; } = new List<GameObject>();

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
        public void GenerateIfRequired()
        {
            if (isServer && !IsTrackGenerated)
            {
                IReadOnlyList<GameObject> availableTrackPiecePrefabs = Resources.LoadAll<GameObject>("Track Pieces");
                StartCoroutine(GenerateTrack(trackLength, availableTrackPiecePrefabs));
            }
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
            }
        }

        /// <summary>
        /// Generate the track however you like. Track length are passed in from Unity
        /// and available track pieces are prefabs loaded from Resources.
        /// </summary>
        /// <param name="trackLength">Number of Track Pieces this track should be composed of.</param>
        /// <param name="availableTrackPiecePrefabs">Collection of Track Pieces we can Instantiate.</param>
        /// <returns>IEnumerator for Unity coroutine, so that track generation can be done concurrently with main thread (useful for calculating collisions).</returns>
        abstract protected IEnumerator GenerateTrack(int trackLength, IReadOnlyList<GameObject> availableTrackPiecePrefabs);

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