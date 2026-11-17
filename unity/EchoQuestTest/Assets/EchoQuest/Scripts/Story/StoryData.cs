using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace EchoQuest.Story
{
    public enum StoryItemKind { Voice, Sound, Interaction, Convergence }

    public class StoryItem
    {
        public string Section, Branch, Id, Character, Text, Inside;
        public StoryItemKind Kind;

        // p00_maes_01 -> "maes"; p00_sfx_03 -> "sfx"
        public string Code
        {
            get
            {
                var parts = Id.Split('_');
                return parts.Length >= 2 ? parts[1] : "";
            }
        }
    }

    /// <summary>Where and how one voice or sound plays. Coordinates are relative to the listener,
    /// who stands at the origin facing +Z: X is right, Y is up, Z is in front.</summary>
    public class StagingEntry
    {
        public Vector3? Position;      // null = not spatial, heard "inside the head"
        public Vector3? MoveTo;        // null = does not move
        public float MoveSeconds;      // 0 = over the whole clip
        public float Volume = 1f;
        public float LowpassHz;        // 0 = automatic, from the distance
        public float? Wait;            // seconds before the next item starts; null = whole clip
        public bool Loop;              // sounds: keep repeating (a background bed)
        public string Until = "";      // sounds: the looping bed fades out when this item starts
        public string Bed = "";        // voices: a sound that loops underneath this line
        public float? PauseAfter;      // silence after this item; negative = the next item starts that much before the end; null = defaults
        /// <summary>Interactions: which button leads to which branch, e.g. "1=B;2=A" (1 = index finger).
        /// Empty = button 1 picks branch A, button 2 branch B, and so on.</summary>
        public Dictionary<int, string> Choices = new Dictionary<int, string>();
        /// <summary>Sounds: when this sound ends, the battle loop with this clip prefix starts on the beat
        /// (stems PREFIX_l0, _l1, _l3); it stops on the bar line where the item in Until starts.</summary>
        public string BattleBase = "";
        /// <summary>Length of the battle loop in bars.</summary>
        public int BattleBars;
        public InteractionSettings Interaction;

        public static readonly StagingEntry Default = new StagingEntry { Position = new Vector3(0, 0, 4) };
    }

    public static class StoryData
    {
        public static List<StoryItem> ParseSequence(string csv)
        {
            var items = new List<StoryItem>();
            var rows = Csv.Parse(csv);
            if (rows.Count == 0) return items;
            var col = Csv.Columns(rows[0]);
            for (int r = 1; r < rows.Count; r++)
            {
                var row = rows[r];
                string Get(string name) => col.TryGetValue(name, out var i) && i < row.Length ? row[i] : "";
                var kind = Get("tipo") switch
                {
                    "voce" => StoryItemKind.Voice,
                    "suono" => StoryItemKind.Sound,
                    "interazione" => StoryItemKind.Interaction,
                    _ => StoryItemKind.Convergence,
                };
                items.Add(new StoryItem
                {
                    Section = Get("sezione"), Branch = Get("ramo"), Kind = kind, Id = Get("id"),
                    Character = Get("personaggio"), Text = Get("testo"), Inside = Get("dentro_battuta"),
                });
            }
            return items;
        }

        public static Dictionary<string, StagingEntry> ParseStaging(string csv)
        {
            var map = new Dictionary<string, StagingEntry>();
            var rows = Csv.Parse(csv);
            if (rows.Count == 0) return map;
            var col = Csv.Columns(rows[0]);
            for (int r = 1; r < rows.Count; r++)
            {
                var row = rows[r];
                string Get(string name) => col.TryGetValue(name, out var i) && i < row.Length ? row[i].Trim() : "";
                float? Num(string name) =>
                    float.TryParse(Get(name).Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : (float?)null;
                Vector3? Vec(string x, string y, string z)
                {
                    var vx = Num(x);
                    var vz = Num(z);
                    if (vx == null && vz == null) return null;
                    return new Vector3(vx ?? 0, Num(y) ?? 0, vz ?? 0);
                }

                var key = Get("id");
                if (key.Length == 0 || key.StartsWith("#")) continue;
                map[key] = new StagingEntry
                {
                    Position = Vec("x", "y", "z"),
                    MoveTo = Vec("a_x", "a_y", "a_z"),
                    MoveSeconds = Num("secondi_movimento") ?? 0,
                    Volume = Num("volume") ?? 1,
                    LowpassHz = Num("filtro_hz") ?? 0,
                    Wait = Num("attesa"),
                    Loop = Get("ripeti").ToLowerInvariant() is "sì" or "si" or "x" or "1" or "yes",
                    Until = Get("fino_a"),
                    Bed = Get("sottofondo"),
                    PauseAfter = Num("pausa_dopo"),
                    Choices = ParseChoices(Get("scelte")),
                    Interaction = InteractionSettings.Parse(Get),
                    BattleBase = Get("base"),
                    BattleBars = (int)(Num("battute") ?? 0),
                };
            }
            return map;
        }

        // "1=B;2=A" -> {1: "B", 2: "A"}
        private static Dictionary<int, string> ParseChoices(string text)
        {
            var map = new Dictionary<int, string>();
            foreach (var pair in text.Split(';', ' '))
            {
                var kv = pair.Split('=');
                if (kv.Length == 2 && int.TryParse(kv[0].Trim(), out var button) && kv[1].Trim().Length > 0)
                    map[button] = kv[1].Trim().ToUpperInvariant();
            }
            return map;
        }
    }

    /// <summary>Minimal RFC 4180 CSV reader: quoted fields, doubled quotes, newlines inside quotes.</summary>
    public static class Csv
    {
        public static List<string[]> Parse(string text)
        {
            var rows = new List<string[]>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (quoted)
                {
                    if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else if (c == '"') quoted = false;
                    else field.Append(c);
                }
                else if (c == '"') quoted = true;
                else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
                else if (c == '\n' || c == '\r')
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    row.Add(field.ToString()); field.Clear();
                    if (row.Count > 1 || row[0].Length > 0) rows.Add(row.ToArray());
                    row.Clear();
                }
                else if (c != '﻿') field.Append(c);
            }
            if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); rows.Add(row.ToArray()); }
            return rows;
        }

        public static Dictionary<string, int> Columns(string[] header)
        {
            var map = new Dictionary<string, int>();
            for (int i = 0; i < header.Length; i++) map[header[i].Trim()] = i;
            return map;
        }
    }
}
