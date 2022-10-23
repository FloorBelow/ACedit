using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(EntityComponent))]
public class EntityComponentEditor : Editor
{
    public override void OnInspectorGUI() {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Make Editable")) {
            Edit();
        }
        if (GUILayout.Button("Reset")) {
            ResetPos();
        }
        EditorGUILayout.EndHorizontal();
        DrawDefaultInspector();
    }

    void Edit() {
        EntityComponent e = (EntityComponent)target;
        e.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(@"Assets\mat3.mat");
        e.edit = true;
    }

    void ResetPos() {
        EntityComponent e = (EntityComponent)target;
        MatrixUtil.SetTransform(e.transform, e.transformMatrix);
        e.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(@"Assets\mat2.mat");
        e.edit = false;
    }

}
