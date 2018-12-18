using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Track Generator - all track generators must inherit from this class.
/// </summary>
public abstract class TrackGeneratorCommon : MonoBehaviour
{
    [SerializeField]
    int m_trackLength;
    [SerializeField]
    float m_trackAltitude;

    public bool IsTrackGenerated { get; private set; }

    /// <summary>
    /// For every physics tick, check if we should generate the track.
    /// </summary>
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Temporary, we will programatically generate the track in the future.
        {
            if (!IsTrackGenerated)
            {
                IReadOnlyList<GameObject> availableTrackPiecePrefabs = Resources.LoadAll<GameObject>("Track Pieces");
                GenerateTrack(m_trackLength, m_trackAltitude, availableTrackPiecePrefabs);
                IsTrackGenerated = true;
            }
        }
    }

    /// <summary>
    /// Generate the track however you like. Track length, altitude are passed in from Unity 
    /// and available track pieces are prefabs loaded from Resources.
    /// </summary>
    /// <param name="trackLength">Number of Track Pieces this track should be composed of.</param>
    /// <param name="trackAltitude">How high the track should be above the ground.</param>
    /// <param name="availableTrackPiecePrefabs">Collection of Track Pieces we can instantiate using the PrefabUtility.</param>
    abstract protected void GenerateTrack(int trackLength, float trackAltitude, IReadOnlyList<GameObject> availableTrackPiecePrefabs);

    #region Helpers

    /// <summary>
    /// Each Track Piece has an ending point called 'Track Piece Link'. This function will return the Transform (position and rotation info) for this link.
    /// </summary>
    /// <param name="trackPiece">Track Piece Game Object</param>
    /// <returns>Track Piece Link Transform</returns>
    protected Transform LoadTrackPieceLinkTransform(GameObject trackPiece)
    {
        Transform tracePieceLinkTransform = trackPiece.transform.Find("Track Piece Link");

        if (tracePieceLinkTransform == null)
        {
            Debug.LogError("Track Piece Failure - Unable to load the Track Piece Link from the specified Track Piece. " +
                "Every Track Piece prefab requires a child game object called 'Track Piece Link' which provides information on where to attach the next Track Piece.");
        }

        return tracePieceLinkTransform;
    }

    #endregion
}
