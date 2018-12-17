using UnityEngine;

public abstract class TrackGeneratorCommon : MonoBehaviour
{
    [SerializeField]
    int m_trackLength;
    [SerializeField]
    float m_trackAltitude;

    public bool IsTrackGenerated { get; private set; }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Temporary, we will programatically generate the track in the future.
        {
            if (!IsTrackGenerated)
            {
                GenerateTrack(m_trackLength, m_trackAltitude);
                IsTrackGenerated = true;
            }
        }
    }

    abstract protected void GenerateTrack(int trackLength, float trackAltitude);
}
