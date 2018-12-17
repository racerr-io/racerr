using UnityEditor;
using UnityEngine;

public class RandomTrackGenerator : TrackGeneratorCommon
{
    [SerializeField]
    GameObject m_firstTrack; // Temporary, we will programatically generate the first track in the future.

    protected override void GenerateTrack(int trackLength, float trackAltitude)
    {
        string[] availableTracks = new[]
        {
            "Track-Straight",
            "Track-ShortRight",
            "Track-SharpLeft"
        };

        var currentTrack = m_firstTrack;

        for (var i = 0; i < trackLength; i++)
        {
            var newTrackName = availableTracks[Random.Range(0, availableTracks.Length)];

            GameObject newTrack;
            Transform roadLinkTransform;

            if ((roadLinkTransform = LoadRoadLinkTransform(currentTrack)) == null)
            {
                break;
            }
            else if ((newTrack = LoadTrackFromResources(newTrackName)) == null)
            {
                continue;
            }

            newTrack.name = $"Auto Generated Track Piece {i + 1} ({newTrackName})";
            var newTrackRotation = roadLinkTransform.rotation.eulerAngles;
            var currentTrackRotation = currentTrack.transform.rotation.eulerAngles;
            newTrackRotation.x = currentTrackRotation.x;
            newTrackRotation.z = currentTrackRotation.z;

            newTrack.transform.rotation = Quaternion.Euler(newTrackRotation);
            newTrack.transform.position = new Vector3(roadLinkTransform.position.x, trackAltitude, roadLinkTransform.position.z);

            trackAltitude += 0.001f; // Tracks overlap, so we want one to be slightly above the other.
            currentTrack = PrefabUtility.InstantiatePrefab(newTrack) as GameObject;
        }
    }

    GameObject LoadTrackFromResources(string trackName)
    {
        const string trackFolderInResources = "Tracks/";
        var newTrack = Resources.Load<GameObject>(trackFolderInResources + trackName);

        if (newTrack == null)
        {
            Debug.LogError("Track Generation Error - Unable to load randomly selected track. Is the name of the track spelt correctly and placed in the Tracks folder in Resources?");
        }

        return newTrack;
    }

    Transform LoadRoadLinkTransform(GameObject track)
    {
        var roadLinkTransform = track.transform.Find("Road Link");

        if (roadLinkTransform == null)
        {
            Debug.LogError("Track Generation Failed - Unable to load the Road Link from the current track. " +
                "Every track prefab requires a child game object called 'Road Link' which provides information on where to attach the next track.");
        }

        return roadLinkTransform;
    }
}
