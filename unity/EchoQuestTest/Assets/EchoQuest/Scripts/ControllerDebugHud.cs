// On-screen debug readout for the controller test scene (IMGUI, no setup needed).
using System.Text;
using EchoQuest.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoQuest
{
    public class ControllerDebugHud : MonoBehaviour
    {
        public CritterController critter;
        public bool showDeviceList = true;

        private GUIStyle m_Style;
        private GUIStyle m_Box;
        private readonly StringBuilder m_Sb = new StringBuilder(1024);
        private string m_Text = "";

        // Build the text in LateUpdate (after the critter updated): in the Editor, reading device controls from OnGUI can
        // return a different (stale) state buffer than the one the game sees.
        private void LateUpdate() => m_Text = BuildText();

        private void OnGUI()
        {
            if (m_Style == null)
            {
                m_Style = new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true, wordWrap = false };
                m_Box = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 8, 8) };
            }

            // Scale with the window so it stays readable at any Game view size.
            var scale = Mathf.Max(0.6f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            var content = new GUIContent(m_Text);
            var size = m_Style.CalcSize(content);
            var rect = new Rect(12, 12, size.x + 20, size.y + 16);
            GUI.Box(rect, GUIContent.none, m_Box);
            GUI.Label(new Rect(rect.x + 10, rect.y + 8, size.x, size.y), content, m_Style);
        }

        private string BuildText()
        {
            var sb = m_Sb;
            sb.Clear();
            sb.AppendLine("<b>EchoQuest controller test</b>");

            var c = EchoQuestController.current;
            if (c != null)
            {
                sb.Append("<color=#7CFC00>Controller: ").Append(c.description.product).Append("</color>")
                  .Append(c.enabled ? "" : " <color=#FF6060>(disabled)</color>").AppendLine();
                sb.AppendFormat("Reports: {0}  ({1:F0}/s)\n", c.reportCount, c.reportsPerSecond);
                sb.Append("Raw: ").AppendLine(c.LastRawReportHex());
                sb.AppendFormat("Buttons: 0x{0:X4}   hat: {1}\n", c.buttonMask, c.hat.ReadValue());
                sb.AppendFormat("Device yaw {0,7:F1}  pitch {1,6:F1}  roll {2,7:F1}\n",
                    c.yawDegrees, c.pitchDegrees, c.rollDegrees);
            }
            else
            {
                sb.AppendLine("<color=#FFB000>Controller: not found, using keyboard</color>");
                sb.AppendLine("  arrows = yaw/pitch, Q/E = roll, 1 = A, 2 = B");
            }

            if (critter != null)
            {
                sb.AppendFormat("Critter yaw {0,7:F1}  pitch {1,6:F1}  roll {2,7:F1}   (yaw offset {3:F1})\n",
                    critter.yawDeg, critter.pitchDeg, critter.rollDeg, critter.yawOffsetDeg);
                sb.Append("A: ").Append(critter.buttonAHeld ? "<b>DOWN</b>" : "up  ")
                  .Append("   B: ").Append(critter.buttonBHeld ? "<b>DOWN</b>" : "up").AppendLine();
            }
            sb.AppendLine("A = colour, B = hop, R or hold A+B 1 s = recenter yaw");

            if (showDeviceList)
            {
                sb.AppendLine("<b>Input devices</b>");
                foreach (var d in InputSystem.devices)
                {
                    var desc = d.description;
                    sb.Append("  ").Append(d.layout).Append(" | ").Append(d.name);
                    if (!string.IsNullOrEmpty(desc.product)) sb.Append(" | ").Append(desc.product);
                    if (!string.IsNullOrEmpty(desc.interfaceName)) sb.Append(" | ").Append(desc.interfaceName);
                    sb.AppendLine();
                }
            }
            return sb.ToString().TrimEnd();
        }
    }
}
