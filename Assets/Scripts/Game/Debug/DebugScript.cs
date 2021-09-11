using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTS.Game;


public class DebugScript : MonoBehaviour
{
    public Text info;

    private List<float> fps = new List<float>() { 0, 0, 0, 0, 0 };

    // Update is called once per frame
    void Update()
    {
        fps.RemoveAt(0);
        fps.Add(Time.deltaTime);
        float avg = 0;
        foreach (float i in fps)
        {
            avg += i;
        }
        avg /= 5;
        info.text = $"FPS:{Mathf.RoundToInt(1 / avg)}\nSize:{GameManager.GameManagerInstance.GetAllGameObjects().Count}";
    }
}
