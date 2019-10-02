using UnityEngine;

namespace Racerr.UX.Camera
{
    /// <summary>
    /// This script is designed to be placed on the root object of a camera rig,
    /// comprising 3 gameobjects, each parented to the next:
    /// 	Camera Rig
    /// 		Pivot
    /// 			Camera
    /// </summary>
    public abstract class PivotBasedCameraRig : AbstractTargetFollower
    {
        protected Transform Cam { get; set; } // the transform of the camera
        protected Transform Pivot { get; set; } // the point at which the camera pivots around
        protected Vector3 LastTargetPosition { get; set; }

        /// <summary>
        /// Find the camera in the object hierarchy
        /// </summary>
        protected virtual void Awake()
        {
            Cam = GetComponentInChildren<UnityEngine.Camera>().transform;
            Pivot = Cam.parent;
        }
    }
}
