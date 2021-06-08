using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RTSGameObjectEditor : Editor
{
    [MenuItem("RTSEngine/RTSGameObject/Add to Json")]
    static void AddRTSGameObjectToJson()
    {
        if (Selection.activeGameObject == null || !PrefabUtility.IsPartOfPrefabAsset(Selection.activeGameObject) ||
            Selection.activeGameObject.GetComponent<RTSGameObjectBaseScript>() == null)
        {
            Debug.LogError("Please select a RTS Game Object prefab object");
            return;
        }
        string path = AssetDatabase.GetAssetPath(Selection.activeGameObject);
        if (path.StartsWith("Assets/Resources/") && path.EndsWith(".prefab"))
        {
            path = path.Substring("Assets/Resources/".Length, path.Length - ".prefab".Length - "Assets/Resources/".Length);
            TextAsset json = Resources.Load<TextAsset>("ItemLibrary/ItemLibrary");
            Dictionary<int, string> items = JsonConvert.DeserializeObject<Dictionary<int, string>>(json.text);
            foreach (string temp in items.Values)
            {
                if (path == temp)
                {
                    Debug.LogError("Please select a new (non used before) RTS Game Object object");
                    return;
                }
            }
            int i = 1;
            while (items.ContainsKey(i))
            {
                i++;
            }
            items.Add(i, path);
            File.WriteAllText(Application.dataPath + "/Resources/ItemLibrary/ItemLibrary.json",
                JsonConvert.SerializeObject(items));
            Debug.Log("Success with index: " + i);
        }
        else
        {
            Debug.LogError("Cannot find it as a prefab or in Assets/Resources");
            return;
        }
    }

    [MenuItem("RTSEngine/RTSGameObject/Get Icon")]
    static void GetRTSGameObjectIcon()
    {
        if (Selection.activeGameObject == null || !PrefabUtility.IsPartOfPrefabAsset(Selection.activeGameObject) ||
            Selection.activeGameObject.GetComponent<RTSGameObjectBaseScript>() == null)
        {
            Debug.LogError("Please select a RTS Game Object prefab object");
            return;
        }
        string path = AssetDatabase.GetAssetPath(Selection.activeGameObject);
        if (path.StartsWith("Assets/Resources/") && path.EndsWith(".prefab"))
        {
            path = path.Substring("Assets".Length, path.Length - ".prefab".Length - "Assets".Length);
            path = Application.dataPath + path + "Icon.png";
            File.WriteAllBytes(path, AssetPreview.GetAssetPreview(Selection.activeGameObject).EncodeToPNG());
        }
        else
        {
            Debug.LogError("Cannot find it as a prefab or in Assets/Resources");
            return;
        }
    }
}

