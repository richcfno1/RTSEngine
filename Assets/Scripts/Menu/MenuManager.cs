using RTS.Menu.Profile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RTS.Menu
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager MenuManagerInstance { get; private set; } = null;
        public static string GameDocumentRootPath { get; private set; } = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "RTSEngine");
        public static string GameDocumentMapPath { get; private set; } = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "RTSEngine", "Map");
        public static string GameDocumentDeckPath { get; private set; } = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "RTSEngine", "Deck");
        public static string GameDocumentProfilePath { get; private set; } = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "RTSEngine", "Profile");

        public PlayerProfile CurrentProfile { get; set; } = null;

        void Awake()
        {
            if (MenuManagerInstance == null)
            {
                MenuManagerInstance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);

            if (!Directory.Exists(GameDocumentRootPath))
            {
                Directory.CreateDirectory(GameDocumentRootPath);
            }
            if (!Directory.Exists(GameDocumentMapPath))
            {
                Directory.CreateDirectory(GameDocumentMapPath);
            }
            if (!Directory.Exists(GameDocumentDeckPath))
            {
                Directory.CreateDirectory(GameDocumentDeckPath);
            }
            if (!Directory.Exists(GameDocumentProfilePath))
            {
                Directory.CreateDirectory(GameDocumentProfilePath);
            }

            CurrentProfile = PlayerProfile.LoadRecentPlayerProfile();
            if (CurrentProfile == null)
            {
                Debug.LogWarning("No recent player profile, create one!");
                // TODO: new profile window
                PlayerProfile.SavePlayerProfile(new PlayerProfile("RC"));
                CurrentProfile = PlayerProfile.LoadPlayerProfile("RC");
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
