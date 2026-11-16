using UnityEngine;

namespace EchoQuest.Story
{
    /// <summary>Names over the sound markers in the top-down view. Everything else is in the debug overlay.</summary>
    public class StoryHud : MonoBehaviour
    {
        public StoryPlayer player;
        public ListenerRecorder recorder;
        public string title = "EchoQuest · Capitolo 1";

        private GUIStyle m_Tag;

        private void OnGUI()
        {
            if (player == null) return;
            m_Tag ??= new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };

            var cam = Camera.main;
            if (cam != null)
            {
                foreach (var s in player.Active)
                {
                    if (s == null || !s.Is3D) continue;
                    var p = cam.WorldToScreenPoint(s.transform.position);
                    if (p.z < 0) continue;
                    GUI.Label(new Rect(p.x - 90, Screen.height - p.y - 34, 180, 20), s.Label, m_Tag);
                }
                if (player.listener != null)
                {
                    var p = cam.WorldToScreenPoint(player.listener.position);
                    GUI.Label(new Rect(p.x - 40, Screen.height - p.y + 18, 80, 20), "Tu", m_Tag);
                }
            }
        }
    }
}
