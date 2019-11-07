using Mirror;
using Racerr.MultiplayerService;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Racerr.Track
{
    /// <summary>
    /// A track generator that randomly generates your tracks.
    /// </summary>
    public class RandomTrackGenerator : TrackGeneratorCommon
    {
        [SerializeField] GameObject firstTrackPiece;
        [SerializeField] Transform origin;

        /// <summary>
        /// Generate tracks by getting the first track piece, then grabbing a random track piece from resources and joining
        /// it together. Track pieces are moved and rotated to the position of the 'Link' on the previous Track Piece.
        /// </summary>
        /// <param name="trackLength">Number of Track Pieces this track should be composed of.</param>
        /// <param name="availableTrackPiecePrefabs">Collection of Track Pieces we can Instantiate.</param>
        /// <returns>IEnumerator for Unity coroutine, so that we can WaitForFixedUpdate() to check if a track is colliding with another one every time we instantiate a new track.</returns>
        protected override IEnumerator GenerateTrack(int trackLength, IReadOnlyList<GameObject> availableTrackPiecePrefabs, IReadOnlyCollection<Player> playersToSpawn)
        {
            GameObject currentTrackPiece = firstTrackPiece;
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

                // Check if there exists any valid track pieces to choose from.
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
                    newTrackPiecePrefab = firstTrackPiece;
                    trackPieceLinkTransform = origin;
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
                    Vector3 gridStartPosition = new Vector3(0, 1, 10);

                    foreach (Player player in playersToSpawn.Where(player => player != null))
                    {
                        player.CreateCarForPlayer(gridStartPosition);
                        gridStartPosition += new Vector3(0, 0, 10);
                        yield return new WaitForFixedUpdate();
                    }
                }

                yield return new WaitForSeconds(0.15f); // Wait for next physics calculation so that Track Piece Collision Detector works properly.

                if (newTrackPiece.GetComponent<TrackPieceCollisionDetector>().IsValidTrackPlacementUponConnection)
                {
                    newTrackPiece.transform.position = new Vector3(newTrackPiece.transform.position.x, newTrackPiece.transform.position.y, newTrackPiece.transform.position.z);
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
                TrackPieceState trackPieceState = trackPiece.GetComponent<TrackPieceState>();
                trackPieceState.MakeDriveable();
                trackPieceState.MakePropsNonKinematic();
            }

            // Set last generated track piece's checkpoint to be the ending checkpoint for the race.
            currentTrackPiece.transform.Find(TrackPieceComponent.Checkpoint).name = TrackPieceComponent.FinishLineCheckpoint;

            FinishTrackGeneration();
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
                return previousTrackStyle == candidateTrackPiece.tag && !candidateTrackPiece.name.Contains("Transition")
                    || previousTrackStyle != candidateTrackPiece.tag && candidateTrackPiece.name.Contains(previousTrackStyle + "Transition");
            }
        }
    }
}

