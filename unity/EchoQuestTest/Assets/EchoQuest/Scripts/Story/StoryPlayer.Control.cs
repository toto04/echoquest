// Control surface for the debug overlay: jump, section and branch control, on-screen
// buttons, an event log. The keyboard and the controller keep their own paths.
using System.Collections.Generic;
using EchoQuest.Input;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace EchoQuest.Story
{
    public partial class StoryPlayer
    {
        [Header("Debug")]
        [Tooltip("Off = no distance muffling and no filtro_hz.")]
        public bool distanceLowpass = true;
        [Tooltip("Off = beds and tails keep their level under voices.")]
        public bool ducking = true;
        [Tooltip("The listener's head follows the controller's yaw.")]
        public bool headFollowsController;
        public float headYawOffset;

        public const int LogCapacity = 300;
        public readonly List<string> Log = new List<string>();
        /// <summary>What the last timed exercise heard, for the overlay.</summary>
        public string LastAttempt { get; private set; } = "";
        public InteractionSettings CurrentInteraction { get; private set; }

        private string m_StartOverride;
        private bool m_ForcePass;
        private string m_ItemsSection;
        private List<StoryItem> m_SectionItems = new List<StoryItem>();

        /// <summary>Every item of the current section, in script order.</summary>
        public IReadOnlyList<StoryItem> SectionItems
        {
            get
            {
                if (m_ItemsSection != section)
                {
                    m_ItemsSection = section;
                    m_SectionItems = m_Items.FindAll(i => i.Section == section);
                }
                return m_SectionItems;
            }
        }

        public List<string> Sections()
        {
            var list = new List<string>();
            foreach (var item in m_Items)
                if (!list.Contains(item.Section)) list.Add(item.Section);
            return list;
        }

        /// <summary>A section's name for people: p00 = Intro, s03 = Scena 3, e00 = Epilogo; in the other
        /// chapters (t = tamburo, c = campanella) 00 is the Prologo, 99 the Epilogo, NN the Scena NN.</summary>
        public static string SectionName(string section) => section switch
        {
            "p00" => "Intro",
            "e00" => "Epilogo",
            _ when section.Length != 3 || !int.TryParse(section.Substring(1), out int n) => section,
            _ when section[0] == 's' => "Scena " + int.Parse(section.Substring(1)),
            _ when section.EndsWith("00") => "Prologo",
            _ when section.EndsWith("99") => "Epilogo",
            _ => "Scena " + int.Parse(section.Substring(1)),
        };

        public void LogEvent(string message)
        {
            if (Log.Count >= LogCapacity) Log.RemoveAt(0);
            Log.Add($"{Time.unscaledTime,7:F1}  {message}");
        }

        public static string ButtonName(int button) => button switch
        {
            StoryInteractionRules.Yes => "Yes",
            StoryInteractionRules.No => "No",
            _ => button.ToString(),
        };

        /// <summary>Restarts playback from this item. Jumping into a branch selects that branch.</summary>
        public void JumpTo(string id)
        {
            var item = m_Items.Find(i => i.Id == id);
            if (item == null) return;
            section = item.Section;
            LogEvent("jump to " + id);
            Begin(id, string.IsNullOrEmpty(item.Branch) ? BranchChoice : item.Branch);
        }

        public void SetSection(string newSection)
        {
            if (newSection == section) return;
            section = newSection;
            LogEvent("section " + newSection);
            Begin(null, "A");
        }

        public void ForceBranch(string branch)
        {
            BranchChoice = branch;
            LogEvent("branch forced to " + branch);
        }

        /// <summary>Jumps to the item before the current one that would actually play.</summary>
        public void Previous()
        {
            var list = SectionItems;
            int index = Current != null ? m_SectionItems.IndexOf(Current) : list.Count;
            for (int i = index - 1; i >= 0; i--)
            {
                if (!Plays(list[i])) continue;
                JumpTo(list[i].Id);
                return;
            }
        }

        private bool Plays(StoryItem item) =>
            item.Kind != StoryItemKind.Convergence && string.IsNullOrEmpty(item.Inside) &&
            !m_CoveredByPrevious.Contains(item.Id) &&
            (string.IsNullOrEmpty(item.Branch) || item.Branch == BranchChoice);

        public bool HasAudio(StoryItem item) => item.Kind switch
        {
            StoryItemKind.Voice => m_Voices.ContainsKey(item.Id) || m_CoveredByPrevious.Contains(item.Id),
            StoryItemKind.Sound => m_Sounds.ContainsKey(item.Id),
            _ => true,
        };

        /// <summary>A press from an on-screen button: 1-4 are the note buttons, Yes and No the thumb ones.</summary>
        public void PressLogical(int button)
        {
            LogEvent("on-screen " + ButtonName(button));
            if (m_CapturingInteraction)
            {
                if (!Paused)
                    m_InteractionInput.Enqueue((button, m_CaptureStart + (InputState.currentTime - m_CaptureInputStart) * Speed));
            }
            else if (button >= 1 && button <= 4) m_Choice = button - 1;
        }

        /// <summary>Makes the interaction that is waiting for input succeed.</summary>
        public void ForcePass()
        {
            if (m_CapturingInteraction) m_ForcePass = true;
        }

        public void RecenterHead()
        {
            var pad = EchoQuestController.current;
            if (pad != null) headYawOffset = pad.yawDegrees;
            if (listener != null) listener.rotation = Quaternion.identity;
        }
    }
}
