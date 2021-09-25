using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RTS.Menu.Profile
{
    public class ProfileGridScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Image backgroundImage;
        public Text playerNameText;
        public GameObject currentProfileIcon;
        public PlayerProfile thisProfile;

        private bool isPointed = false;

        // Update is called once per frame
        void Update()
        {
            Color temp = backgroundImage.color;
            currentProfileIcon.SetActive(thisProfile.playerName == MenuManager.MenuManagerInstance.CurrentProfile.playerName);
            if (ProfileMenuManager.ProfileMenuManagerInstance.CurrentSelectedProfile != null && 
                thisProfile.playerName == ProfileMenuManager.ProfileMenuManagerInstance.CurrentSelectedProfile.playerName)
            {
                temp.a = 0.5f;
            }
            else
            {
                temp.a = 0.25f;
            }
            if (isPointed)
            {
                temp.a += 0.25f;
            }
            backgroundImage.color = temp;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointed = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointed = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ProfileMenuManager.ProfileMenuManagerInstance.CurrentSelectedProfile = thisProfile;
        }

        public void InitProfileGrid(PlayerProfile profile)
        {
            thisProfile = profile;
            playerNameText.text = thisProfile.playerName;
        }
    }
}