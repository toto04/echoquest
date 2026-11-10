// Records what the listener hears (after spatialisation and reverb) to a stereo WAV file,
// to share a demo or to check the left/right placement. F9 starts and stops.
// Files go to <project>/Recordings/, which git ignores.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoQuest.Story
{
    [RequireComponent(typeof(AudioListener))]
    public class ListenerRecorder : MonoBehaviour
    {
        public bool IsRecording => m_Recording;
        public string LastFile { get; private set; } = "";

        private readonly object m_Lock = new object();
        private readonly List<float> m_Samples = new List<float>();
        private volatile bool m_Recording;
        private int m_Channels = 2;
        private int m_SampleRate;

        private void Awake() => m_SampleRate = AudioSettings.outputSampleRate;

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f9Key.wasPressedThisFrame) Toggle();
        }

        public void Toggle()
        {
            if (m_Recording) StopAndSave();
            else StartRecording();
        }

        public void StartRecording()
        {
            lock (m_Lock) m_Samples.Clear();
            m_Recording = true;
        }

        public string StopAndSave()
        {
            if (!m_Recording) return LastFile;
            m_Recording = false;
            float[] data;
            lock (m_Lock) data = m_Samples.ToArray();

            var dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Recordings"));
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"storia_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
            WriteWav(path, data, m_Channels, m_SampleRate);
            LastFile = path;
            Debug.Log($"[Story] Saved {data.Length / Mathf.Max(1, m_Channels) / (float)m_SampleRate:F1} s to {path}");
            return path;
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!m_Recording) return;
            m_Channels = channels;
            lock (m_Lock) m_Samples.AddRange(data);
        }

        private void OnDisable()
        {
            if (m_Recording) StopAndSave();
        }

        private static void WriteWav(string path, float[] samples, int channels, int rate)
        {
            using var w = new BinaryWriter(File.Create(path));
            int bytes = samples.Length * 2;
            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(36 + bytes);
            w.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            w.Write(16);
            w.Write((short)1);
            w.Write((short)channels);
            w.Write(rate);
            w.Write(rate * channels * 2);
            w.Write((short)(channels * 2));
            w.Write((short)16);
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(bytes);
            foreach (var s in samples) w.Write((short)(Mathf.Clamp(s, -1f, 1f) * 32767));
        }
    }
}
