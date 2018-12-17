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
    string[] AvailableTrackPieces => new[]
    {
        "TrackPiece-Straight",
        "TrackPiece-ShortRight",
        "TrackPiece-SharpLeft"
    };

    /// <summary>
    /// For every physics tick, check if we should generate the track.
    /// </summary>
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Temporary, we will programatically generate the track in the future.
        {
            if (!IsTrackGenerated)
            {
                GenerateTrack(m_trackLength, m_trackAltitude, AvailableTrackPieces);
                IsTrackGenerated = true;
            }
        }
    }

    /// <summary>
    /// Generate the track however you like. Track length, altitude are passed in from Unity 
    /// and available track pieces are ones available in Resources. Please ensure AvailableTrackPieces is
    /// up to date in TrackGeneratorCommon.
    /// </summary>
    /// <param name="trackLength"></param>
    /// <param name="trackAltitude"></param>
    /// <param name="availableTracks"></param>
    abstract protected void GenerateTrack(int trackLength, float trackAltitude, string[] availableTrackPieces);

    #region Helpers

    /// <summary>
    /// Given a track piece name, e.g. 'Track-Straight', load the prefab from the Resources folder in Unity.
    /// </summary>
    /// <param name="trackPieceName"></param>
    /// <returns> Track Piece Prefab </returns>
    protected GameObject LoadTrackPieceFromResources(string trackPieceName)
    {
        const string trackFolder = "Tracks/";
        GameObject newTrackPiece = Resources.Load<GameObject>(trackFolder + trackPieceName);

        if (newTrackPiece == null)
        {
            Debug.LogError("Track Piece Failure - Unable to load randomly selected track. Is the name of the track spelt correctly and placed in the Tracks folder in Resources?");
        }

        return newTrackPiece;
    }

    /// <summary>
    /// Each Track Piece has an ending point called 'Track Piece Link'. This method will return the Transform for this link.
    /// </summary>
    /// <param name="trackPiece"></param>
    /// <returns> Track Piece Link Transform </returns>
    protected Transform LoadTrackPieceLinkTransform(GameObject trackPiece)
    {
        Transform tracePieceLinkTransform = trackPiece.transform.Find("Track Piece Link");

        if (tracePieceLinkTransform == null)
        {
            Debug.LogError("Track Piece Failure - Unable to load the Track Piece Link from the current track. " +
                "Every track prefab requires a child game object called 'Track Piece Link' which provides information on where to attach the next track.");
        }

        return tracePieceLinkTransform;
    }

    #endregion
}
