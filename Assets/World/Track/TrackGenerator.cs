using Mirror;
using Racerr.Infrastructure;
using Racerr.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.World.Track
{
    /// <summary>
    /// Track Generator for Racerr, uses smart algorithm to generate a random,
    /// non-colliding track of specified length.
    /// </summary>
    public sealed class TrackGenerator : NetworkBehaviour
    {
        /* Client and Server Properties */
        public static TrackGenerator Singleton;

        [SerializeField] GameObject firstTrackPiecePrefab;
        [SerializeField] GameObject finalTrackPiecePrefab;
        [Min(2)] [SerializeField] int trackLength = 50;

        // Synchronise all Generated Track Pieces to all clients
        // Purpose of the synchronisation is to allow clients to update their camera.
        // Must declare empty class and readonly field as dictated by Mirror.
        public class SyncListGameObject : SyncList<GameObject> { }
        readonly SyncListGameObject generatedTrackPieces = new SyncListGameObject();
        public SyncListGameObject GeneratedTrackPieces => generatedTrackPieces;


        /* Server Only Properties */
        public bool IsTrackGenerated { get; private set; }
        public bool IsTrackGenerating { get; private set; }

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
        /// Destroy the track for all players on the server.
        /// </summary>
        public void DestroyIfRequired()
        {
            if (isServer && IsTrackGenerated)
            {
                foreach (GameObject trackPiece in GeneratedTrackPieces)
                {
                    NetworkServer.Destroy(trackPiece);
                }

                GeneratedTrackPieces.Clear();
                IsTrackGenerated = false;
                CheckpointsInRace = null;
            }
        }

        /// <summary>
        /// Generate tracks by getting the first track piece, then grabbing a random track piece from resources and joining
        /// it together. Track pieces are moved and rotated to the position of the 'Link' on the previous Track Piece.
        /// </summary>
        /// <param name="trackLength">Number of Track Pieces this track should be composed of.</param>
        /// <param name="availableTrackPiecePrefabs">Collection of Track Pieces we can Instantiate.</param>
        /// <returns>IEnumerator for Unity coroutine, so that we can WaitForFixedUpdate() to check if a track is colliding with another one every time we instantiate a new track.</returns>
        IEnumerator GenerateTrack(int trackLength, IReadOnlyList<GameObject> availableTrackPiecePrefabs, IReadOnlyCollection<Player> playersToSpawn)
        {
            GameObject origin = new GameObject("Temporary Origin for Random Track Generator");
            GameObject currentTrackPiece = firstTrackPiecePrefab;
            int numTracks = 0;

            // Stores a validity map for the current track marked by numTrack index, where all of the possible track piece candidates are either valid or invalid.
            bool[,] validAvailableTracks = new bool[trackLength, availableTrackPiecePrefabs.Count];
            for (int numTracksIndex = 0; numTracksIndex < trackLength; numTracksIndex++)
            {
                for (int candidateTrackPiece = 0; candidateTrackPiece < availableTrackPiecePrefabs.Count; candidateTrackPiece++)
                {
                    validAvailableTracks[numTracksIndex, candidateTrackPiece] = true;
                }
            }

            while (numTracks < trackLength)
            {
                // Compile a list of valid track piece options.
                List<int> validTrackOptions = new List<int>();
                for (int candidateTrackPiece = 0; candidateTrackPiece < availableTrackPiecePrefabs.Count; candidateTrackPiece++)
                {
                    if (validAvailableTracks[numTracks, candidateTrackPiece] == true && IsSameTrackPieceStyle(availableTrackPiecePrefabs[candidateTrackPiece]))
                    {
                        validTrackOptions.Add(candidateTrackPiece);
                    }
                }

                // BACKTRACK
                // Check if there exists any valid track pieces to choose from. If not, delete the recently placed piece.
                if (validTrackOptions.Count == 0)
                {
                    // All track options for the current track piece are exhausted with no valid tracks.
                    // Must backtrack from the current track piece by destroying the current track piece.
                    NetworkServer.Destroy(currentTrackPiece);
                    Destroy(currentTrackPiece);
                    GeneratedTrackPieces.RemoveAt(GeneratedTrackPieces.Count - 1);
                    currentTrackPiece = GeneratedTrackPieces[GeneratedTrackPieces.Count - 1];
                    // Reset validAvailableTracks memory of this track's options for the future track pieces to use this space.
                    for (int candidateTrackPiece = 0; candidateTrackPiece < availableTrackPiecePrefabs.Count; candidateTrackPiece++)
                    {
                        validAvailableTracks[numTracks, candidateTrackPiece] = true;
                    }
                    numTracks--;
                    continue;
                }

                Transform trackPieceLinkTransform;
                GameObject newTrackPiecePrefab;

                if (numTracks == 0)
                {
                    newTrackPiecePrefab = firstTrackPiecePrefab;
                    trackPieceLinkTransform = origin.transform;
                }
                else if (numTracks == trackLength - 1)
                {
                    // We want to force the algorithm to place the Final Track Piece only, so mark every other track piece as invalid.
                    // This way, if the Final Track Piece cannot be placed, the algorithm will backtrack.
                    for (int candidateTrackPiece = 0; candidateTrackPiece < availableTrackPiecePrefabs.Count; candidateTrackPiece++)
                    {
                        validAvailableTracks[numTracks, candidateTrackPiece] = false;
                    }

                    // If the Final Track Piece doesn't have the same track piece style as the previously placed track,
                    // then it cannot be placed, and we must backtrack.
                    if (!IsSameTrackPieceStyle(finalTrackPiecePrefab))
                    {
                        // We marked every track as invalid above, so backtracking will occur on the next iteration.
                        continue;
                    }

                    newTrackPiecePrefab = finalTrackPiecePrefab;
                    trackPieceLinkTransform = LoadTrackPieceLinkTransform(currentTrackPiece);
                }
                else
                {
                    int randomTrack = validTrackOptions[Random.Range(0, validTrackOptions.Count)];
                    newTrackPiecePrefab = availableTrackPiecePrefabs[randomTrack];
                    validAvailableTracks[numTracks, randomTrack] = false;
                    trackPieceLinkTransform = LoadTrackPieceLinkTransform(currentTrackPiece);
                }

                if (trackPieceLinkTransform == null)
                {
                    Debug.LogError("An error has occurred loading the track piece link during track generation for this track piece.");
                    break;
                }

                GameObject newTrackPiece = Instantiate(newTrackPiecePrefab);
                newTrackPiece.name = $"Auto Generated Track Piece { numTracks + 1 } ({ newTrackPiecePrefab.name })";
                newTrackPiece.transform.position = trackPieceLinkTransform.transform.position;
                newTrackPiece.transform.rotation *= trackPieceLinkTransform.rotation;

                // Spawn the players cars onto the starting piece of the track
                if (numTracks == 0)
                {
                    Transform startLine = newTrackPiece.transform.Find(GameObjectIdentifiers.StartLine);
                    if (startLine == null)
                    {
                        Debug.LogError($"First Track Piece must have a GameObject named { GameObjectIdentifiers.StartLine } which marks the starting line.");
                        break;
                    }

                    Vector3 firstCarStartLineDisplacement = new Vector3(0, 0.1f, -15);
                    Vector3 gridStartPosition = startLine.position + firstCarStartLineDisplacement;
                    Vector3 distanceBetweenCars = new Vector3(0, 0, 10);
                    foreach (Player player in playersToSpawn.Where(player => player != null))
                    {
                        player.CreateCarForPlayer(gridStartPosition);
                        gridStartPosition -= distanceBetweenCars;
                        yield return new WaitForFixedUpdate();
                    }
                }

                // Wait for next physics calculation so that Track Piece Collision Detector works properly.
                yield return new WaitForSeconds(0.15f);

                if (newTrackPiece.GetComponent<TrackGeneratorCollisionDetector>().IsValidTrackPlacementUponConnection)
                {
                    NetworkServer.Spawn(newTrackPiece);
                    GeneratedTrackPieces.Add(newTrackPiece);
                    currentTrackPiece = newTrackPiece;
                    numTracks++;
                }
                else
                {
                    Destroy(newTrackPiece);
                }
            }

            // By default track piece props are kinematic so they don't fall over when track pieces collide.
            // Once done we should set kinematic to false so cars can collide into them.
            foreach (GameObject trackPiece in GeneratedTrackPieces)
            {
                TrackPieceManager trackPieceState = trackPiece.GetComponent<TrackPieceManager>();
                trackPieceState.MakeReadyForRace();
                trackPieceState.MakePropsNonKinematic();
            }

            if (finalTrackPiecePrefab.transform.Find(GameObjectIdentifiers.FinishLineCheckpoint) == null)
            {
                Debug.LogError($"Final Track Piece Prefab must have a GameObject named { GameObjectIdentifiers.FinishLineCheckpoint } in order for the finish line to function.");
            }

            // Cleanup and finish track generation
            Destroy(origin);
            IsTrackGenerating = false;
            IsTrackGenerated = true;

            CheckpointsInRace = GeneratedTrackPieces.Select(trackPiece =>
            {
                GameObject result = trackPiece.transform.Find(GameObjectIdentifiers.Checkpoint)?.gameObject;

                if (result == null)
                {
                    // Special case for the finishing line, it has a different label.
                    result = trackPiece.transform.Find(GameObjectIdentifiers.FinishLineCheckpoint).gameObject;
                }

                return result;
            }).ToArray();
        }

        bool IsSameTrackPieceStyle(GameObject candidateTrackPiece)
        {
            if (GeneratedTrackPieces.Count == 0)
            {
                // First track cannot be a transition piece.
                return !candidateTrackPiece.name.Contains("Transition");
            }
            else
            {
                // Highway_RoadTransition
                //   ^To     ^From
                // So this is a Road -> Highway transition
                // The Highway comes first as the transition piece will naturally be of a highway style
                string previousTrackStyle = GeneratedTrackPieces[GeneratedTrackPieces.Count - 1].tag;
                return candidateTrackPiece.CompareTag(previousTrackStyle) && !candidateTrackPiece.name.Contains("Transition")
                    || !candidateTrackPiece.CompareTag(previousTrackStyle) && candidateTrackPiece.name.Contains(previousTrackStyle + "Transition");
            }
        }

        /// <summary>
        /// Each Track Piece has an ending point called 'Link'. This function will return the Transform (position and rotation info) for this link.
        /// </summary>
        /// <param name="trackPiece">Track Piece Game Object</param>
        /// <returns>Track Piece Link Transform</returns>
        Transform LoadTrackPieceLinkTransform(GameObject trackPiece)
        {
            Transform tracePieceLinkTransform = trackPiece.transform.Find(GameObjectIdentifiers.Link);

            if (tracePieceLinkTransform == null)
            {
                Debug.LogError("Track Piece Failure - Unable to load the Track Piece Link from the specified Track Piece. " +
                    $"Every Track Piece prefab requires a child game object called '{ GameObjectIdentifiers.Link }' which provides information on where to attach the next Track Piece.");
            }

            return tracePieceLinkTransform;
        }
    }
}