using Mirror;
using Racerr.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.World.Track
{
    /// <summary>
    /// Manages state of the track and whether it is ready for driving.
    /// </summary>
    public class TrackPieceManager : NetworkBehaviour
    {
        [SerializeField] int propDownForce = 8000;
        IEnumerable<Rigidbody> propRigidBodies = null;
        IEnumerable<Rigidbody> PropRigidBodies => propRigidBodies ?? (propRigidBodies = GetComponentsInChildren<Rigidbody>().Where(p => p != null && p.CompareTag(Tags.Prop)));

        /// <summary>
        /// Called when object is instantiated.
        /// If we are on the client then automatically make the track driveable as server is responsible for track positioning.
        /// </summary>
        void Start()
        {
            if (isClient)
            {
                MakeReadyForRace();
                RemovePhysicsFromProps();
                Destroy(GetComponentInChildren<TrackPieceCheckpointDetector>());
            }
        }

        /// <summary>
        /// Called every physics tick.
        /// Applies down force to all props so they fall faster.
        /// </summary>
        void FixedUpdate()
        {
            if (isServer)
            {
                ApplyDownForceToProps();
            }
        }

        /// <summary>
        /// Make track ready for race by disabling convex mesh colliders and destroying the top level rigidbody, which are only useful for
        /// detecting collisions during track generation. Also destroy the track gen collision detector script on the track,
        /// as we don't need it once the track is generated. Intended to be called after the entire track has been generated.
        /// </summary>
        /// <remarks>
        /// Convex colliders are less detailed but are the only way to detect collisions during track generation. Once track generation is done,
        /// then we need to restore back the detailed nature of the track, so we make the track non-convex. This has the added bonus that the 
        /// physics engine will not detect collisions with convex colliders to save performance. There is also no need for the rigidbody on the track, 
        /// this was only useful for detecting collisions with other tracks during track generation and can be safely removed.
        /// </remarks>
        public void MakeReadyForRace()
        {
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.isKinematic = true; // This is redundant step, but upon setting the mesh collider to non-convex Unity complains as
                                          // the frame has not been updated yet. (Unity does not allow non-convex, non-kinematic rigidbodies
                                          // on a gameobject). 
            Destroy(rigidbody);

            foreach (MeshCollider meshCollider in GetComponentsInChildren<MeshCollider>())
            {
                meshCollider.convex = false;
            }

            Destroy(GetComponent<TrackGeneratorCollisionDetector>());
        }

        /// <summary>
        /// Remove physics from props, so that collisions on props such as street lights and signs are calculated only on the server.
        /// Weird teleporting glitching occurs if we calculate on both client and server.
        /// </summary>
        [Client]
        void RemovePhysicsFromProps()
        {
            foreach (Collider propCollider in GetComponentsInChildren<Collider>().Where(p => p.CompareTag(Tags.Prop)))
            {
                Destroy(propCollider);
            }

            foreach (Rigidbody propRigidBody in PropRigidBodies)
            {
                Destroy(propRigidBody);
            }
        }

        /// <summary>
        /// By default props are kinematic so that during track generation they don't fall over. 
        /// Once track generation is done this function should be called to ensure cars can collide
        /// properly into props.
        /// </summary>
        [Server]
        public void MakePropsNonKinematic()
        {
            foreach (Rigidbody propRigidBody in PropRigidBodies)
            {
                propRigidBody.isKinematic = false;
            }
        }

        /// <summary>
        /// Apply artificial gravity to props so they fall faster.
        /// <remarks>
        /// Please note that props physics are calculated on the server only. There should not
        /// be any prop rigid bodies on the client.
        /// </remarks>
        /// </summary>
        [Server]
        void ApplyDownForceToProps()
        {
            foreach (Rigidbody propRigidBody in PropRigidBodies)
            {
                propRigidBody.AddForce(Vector3.down * propDownForce);
            }
        }
    }
}