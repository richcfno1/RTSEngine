using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RTS.Menu.Main
{
    public class MainMenuManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnSkirmishButtonClicked()
        {

        }

        public void OnMultiplayerButtonClicked()
        {

        }

        public void OnDeckButtonClicked()
        {

        }

        public void OnProfileButtonClicked()
        {
            SceneManager.LoadScene("ProfileMenuScene");
        }

        public void OnSettingButtonClicked()
        {

        }

        public void OnQuitButtonClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}