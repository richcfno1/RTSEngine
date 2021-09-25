using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace RTS.Menu.Profile
{
    public class PlayerProfile
    {
        public string playerName;
        public Dictionary<string, KeyCode> playerHotkeys;

        public PlayerProfile(string name)
        {
            playerName = name;
            playerHotkeys = new Dictionary<string, KeyCode>()
            {
                { "SelectUnit", KeyCode.Mouse0 },
                { "SelectAllUnit", KeyCode.Q },
                { "SelectSameType", KeyCode.W },
                { "MoveMainSelectTypeToNext", KeyCode.Tab },
                { "Select1", KeyCode.F1 },
                { "Select2", KeyCode.F2 },
                { "Select3", KeyCode.F3 },
                { "Select4", KeyCode.F4 },
                { "Select5", KeyCode.F5 },
                { "Select6", KeyCode.F6 },
                { "Select7", KeyCode.F7 },
                { "Select8", KeyCode.F8 },
                { "Select9", KeyCode.F9 },
                { "Select10", KeyCode.F10 },
                { "GroupKey", KeyCode.LeftControl },
                { "GroupAddKey", KeyCode.LeftShift },
                { "Group1", KeyCode.Alpha1 },
                { "Group2", KeyCode.Alpha2 },
                { "Group3", KeyCode.Alpha3 },
                { "Group4", KeyCode.Alpha4 },
                { "Group5", KeyCode.Alpha5 },
                { "Group6", KeyCode.Alpha6 },
                { "Group7", KeyCode.Alpha7 },
                { "Group8", KeyCode.Alpha8 },
                { "Group9", KeyCode.Alpha9 },
                { "Group10", KeyCode.Alpha0 },
                { "MainCommand", KeyCode.Mouse1 },
                { "CancelMainCommand", KeyCode.Mouse0 },
                { "SelectTarget", KeyCode.Mouse0 },
                { "CancelSelectTarget", KeyCode.Mouse1 },
                { "SetUnitMoveHeight", KeyCode.LeftShift },
                { "Stop", KeyCode.S },
                { "Attack", KeyCode.A },
                { "Follow", KeyCode.Z },
                { "LookAt", KeyCode.X },
                { "Skill1", KeyCode.E },
                { "Skill2", KeyCode.R },
                { "Skill3", KeyCode.D },
                { "Skill4", KeyCode.F },
                { "Skill5", KeyCode.C },
                { "FireControlKey", KeyCode.LeftControl },
                { "Aggressive", KeyCode.A },
                { "Neutral", KeyCode.S },
                { "Passive", KeyCode.D },
                { "RotateCamera", KeyCode.Mouse2 },
                { "SetCameraHeight", KeyCode.LeftShift },
                { "TrackSelectedUnits", KeyCode.V },
                { "TacticalView", KeyCode.Space }
            };
        }

        public static void SavePlayerProfile(PlayerProfile profile)
        {
            string path = Path.Combine(MenuManager.GameDocumentProfilePath, $"{profile.playerName}.pfl");
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            File.WriteAllText(path, JsonConvert.SerializeObject(profile));
        }

        public static PlayerProfile LoadPlayerProfile(string name)
        {
            string path = Path.Combine(MenuManager.GameDocumentProfilePath, $"{name}.pfl");
            if (!File.Exists(path))
            {
                return null;
            }
            PlayerProfile temp = JsonConvert.DeserializeObject<PlayerProfile>(File.ReadAllText(path));
            path = Path.Combine(MenuManager.GameDocumentProfilePath, $"Recent.txt");
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            File.WriteAllText(path, temp.playerName);
            SaveRecentPlayerProfile(temp.playerName);
            return temp;
        }

        public static List<PlayerProfile> GetAllPlayerProfiles()
        {
            List<string> allFiles = Directory.EnumerateFiles(MenuManager.GameDocumentProfilePath, "*.*", SearchOption.AllDirectories)
                .Where(s => Path.GetExtension(s).TrimStart('.').ToLowerInvariant() == "pfl").ToList();

            List<PlayerProfile> result = new List<PlayerProfile>();
            foreach (string i in allFiles)
            {
                result.Add(LoadPlayerProfile(Path.GetFileNameWithoutExtension(i)));
            }

            return result;
        }

        public static void SaveRecentPlayerProfile(string name)
        {
            string path = Path.Combine(MenuManager.GameDocumentProfilePath, "Recent.txt");
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            File.WriteAllText(path, name);
        }

        public static PlayerProfile LoadRecentPlayerProfile()
        {
            string path = Path.Combine(MenuManager.GameDocumentProfilePath, "Recent.txt");
            if (File.Exists(path))
            {
                return LoadPlayerProfile(File.ReadAllText(path));
            }
            return null;
        }

        public static void DeletePlayerProfile(string name)
        {
            string path = Path.Combine(MenuManager.GameDocumentProfilePath, $"{name}.pfl");
            if (!File.Exists(path))
            {
                return;
            }
            File.Delete(path);
        }
    }
}
