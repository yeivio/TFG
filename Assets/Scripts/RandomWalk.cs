using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RandomWalk : GenerationAlgorithm
{
    public int threshold; // Percentage of the cellMap that must be painted
    public int numMaxPasos; // Numero de pasos maximos que puede dar
    public int numMaxAgentes; // Numero de agentes que se van a utilizar

    private int paintedMap;


    public void DataMeassure(int seed = -1)
    {
        base.startGridSize = 50;
        base.endGridSize = 350;
        base.intervalGridSize = 50;
        
        List<String> textoCabecera = new List<String>();
        List<String> textoData = new List<String>();
        int numPruebas = 5;
        for (int i = base.startGridSize; i <= base.endGridSize; i += base.intervalGridSize)
        {
            textoCabecera.Add(i.ToString());
        }
        for (int i = base.startGridSize; i <= base.endGridSize; i += base.intervalGridSize)
        {
            var totalPruebas = (long)0;
            for (int j = 0; j <= numPruebas; j++)
            {
                this.widthMap = i;
                this.heightMap = i;
                var watch = System.Diagnostics.Stopwatch.StartNew();    // Start meassuring time
                this.Generate(seed);
                watch.Stop();
                totalPruebas += watch.ElapsedMilliseconds; // This is in ms
                //textoData.Add(executionTime.ToString());
            }
            textoData.Add(((long)(totalPruebas / numPruebas)).ToString());
        }

        using (StreamWriter writer = new StreamWriter("RW.csv"))
        {
            // Escribir la primera fila con los números
            writer.WriteLine(string.Join(";", textoCabecera));

            // Escribir la segunda fila con los valores
            writer.WriteLine(string.Join(";", textoData));
        }
    }


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

        float numPasos = 0;
        float numAgentes = 0;
        while ((float)(paintedMap / (float)(widthMap * heightMap)) < (float)(threshold / 100f) && numAgentes < numMaxAgentes)
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
                    if (randomY > 0 )
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


            numPasos++;

            // Se resetea si es necesario
            if (numPasos >= numMaxPasos)
            {
                randomX = widthMap / 2;
                randomY = heightMap / 2;
                numAgentes++;
                numPasos = 0;
            }
        }


        for (int i = 0; i < widthMap; i++) { 
            for (int j = 0; j < heightMap; j++)
            {
                if (map[i, j] == CELL_TYPE.NOTHING)
                    map[i, j] = CELL_TYPE.WALL;
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
        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);
        //DrawDefaultInspector(); // Draw all public variables
        EditorGUILayout.Space();

        gizmoDrawing.threshold = EditorGUILayout.IntSlider("% Fill of cellMap", gizmoDrawing.threshold, 0, 100);
        gizmoDrawing.numMaxAgentes = EditorGUILayout.IntField("Num Max de agentes", gizmoDrawing.numMaxAgentes);
        gizmoDrawing.numMaxPasos = EditorGUILayout.IntField("Max num pasos", gizmoDrawing.numMaxPasos);
        if (GUILayout.Button("Generate cellular automata"))
        {
            gizmoDrawing.Generate(gizmoDrawing.seed);

        }

        if (GUILayout.Button("Generate random cellular automata"))
        {
            gizmoDrawing.Generate();
        }
        if (GUILayout.Button("MeassureTime"))
        {
            gizmoDrawing.DataMeassure();
        }

        if (GUI.changed)
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif
#endregion
