using UnityEngine;
using Random = UnityEngine.Random;

namespace Racerr.Car.Core
{
    /// <summary>
    /// This script reads some of the car's current properties and plays sounds accordingly.
    /// The engine sound can be a simple single clip which is looped and pitched, or it
    /// can be a crossfaded blend of four clips which represent the timbre of the engine
    /// at different RPM and Throttle state.
    /// the engine clips should all be a steady pitch, not rising or falling.
    /// when using four channel engine crossfading, the four clips should be:
    /// lowAccelClip : The engine at low revs, with throttle open (i.e. begining acceleration at very low speed)
    /// highAccelClip : Thenengine at high revs, with throttle open (i.e. accelerating, but almost at max speed)
    /// lowDecelClip : The engine at low revs, with throttle at minimum (i.e. idling or engine-braking at very low speed)
    /// highDecelClip : Thenengine at high revs, with throttle at minimum (i.e. engine-braking at very high speed)
    /// For proper crossfading, the clips pitches should all match, with an octave offset between low and high.
    /// </summary>
    [RequireComponent(typeof (CarController))]
    public class CarAudio : MonoBehaviour
    {
        public enum EngineAudioOptions // Options for the engine audio
        {
            Simple, // Simple style audio
            FourChannel // four Channel audio
        }

        [SerializeField] EngineAudioOptions m_EngineSoundStyle = EngineAudioOptions.FourChannel;// Set the default audio options to be four channel
        [SerializeField] AudioClip m_LowAccelClip;                                              // Audio clip for low acceleration
        [SerializeField] AudioClip m_LowDecelClip;                                              // Audio clip for low deceleration
        [SerializeField] AudioClip m_HighAccelClip;                                             // Audio clip for high acceleration
        [SerializeField] AudioClip m_HighDecelClip;                                             // Audio clip for high deceleration
        [SerializeField] float m_PitchMultiplier = 1f;                                          // Used for altering the pitch of audio clips
        [SerializeField] float m_LowPitchMin = 1f;                                              // The lowest possible pitch for the low sounds
        [SerializeField] float m_LowPitchMax = 6f;                                              // The highest possible pitch for the low sounds
        [SerializeField] float m_HighPitchMultiplier = 0.25f;                                   // Used for altering the pitch of high sounds
        [SerializeField] float m_MaxRolloffDistance = 500;                                      // The maximum distance where rollof starts to take place
        [SerializeField] float m_DopplerLevel = 1;                                              // The mount of doppler effect used in the audio
        [SerializeField] bool m_UseDoppler = true;                                              // Toggle for using doppler

        AudioSource LowAccel { get; set; } // Source for the low acceleration sounds
        AudioSource LowDecel { get; set; } // Source for the low deceleration sounds
        AudioSource HighAccel { get; set; } // Source for the high acceleration sounds
        AudioSource HighDecel { get; set; } // Source for the high deceleration sounds
        bool StartedSound { get; set; } // flag for knowing if we have started sounds
        CarController CarController { get; set; } // Reference to car we are controlling

        /// <summary>
        /// Setup audio sources.
        /// </summary>
        void StartSound()
        {
            // get the carcontroller ( this will not be null as we have require component)
            CarController = GetComponent<CarController>();

            // setup the simple audio source
            HighAccel = SetUpEngineAudioSource(m_HighAccelClip);

            // if we have four channel audio setup the four audio sources
            if (m_EngineSoundStyle == EngineAudioOptions.FourChannel)
            {
                LowAccel = SetUpEngineAudioSource(m_LowAccelClip);
                LowDecel = SetUpEngineAudioSource(m_LowDecelClip);
                HighDecel = SetUpEngineAudioSource(m_HighDecelClip);
            }

            // flag that we have started the sounds playing
            StartedSound = true;
        }

        /// <summary>
        /// Destroy all audio sources in this object.
        /// </summary>
        void StopSound()
        {
            foreach (var source in GetComponents<AudioSource>())
            {
                Destroy(source);
            }

            StartedSound = false;
        }


        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            // get the distance to main camera
            float camDist = (Camera.main.transform.position - transform.position).sqrMagnitude;

