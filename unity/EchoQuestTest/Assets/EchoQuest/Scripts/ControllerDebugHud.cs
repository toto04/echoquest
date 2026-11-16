using System.Text;
using EchoQuest.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoQuest
{
    /// <summary>Text for the overlay's Pad tab. The component only marks the scene's critter.</summary>
    public class ControllerDebugHud : MonoBehaviour
    {
        public CritterController critter;
        public bool showDeviceList = true;

        /// <summary>Call from Update or LateUpdate, not OnGUI: in the Editor, device controls read from
        /// OnGUI can return a different (stale) state buffer than the one the game sees.</summary>
        public static string BuildText(StringBuilder sb, CritterController critter, bool showDeviceList)
        {
            sb.Clear();

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
            if (critter != null) sb.AppendLine("A = colour, B = hop, R or hold A+B 1 s = recenter yaw");

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
