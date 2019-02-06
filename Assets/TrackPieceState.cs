using Mirror;
using System.Linq;
using UnityEngine;

namespace Racerr.Track
{
    /// <summary>
    /// Manages state of the track and whether it is ready for driving.
    /// </summary>
    public class TrackPieceState : NetworkBehaviour
    {
        /// <summary>
        /// Called when object is instantiated.
        /// If we are on the client then automatically make the track driveable as server is responsible for track positioning.
        /// </summary>
        void Start()
        {
            if (isClient)
            {
                MakeDriveable();
                RemovePhysicsFromProps();
            }
        }

        /// <summary>
        /// Make tracks driveable by disabling convex mesh colliders and making them kinematic.
        /// </summary>
        /// <remarks>
        /// Making a track driveable means that it will stay still and have detailed mesh colliders,
        /// but the physics engine will not detect collisions with the track to save performance (since the Mesh Collider is not convex).
        /// The reason for doing this is that we need to make tracks detect collisions during track generation. Once the track is placed
        /// we can make it driveable.
        /// </remarks>
        public void MakeDriveable()
        {
            GetComponent<Rigidbody>().isKinematic = true;

            foreach (MeshCollider meshCollider in GetComponentsInChildren<MeshCollider>())
            {
                meshCollider.convex = false;
            }
        }
        
        /// <summary>
        /// Remove physics from props, so that collisions on props such as street lights and signs are calculated only on the server.
        /// Weird teleporting glitching occurs if we calculate on both client and server.
        /// </summary>
        void RemovePhysicsFromProps()
        {
            foreach (Collider propCollider in GetComponentsInChildren<Collider>().Where(p => p.CompareTag("Prop")))
            {
                Destroy(propCollider);
            }

            foreach (Rigidbody propRigidBody in GetComponentsInChildren<Rigidbody>().Where(p => p.CompareTag("Prop")))
            {
                Destroy(propRigidBody);
            }
        }
    }
}