            // stop sound if the object is beyond the maximum roll off distance
            if (StartedSound && camDist > m_MaxRolloffDistance*m_MaxRolloffDistance)
            {
                StopSound();
            }

            // start the sound if not playing and it is nearer than the maximum distance
            if (!StartedSound && camDist < m_MaxRolloffDistance*m_MaxRolloffDistance)
            {
                StartSound();
            }

            if (StartedSound)
            {
                // The pitch is interpolated between the min and max values, according to the car's revs.
                float pitch = ULerp(m_LowPitchMin, m_LowPitchMax, CarController.Revs);

                // clamp to minimum pitch (note, not clamped to max for high revs while burning out)
                pitch = Mathf.Min(m_LowPitchMax, pitch);

                if (m_EngineSoundStyle == EngineAudioOptions.Simple)
                {
                    // for 1 channel engine sound, it's oh so simple:
                    HighAccel.pitch = pitch*m_PitchMultiplier*m_HighPitchMultiplier;
                    HighAccel.dopplerLevel = m_UseDoppler ? m_DopplerLevel : 0;
                    HighAccel.volume = 1;
                }
                else
                {
                    // for 4 channel engine sound, it's a little more complex:

                    // adjust the pitches based on the multipliers
                    LowAccel.pitch = pitch*m_PitchMultiplier;
                    LowDecel.pitch = pitch*m_PitchMultiplier;
                    HighAccel.pitch = pitch*m_HighPitchMultiplier*m_PitchMultiplier;
                    HighDecel.pitch = pitch*m_HighPitchMultiplier*m_PitchMultiplier;

                    // get values for fading the sounds based on the acceleration
                    float accFade = Mathf.Abs(CarController.AccelInput);
                    float decFade = 1 - accFade;

                    // get the high fade value based on the cars revs
                    float highFade = Mathf.InverseLerp(0.2f, 0.8f, CarController.Revs);
                    float lowFade = 1 - highFade;

                    // adjust the values to be more realistic
                    highFade = 1 - ((1 - highFade)*(1 - highFade));
                    lowFade = 1 - ((1 - lowFade)*(1 - lowFade));
                    accFade = 1 - ((1 - accFade)*(1 - accFade));
                    decFade = 1 - ((1 - decFade)*(1 - decFade));

                    // adjust the source volumes based on the fade values
                    LowAccel.volume = lowFade*accFade;
                    LowDecel.volume = lowFade*decFade;
                    HighAccel.volume = highFade*accFade;
                    HighDecel.volume = highFade*decFade;

                    // adjust the doppler levels
                    HighAccel.dopplerLevel = m_UseDoppler ? m_DopplerLevel : 0;
                    LowAccel.dopplerLevel = m_UseDoppler ? m_DopplerLevel : 0;
                    HighDecel.dopplerLevel = m_UseDoppler ? m_DopplerLevel : 0;
                    LowDecel.dopplerLevel = m_UseDoppler ? m_DopplerLevel : 0;
                }
            }
        }


        /// <summary>
        /// Sets up and adds new audio source to the gane object
        /// </summary>
        /// <param name="clip">Audio Clip</param>
        /// <returns>AudioSource object</returns>
        AudioSource SetUpEngineAudioSource(AudioClip clip)
        {
            // create the new audio source component on the game object and set up its properties
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = 0;
            source.loop = true;

            // start the clip from a random point
            source.time = Random.Range(0f, clip.length);
            source.Play();
            source.minDistance = 5;
            source.maxDistance = m_MaxRolloffDistance;
            source.dopplerLevel = 0;
            return source;
        }

        /// <summary>
        /// Unclamped versions of Lerp and Inverse Lerp, to allow value to exceed the from-to range
        /// </summary>
        /// <param name="from">Lower Bound</param>
        /// <param name="to">Upper Bound</param>
        /// <param name="value">Value</param>
        /// <returns>Lerped value</returns>
        float ULerp(float from, float to, float value)
        {
            return (1.0f - value)*from + value*to;
        }
    }
}
