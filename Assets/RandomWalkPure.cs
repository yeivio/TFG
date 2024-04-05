using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RandomWalkPure : GenerationAlgorithm
{
    public int threshold; // Percentage of the cellMap that must be painted
    private int paintedMap;

    public override void Generate(int seed = -1)
    {
        paintedMap = 0;
        map = new CELL_TYPE[widthMap, heightMap];
        this.seed = seed;
        GenerateSeed(seed);
        int randomX = widthMap / 2;
        int randomY = heightMap / 2;
        map[randomX, randomY] = CELL_TYPE.FLOOR;
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
                    }
                    break;

                case 1: // Top
                    if (randomY + 1 < heightMap)
                    {
                        randomY = randomY + 1;
                    }
                    break;

                case 2: // Right
                    if (randomX + 1 < widthMap)
                    {
                        randomX = randomX + 1;
                    }
                    break;

                case 3: // Down
                    if (randomY > 0)
                    {
                        randomY = randomY - 1;
                    }
                    break;
            }

            if (map[randomX, randomY] == CELL_TYPE.NOTHING) // Cell is not visited   
            {
                map[randomX, randomY] = CELL_TYPE.FLOOR;
                paintedMap++;
            }
        }


        for (int i = 0; i < widthMap; i++)
        {
            for (int j = 0; j < heightMap; j++)
            {
                if (map[i, j] == CELL_TYPE.NOTHING)
                    map[i, j] = CELL_TYPE.WALL;
            }
        }
    }

    //private void OnDrawGizmosSelected()
    //{
    //    if (this.map != null)
    //    {
    //        for (int i = 0; i < widthMap; i++)
    //            for (int j = 0; j < heightMap; j++)
    //            {
    //                try
    //                {
    //                    switch (map[i, j])
    //                    {
    //                        case CELL_TYPE.WALL:
    //                            Gizmos.color = Color.black;
    //                            break;
    //                        case CELL_TYPE.FLOOR:
    //                            Gizmos.color = Color.white;
    //                            break;
    //                        case CELL_TYPE.CORRIDOR:
    //                            Gizmos.color = Color.grey;
    //                            break;
    //                        case CELL_TYPE.NOTHING:
    //                            Gizmos.color = Color.red;
    //                            break;
    //                    }
    //                    Gizmos.DrawCube(new Vector3(tileSize * j + 0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
    //                }
    //                catch
    //                {
    //                    Gizmos.color = Color.gray;
    //                    Gizmos.DrawCube(new Vector3(tileSize * j + 0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
    //                }
    //            }
    //    }
    //}
}

# region GizmoEditor
#if UNITY_EDITOR
[CustomEditor(typeof(RandomWalkPure))]
public class ScriptEditorRWP : Editor
{
    private RandomWalkPure gizmoDrawing;

    public override void OnInspectorGUI()
    {
        gizmoDrawing = (RandomWalkPure)target;
        gizmoDrawing.widthMap = EditorGUILayout.IntSlider("Width", gizmoDrawing.widthMap, 0, 300);
        gizmoDrawing.heightMap = EditorGUILayout.IntSlider("Height", gizmoDrawing.heightMap, 0, 300);
        gizmoDrawing.tileSize = EditorGUILayout.IntSlider("Tile Size", gizmoDrawing.tileSize, 1, 100);

        EditorGUILayout.FloatField("Execution time (ms)", gizmoDrawing.executionTime);
        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);
        //DrawDefaultInspector(); // Draw all public variables
        EditorGUILayout.Space();

        gizmoDrawing.threshold = EditorGUILayout.IntSlider("% Fill of cellMap", gizmoDrawing.threshold, 0, 100);
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
