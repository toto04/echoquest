using UnityEngine;

namespace EchoQuest.Story
{
    public class StoryHud : MonoBehaviour
    {
        public StoryPlayer player;
        public ListenerRecorder recorder;
        public string title = "EchoQuest · Capitolo 1";

        private GUIStyle m_Box, m_Label, m_Small, m_Tag;

        private void OnGUI()
        {
            if (player == null) return;
            if (m_Box == null)
            {
                m_Box = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, padding = new RectOffset(12, 12, 10, 10) };
                m_Label = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 15, richText = true };
                m_Small = new GUIStyle(m_Label) { fontSize = 12 };
                m_Tag = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            }

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

            float w = Mathf.Min(560, Screen.width - 24);
            GUILayout.BeginArea(new Rect(12, 12, w, Screen.height - 24));
            GUILayout.BeginVertical(m_Box);
            GUILayout.Label($"<b>{title}</b>  ·  sezione {player.section}  ·  {player.ItemIndex + 1}/{player.ItemCount}", m_Label);
            GUILayout.Label((player.Paused ? "<color=#ffcc66>IN PAUSA</color>  " : "") + player.Status, m_Label);

            var item = player.Current;
            if (item != null)
            {
                string who = string.IsNullOrEmpty(item.Character) ? item.Id : $"{item.Character}  ({item.Id})";
                GUILayout.Label($"<b>{who}</b>", m_Label);
                string text = item.Text.Length > 320 ? item.Text.Substring(0, 320) + "…" : item.Text;
                GUILayout.Label(text, m_Small);
                if (!string.IsNullOrEmpty(player.CurrentClip)) GUILayout.Label("File: " + player.CurrentClip, m_Small);
            }

            int voices = 0;
            foreach (var s in player.Active) if (s != null) voices++;
            GUILayout.Label($"Suoni attivi: {voices}   ·   ramo scelto: {player.BranchChoice}", m_Small);
            if (recorder != null && recorder.IsRecording) GUILayout.Label("<color=#ff6666>● Registrazione in corso (F9 per fermare)</color>", m_Small);
            else if (recorder != null && !string.IsNullOrEmpty(recorder.LastFile)) GUILayout.Label("Ultima registrazione: " + recorder.LastFile, m_Small);
            GUILayout.Label("1–4 note · Y Sì / N No nelle domande · N salta durante il parlato · Spazio pausa/conferma · R ripeti · Backspace da capo · ← → gira la testa · F9 registra", m_Small);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
