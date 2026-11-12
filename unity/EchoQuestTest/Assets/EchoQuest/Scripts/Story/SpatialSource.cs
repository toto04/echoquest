using UnityEngine;

namespace EchoQuest.Story
{
    [RequireComponent(typeof(AudioSource))]
    public class SpatialSource : MonoBehaviour
    {
        public AudioSource Source { get; private set; }
        public string Label { get; private set; }
        public bool Is3D { get; private set; }
        public bool IsVoice { get; private set; }
        /// <summary>Multiplies the staging volume; the player lowers it under voices (ducking).</summary>
        public float TargetGain { get; set; } = 1f;

        // Ducking speeds, in gain units per second: duck quickly when someone starts speaking,
        // come back slowly, so short pauses between sentences don't make the music pump.
        private const float DuckSpeed = 3f, ReleaseSpeed = 0.5f;
        private float m_Volume, m_Gain = 1f, m_Fade = 1f, m_FadeSeconds;
        private bool m_FadingOut;

        // Distance darkening: full brightness up to NearDistance, FarCutoffHz at FarDistance and beyond.
        private const float NearDistance = 3f, FarDistance = 25f, FarCutoffHz = 2500f;

        private StoryPlayer m_Owner;
        private Transform m_Listener;
        private AudioLowPassFilter m_Lowpass;
        private float m_FixedCutoff;
        private Vector3 m_From, m_To;
        private float m_MoveSeconds, m_MoveTime;
        private bool m_Moves;
        private double m_ScheduledStart;

        public void Init(StoryPlayer owner, Transform listener, AudioClip clip, StagingEntry staging, string label,
            Material marker, bool isVoice = false, bool loop = false, float startGain = 1f, double? scheduledStart = null)
        {
            m_Owner = owner;
            m_Listener = listener;
            Label = label;
            Is3D = staging.Position.HasValue;
            IsVoice = isVoice;
            m_Volume = staging.Volume;
            m_Gain = TargetGain = startGain;

            Source = GetComponent<AudioSource>();
            Source.clip = clip;
            Source.loop = loop;
            Source.volume = m_Volume * m_Gain;
            Source.playOnAwake = false;
            Source.dopplerLevel = 0f;

            if (Is3D)
            {
                m_From = staging.Position.Value;
                m_To = staging.MoveTo ?? m_From;
                m_Moves = staging.MoveTo.HasValue;
                m_MoveSeconds = staging.MoveSeconds > 0 ? staging.MoveSeconds : Mathf.Max(0.1f, clip.length);
                transform.position = m_From;

                Source.spatialBlend = 1f;
                Source.rolloffMode = AudioRolloffMode.Logarithmic;
                // Full volume within 2.5 m, then -6 dB per doubling of distance.
                Source.minDistance = 2.5f;
                Source.maxDistance = 60f;
                Source.spread = 0f;
                Source.reverbZoneMix = 1f;

                m_Lowpass = gameObject.AddComponent<AudioLowPassFilter>();
                m_FixedCutoff = staging.LowpassHz;
                UpdateLowpass();

                if (marker != null)
                {
                    var dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Destroy(dot.GetComponent<Collider>());
                    dot.name = "Segnaposto";
                    dot.transform.SetParent(transform, false);
                    dot.transform.localScale = Vector3.one * 0.8f;
                    dot.GetComponent<Renderer>().sharedMaterial = marker;
                }
            }
            else
            {
                Source.spatialBlend = 0f;   // the narrator: same level in both ears, no room echo
                Source.reverbZoneMix = 0f;
                transform.position = listener.position;
            }

            m_Owner.Active.Add(this);
            m_ScheduledStart = scheduledStart ?? AudioSettings.dspTime;
            if (scheduledStart.HasValue) Source.PlayScheduled(m_ScheduledStart);
            else Source.Play();
        }

        private void Update()
        {
            if (Source == null || AudioListener.pause) return;
            if (m_Moves)
            {
                m_MoveTime += Time.deltaTime;
                transform.position = Vector3.Lerp(m_From, m_To, Mathf.Clamp01(m_MoveTime / m_MoveSeconds));
            }
            if (m_Lowpass != null) UpdateLowpass();

            float speed = TargetGain < m_Gain ? DuckSpeed : ReleaseSpeed;
            m_Gain = Mathf.MoveTowards(m_Gain, TargetGain, speed * Time.deltaTime);
            if (m_FadingOut)
            {
                m_Fade -= Time.deltaTime / m_FadeSeconds;
                if (m_Fade <= 0f) { Destroy(gameObject); return; }
            }
            Source.volume = m_Volume * m_Gain * m_Fade;

            if (!Source.isPlaying && AudioSettings.dspTime >= m_ScheduledStart) Destroy(gameObject);
        }

        public void Stop()
        {
            if (Source != null) Source.Stop();
            Destroy(gameObject);
        }

        /// <summary>Fades out over <paramref name="seconds"/>, then removes itself.</summary>
        public void FadeOut(float seconds)
        {
            if (seconds <= 0f) { Stop(); return; }
            m_FadingOut = true;
            m_FadeSeconds = seconds;
        }

        private void UpdateLowpass()
        {
            if (m_FixedCutoff > 0) { m_Lowpass.cutoffFrequency = m_FixedCutoff; return; }
            float d = Vector3.Distance(transform.position, m_Listener.position);
            float t = Mathf.InverseLerp(NearDistance, FarDistance, d);
            // Interpolate in log-frequency, which is how we hear pitch.
            m_Lowpass.cutoffFrequency = Mathf.Exp(Mathf.Lerp(Mathf.Log(20000f), Mathf.Log(FarCutoffHz), t));
        }

        private void OnDestroy()
        {
            if (m_Owner != null) m_Owner.Active.Remove(this);
        }
    }
}
