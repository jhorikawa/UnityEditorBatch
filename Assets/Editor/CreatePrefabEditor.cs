using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Debug = UnityEngine.Debug;

[CustomEditor(typeof(CreatePrefab))]
public class CreatePrefabEditor : Editor
{
    void NewPrefab()
    {

        var objs = Resources.FindObjectsOfTypeAll<GameObject>();
        List<string> filePaths = new List<string>();
        foreach (var obj in objs)
        {
            var path = AssetDatabase.GetAssetPath(obj);

            if (Path.HasExtension(path) && Path.GetExtension(path) == ".fbx" && !filePaths.Contains(path))
            {
                filePaths.Add(path);
                Debug.Log(path);

                var savePath = Path.GetDirectoryName(path);
                var fileName = Path.GetFileNameWithoutExtension(path) + ".prefab";
                savePath = Path.Combine(savePath, fileName);

                var tmpobj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                GameObject tmpInstance = GameObject.Instantiate(tmpobj);

                var targetObject = serializedObject.targetObject;
                var field = targetObject.GetType()
                    .GetField(serializedObject.FindProperty("layerMats").propertyPath);
                var layerMats = field.GetValue(targetObject);
                if (layerMats != null)
                {
                    foreach (var layerMat in (LayerMat[])layerMats)
                    {
                        var layerObj = tmpInstance.transform.Find(layerMat.layer);
                        if (layerObj != null)
                        {
                            var meshRenderers = layerObj.GetComponentsInChildren<MeshRenderer>();
                            foreach (var meshRenderer in meshRenderers)
                            {
                                meshRenderer.material = layerMat.material;
                            }
                        }
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(tmpInstance, savePath);
                
                GameObject.DestroyImmediate(tmpInstance);
            }
            
        }
    }
    
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Convert FBX to Prefab"))
        {
            NewPrefab();
        }
    }
}



