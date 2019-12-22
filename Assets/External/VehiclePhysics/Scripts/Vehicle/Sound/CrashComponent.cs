using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace NWH.VehiclePhysics
{
    /// <summary>
    /// Sound of vehicle crashing into an object.
    /// Supports multiple audio clips of which one will be chosen at random each time this effect is played.
    /// </summary>
    [System.Serializable]
    public class CrashComponent : SoundComponent
    {
        public override void Initialize(VehicleController vc, AudioMixerGroup amg)
        {
            this.vc = vc;
            this.audioMixerGroup = amg;

            if (Clip != null)
            {
                Source = vc.gameObject.AddComponent<AudioSource>();
                vc.sound.SetAudioSourceDefaults(Source, false, false);
                RegisterSources();
            }

            vc.damage.OnCollision.AddListener(Play);
        }

        public override void Update() { }

        public void Play(Collision collision)
        {
            if (Clips.Count == 0) return;
            if (collision == null) return;

            // Do not play if rim collider was hit
            ContactPoint[] contactPoints = collision.contacts;
            for (int i = 0; i < contactPoints.Length; i++)
            {
                if (contactPoints[i].thisCollider.name == "RimCollider")
                {
                    return;
                }
            }

            float collisionMagnitude = Mathf.Abs(vc.Acceleration.magnitude);
            Source.clip = RandomClip;
            SetVolume(Mathf.Clamp01(collisionMagnitude / 2000f) * volume);
            Source.pitch = Random.Range(0.6f, 1.4f);
            Source.Play();
        }
    }
}

