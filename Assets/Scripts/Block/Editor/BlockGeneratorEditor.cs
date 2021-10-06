using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BlockGenerator))]
public class BlockGeneratorEditor : Editor
{
    SerializedProperty blockPrototype;
    SerializedProperty blockTextures;

    void OnEnable()
    {
        blockPrototype = serializedObject.FindProperty("blockPrototype");
        blockTextures = serializedObject.FindProperty("blockTextures");
    }

    public override void OnInspectorGUI()
    {
        BlockGenerator targetScript = (BlockGenerator)serializedObject.targetObject;

        string[] colorNames = System.Enum.GetNames(typeof(BlockGenerator.BlockColor));
        int numColors = colorNames.Length;
        if (targetScript.blockTextures.Length < numColors)
        {
            targetScript.blockTextures = new Texture[numColors];
        }

        EditorGUILayout.PropertyField(blockPrototype, true);

        for (int i = 0; i < numColors; i++)
        {
            EditorGUILayout.ObjectField(blockTextures.GetArrayElementAtIndex(i), new GUIContent(colorNames[i]));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
