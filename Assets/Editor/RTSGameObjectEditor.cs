using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using RTS.Game.RTSGameObject;

namespace RTS.RTSEditor
{
    public class RTSGameObjectEditor : Editor
    {
        [MenuItem("RTSEngine/RTSGameObject/Add to Json")]
        static void AddRTSGameObjectDataToJson()
        {
            if (Selection.activeObject == null || Selection.activeObject.GetType() != typeof(RTSGameObjectData))
            {
                Debug.LogError("Please select a RTSGameObjectData asset");
                return;
            }
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            RTSGameObjectData data = (RTSGameObjectData)Selection.activeObject;
            if (path.StartsWith("Assets/Resources/") && path.EndsWith(".asset"))
            {
                path = path.Substring("Assets/Resources/".Length, path.Length - ".asset".Length - "Assets/Resources/".Length);
                TextAsset json = Resources.Load<TextAsset>("Library/GameObjectLibrary");
                SortedDictionary<string, string> library = new SortedDictionary<string, string>();
                if (json != null)
                {
                    library = JsonConvert.DeserializeObject<SortedDictionary<string, string>>(json.text);
                }
                else
                {
                    Debug.Log("Create new library.");
                }
                string newKey = data.prefab.GetComponent<RTSGameObjectBaseScript>().typeID;
                if (!library.ContainsKey(newKey) && !library.ContainsKey(path))
                {
                    library.Add(newKey, path);
                }
                File.WriteAllText(Application.dataPath + "/Resources/Library/GameObjectLibrary.json",
                    JsonConvert.SerializeObject(library));
                Debug.Log("Success");
            }
            else
            {
                Debug.LogError("Cannot find it as an asset or in Assets/Resources");
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
}