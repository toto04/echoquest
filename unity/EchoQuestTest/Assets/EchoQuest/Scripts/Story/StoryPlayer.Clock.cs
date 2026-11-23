// Development playback speed (x0.5 ... x3). The story runs on its own clock, Now, which follows the audio
// clock (AudioSettings.dspTime) scaled by the speed; every source plays with pitch = speed, so the audio
// and the clock stay in step and battle bars, rhythms and presses keep their musical meaning. At x1 Now
// equals the audio clock. Voices get higher at x2: this is a tool for moving through the story, not a
// setting for children.
using UnityEngine;

namespace EchoQuest.Story
{
    public partial class StoryPlayer
    {
        public static readonly float[] SpeedSteps = { 0.5f, 1f, 1.5f, 2f, 3f };

        [Header("Development")]
        [Tooltip("Playback speed for development: audio pitch and every story wait scale together.")]
        [Range(0.5f, 3f)] public float playbackSpeed = 1f;

        private double m_ClockDsp, m_ClockStory;
        private float m_ClockSpeed = 1f;

        /// <summary>Story time in seconds: the audio clock scaled by the playback speed.</summary>
        public double Now => m_ClockStory + (AudioSettings.dspTime - m_ClockDsp) * m_ClockSpeed;

        /// <summary>The playback speed in effect.</summary>
        public float Speed => m_ClockSpeed;

        /// <summary>The audio-clock time at which story time <paramref name="storyTime"/> falls.</summary>
        public double ToDsp(double storyTime) => m_ClockDsp + (storyTime - m_ClockStory) / m_ClockSpeed;

        public void SetSpeed(float speed)
        {
            speed = Mathf.Clamp(speed, 0.5f, 3f);
            playbackSpeed = speed;
            if (Mathf.Approximately(speed, m_ClockSpeed)) return;
            m_ClockStory = Now;
            m_ClockDsp = AudioSettings.dspTime;
            m_ClockSpeed = speed;
            foreach (var source in Active) source.ApplySpeed();
            RebaseInteractionClock();
            LogEvent($"speed x{speed:0.##}");
        }

        private void LateUpdate()
        {
            // The inspector field can change the speed too.
            if (!Mathf.Approximately(playbackSpeed, m_ClockSpeed)) SetSpeed(playbackSpeed);
        }
    }
}
