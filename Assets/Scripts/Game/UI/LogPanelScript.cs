using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.Game.UI
{
    public class LogPanelScript : MonoBehaviour
    {
        public static LogPanelScript LogPanelScriptInstance { get; private set; }
        public Image background;
        public Text textbox;

        void Awake()
        {
            LogPanelScriptInstance = this;
        }

        public void DisplayText(string text, float time)
        {
            StopAllCoroutines();
            textbox.text = text;
            Color temp = background.color;
            temp.a = 0.5f;
            background.color = temp;
            StartCoroutine(HideLogPanel(time));
        }

        private IEnumerator HideLogPanel(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            textbox.text = "";
            Color temp = background.color;
            temp.a = 0;
            background.color = temp;
        }
    }
}
