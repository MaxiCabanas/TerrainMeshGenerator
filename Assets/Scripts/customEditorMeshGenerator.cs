using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(meshGenerator))]
[CanEditMultipleObjects]
public class customEditorMeshGenerator : Editor
{
    private meshGenerator script;
    public override void OnInspectorGUI()
    {
        //dibujo el inspector original
        base.OnInspectorGUI();

        //referencia al script al que estamos editando el inspector.
        script = (meshGenerator)target;

        //botones
        if(GUILayout.Button("Refresh"))
        {
            script.CreateMesh();
        }
        if(GUILayout.Button("Generate mesh with random seed"))
        {
            script.GenerateSeed();
            script.CreateMesh();
        }
    }
}
