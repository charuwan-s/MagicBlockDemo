using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Block))]
public class BlockEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Block targetScript = (Block)serializedObject.targetObject;

        EditorGUILayout.LabelField("Row", targetScript.Row.ToString());
        EditorGUILayout.LabelField("Column", targetScript.Column.ToString());
    }
}
