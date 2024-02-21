using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RandomWalk : GenerationAlgorithm
{
    #region Gizmo Settings
    [Header("DRAWING OPTIONS")]
    public int widthMap;   // Number of columns in the map
    public int heightMap;  // Number of rows in the map
    public int tileSize;    // Size of each tile on the canvas
    public long executionTime;
    public int threshold;
    #endregion   
    private bool[,] map;

    private int paintedMap;

    public void Generate(int seed = -1)
    {
        paintedMap = 0;
        map = new bool[widthMap, heightMap];
        this.seed = seed;
        GenerateSeed(seed);
        int randomX = UnityEngine.Random.Range(0, widthMap);
        int randomY = UnityEngine.Random.Range(0, heightMap);
        map[randomX, randomY] = true;
        paintedMap++;
        while ((float)(paintedMap / (float)(widthMap * heightMap)) < (float)(threshold / 100f))
        {
            int randomDir = UnityEngine.Random.Range(0, 4);
            
            switch (randomDir)
            {
                case 0: // Left
                    if (randomX > 0)
                    {
                        randomX = randomX - 1;
                        if (!map[randomX, randomY]) // Cell is not visited   
                        {
                            map[randomX, randomY] = true;
                            paintedMap++;
                        }
                    }
                    break;

                case 1: // Top
                    if (randomY + 1 < heightMap)
                    {
                        randomY = randomY + 1;
                        if (!map[randomX, randomY]) // Cell is not visited   
                        {
                            map[randomX, randomY] = true;
                            paintedMap++;
                        }
                    }
                    break;

                case 2: // Right
                    if (randomX + 1 < widthMap)
                    {
                        randomX = randomX + 1;
                        if (!map[randomX, randomY]) // Cell is not visited   
                        {
                            map[randomX, randomY] = true;
                            paintedMap++;
                        }
                    }
                    break;

                case 3: // Down
                    if (randomY > 0 )
                    {
                        randomY = randomY - 1;
                        if (!map[randomX, randomY]) // Cell is not visited   
                        {
                            map[randomX, randomY] = true;
                            paintedMap++;
                        }
                    }
                    break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
            return;
        if (this.map != null)
        {
            for (int i = -1; i < heightMap+1; i++)
                for (int j = -1; j < widthMap+1; j++)
                {
                    try
                    {
                        if (!map[j, i])
                            Gizmos.color = Color.black;
                        else
                            Gizmos.color = Color.white;
                        Gizmos.DrawCube(new Vector3(tileSize * j + 0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                    }
                    catch
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(new Vector3(tileSize * j + 0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                    }
                }
        }
    }
}

# region GizmoEditor
#if UNITY_EDITOR
[CustomEditor(typeof(RandomWalk))]
public class ScriptEditorRW : Editor
{
    private RandomWalk gizmoDrawing;

    public override void OnInspectorGUI()
    {
        gizmoDrawing = (RandomWalk)target;
        gizmoDrawing.widthMap = EditorGUILayout.IntSlider("Width", gizmoDrawing.widthMap, 0, 300);
        gizmoDrawing.heightMap = EditorGUILayout.IntSlider("Height", gizmoDrawing.heightMap, 0, 300);
        gizmoDrawing.tileSize = EditorGUILayout.IntSlider("Tile Size", gizmoDrawing.tileSize, 1, 100);
        EditorGUILayout.FloatField("Execution time (ms)", gizmoDrawing.executionTime);
        //DrawDefaultInspector(); // Draw all public variables
        EditorGUILayout.Space();

        gizmoDrawing.threshold = EditorGUILayout.IntSlider("% Fill of map", gizmoDrawing.threshold, 0, 100);

        if (GUILayout.Button("Generate cellular automata"))
        {
            gizmoDrawing.Generate(gizmoDrawing.seed);

        }

        if (GUILayout.Button("Generate random cellular automata"))
        {
            gizmoDrawing.Generate();
        }

        if (GUI.changed)
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif
#endregion
