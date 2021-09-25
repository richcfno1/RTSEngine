using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RTS.Menu.Profile
{
    public class ProfileMenuManager : MonoBehaviour
    {
        public static ProfileMenuManager ProfileMenuManagerInstance { get; private set; } = null;
        public Transform profileDisplayList;
        public GameObject profileGridPrefab;

        public PlayerProfile CurrentSelectedProfile { get; set; }

        void Awake()
        {
            ProfileMenuManagerInstance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            DrawAllProfiles();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void DrawAllProfiles()
        {
            foreach (ProfileGridScript i in profileDisplayList.GetComponentsInChildren<ProfileGridScript>())
            {
                Destroy(i.gameObject);
            }
            foreach (PlayerProfile i in PlayerProfile.GetAllPlayerProfiles())
            {
                GameObject temp = Instantiate(profileGridPrefab, profileDisplayList);
                temp.GetComponent<ProfileGridScript>().InitProfileGrid(i);
            }
        }

        public void OnCreateButtonClicked()
        {
            PlayerProfile temp = new PlayerProfile("TestPlayerA");
            PlayerProfile.SavePlayerProfile(temp);
            DrawAllProfiles();
        }

        public void OnSelectButtonClicked()
        {
            // Load profile into CurrentProfile
            MenuManager.MenuManagerInstance.CurrentProfile = PlayerProfile.LoadPlayerProfile(CurrentSelectedProfile.playerName);
        }

        public void OnDeleteButtonClicked()
        {
            PlayerProfile.DeletePlayerProfile(CurrentSelectedProfile.playerName);
            CurrentSelectedProfile = null;
            DrawAllProfiles();
        }

        public void OnMenuButtonClicked()
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}

