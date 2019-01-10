using System.Collections;
using UnityEngine;

namespace Racerr.Car.Core
{
    /// <summary>
    /// Wheels Effects for skid trails.
    /// </summary>
    [RequireComponent(typeof (AudioSource))]
    public class WheelEffects : MonoBehaviour
    {
        [SerializeField] Transform m_SkidTrailPrefab;

        Transform SkidTrailsDetachedParent { get; set; }
        ParticleSystem SkidParticles { get; set; }
        public bool IsSkidding { get; private set; }
        public bool PlayingAudio { get; private set; }
        AudioSource AudioSource { get; set; }
        Transform SkidTrail { get; set; }
        WheelCollider WheelCollider { get; set; }

        /// <summary>
        /// Initialise the wheel effects
        /// </summary>
        void Start()
        {
            SkidParticles = transform.root.GetComponentInChildren<ParticleSystem>();

            if (SkidParticles == null)
            {
                Debug.LogWarning("No particle system found on car to generate smoke particles", gameObject);
            }
            else
            {
                SkidParticles.Stop();
            }

            WheelCollider = GetComponent<WheelCollider>();
            AudioSource = GetComponent<AudioSource>();
            PlayingAudio = false;

            if (SkidTrailsDetachedParent == null)
            {
                SkidTrailsDetachedParent = new GameObject("Skid Trails - Detached").transform;
            }
        }

        /// <summary>
        /// Emit some smoke.
        /// </summary>
        public void EmitSmoke()
        {
            SkidParticles.transform.position = transform.position - transform.up*WheelCollider.radius;
            SkidParticles.Emit(1);
            if (!IsSkidding)
            {
                StartCoroutine(StartSkidTrail());
            }
        }

        /// <summary>
        /// Play Audio from the Audio Source
        /// </summary>
        public void PlayAudio()
        {
            AudioSource.Play();
            PlayingAudio = true;
        }

        /// <summary>
        /// Stop playing Audio from the Audio Source
        /// </summary>
        public void StopAudio()
        {
            AudioSource.Stop();
            PlayingAudio = false;
        }

        /// <summary>
        /// Make Skid Trails
        /// </summary>
        /// <returns>Null</returns>
        public IEnumerator StartSkidTrail()
        {
            IsSkidding = true;
            SkidTrail = Instantiate(m_SkidTrailPrefab);
            while (SkidTrail == null)
            {
                yield return null;
            }
            SkidTrail.parent = transform;
            SkidTrail.localPosition = -Vector3.up*WheelCollider.radius;
        }

        /// <summary>
        /// Stop making Skid Trails
        /// </summary>
        public void EndSkidTrail()
        {
            if (!IsSkidding)
            {
                return;
            }
            IsSkidding = false;
            SkidTrail.parent = SkidTrailsDetachedParent;
            Destroy(SkidTrail.gameObject, 10);
        }
    }
}
