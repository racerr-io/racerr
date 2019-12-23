using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

namespace NWH.VehiclePhysics
{
    /// <summary>
    /// Click-clack of the working blinker.
    /// Accepts two clips, first is for the blinker turning on and the second is for blinker turning off.
    /// </summary>
    [System.Serializable]
    public class BlinkerComponent : SoundComponent
    {
        private bool blinkersActive;
        private bool prevBlinkersActive;

        public override void Initialize(VehicleController vc, AudioMixerGroup amg)
        {
            this.vc = vc;
            this.audioMixerGroup = amg;

            if (Clip != null)
            {
                Source = vc.gameObject.AddComponent<AudioSource>();
                vc.sound.SetAudioSourceDefaults(Source, false, false, volume);
                RegisterSources();
            }
        }

        public override void Update()
        {
            if (Clips.Count > 0)
            {
                prevBlinkersActive = blinkersActive;
                blinkersActive = false;

                if(vc.effects != null && vc.effects.lights != null && vc.effects.lights.leftBlinkers != null && vc.effects.lights.rightBlinkers != null)
                if(vc.effects.lights.leftBlinkers.On || vc.effects.lights.rightBlinkers.On)
                {
                    blinkersActive = true;
                }

                if(!prevBlinkersActive && blinkersActive)
                {
                    Source.clip = Clips[0];
                    Source.Play();
                }
                else if(prevBlinkersActive && !blinkersActive)
                {
                    // Play off clip if available or play the same on clip if not.
                    if(Clips.Count == 2)
                    {
                        Source.clip = Clips[1];
                    }
                    else
                    {
                        Source.clip = Clips[0];
                    }

                    Source.Play();
                }
            }
        }
    }
}
