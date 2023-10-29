using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(GizmoDrawing))]
public class ScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw all public variables

        GizmoDrawing gizmoDrawing = (GizmoDrawing)target;
        if (GUILayout.Button("New Generation"))
            gizmoDrawing.generateRandomPattern();
            
    }
}
