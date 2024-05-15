using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


// 2 3 1528892033
public class MazeAlgorithm : GenerationAlgorithm
{
    private Cell[,] cellMap; // True is Used, False is not Used
    public List<Tiles> usableTiles;
    private long currentExploredNumber = 0;
    public GameObject prefab;
    public void DataMeassure(int seed = -1)
    {
        base.startGridSize = 50;
        base.endGridSize = 600;
        base.intervalGridSize = 50;
        List<String> textoCabecera = new List<String>();
        List<String> textoData = new List<String>();
        int numPruebas = 2;
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
                this.Generate(seed);
                var watch = System.Diagnostics.Stopwatch.StartNew();    // Start meassuring time
                //Profiler.BeginSample("MAZEMEDIDA");
                for (int p = 0; p < heightMap; p++)
                    for (int q = 0; q < widthMap; q++)
                    {
                        if (cellMap[q, p].IsVisited())
                        {
                            SpawnTile(q, p);
                        }
                    }
                //Profiler.EndSample();
                watch.Stop();
                totalPruebas += watch.ElapsedMilliseconds; // This is in ms
                //textoData.Add(executionTime.ToString());
            }
            textoData.Add(((long)(totalPruebas / numPruebas)).ToString());
        }

        using (StreamWriter writer = new StreamWriter("MZ_Painting.csv"))
        {
            // Escribir la primera fila con los n�meros
            writer.WriteLine(string.Join(";", textoCabecera));

            // Escribir la segunda fila con los valores
            writer.WriteLine(string.Join(";", textoData));
        }
    }

    public override void Generate(int seed = -1)
    {
        //Clean prev output
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }


        this.seed = seed;
        GenerateSeed(seed);
        cellMap = new Cell[widthMap, heightMap];
        for (int i = 0; i < widthMap; i++)
        {
            for (int j = 0; j < heightMap; j++)
            {
                cellMap[i, j] = new Cell();
            }
        }
        UnityEngine.Profiling.Profiler.BeginSample("MAZEMEDIDA");
        int startingX = UnityEngine.Random.Range(0, widthMap);
        int startingY = UnityEngine.Random.Range(0, heightMap);
        long maxCellNumber = widthMap * heightMap;
        currentExploredNumber = 0;
        while (currentExploredNumber < maxCellNumber)
        {
            kill(startingX, startingY);
            Vector2Int aux = hunt();
            if (aux.x < 0)
            {
                break;
            }

            startingX = aux.x;
            startingY = aux.y;
        }
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("MAZEMEDIDA");
        var watch = System.Diagnostics.Stopwatch.StartNew();    // Start meassuring time
        for (int p = 0; p < heightMap; p++)
            for (int q = 0; q < widthMap; q++)
            {
                if (cellMap[q, p].IsVisited())
                {
                    SpawnTile(q, p);
                }
            }

        watch.Stop();
        UnityEngine.Profiling.Profiler.EndSample();
        Debug.Log(watch.ElapsedMilliseconds);
    }

    //private void Update()
    //{
    //    Generate();
    //}

    public void SpawnTile(int x, int y)
    {
        Cell celda = cellMap[x, y];
        List<Tiles> options = new List<Tiles>() ;
        foreach(Tiles tile in new List<Tiles>(usableTiles))
        {
            
            if (tile.isLeftConnected && !celda.HasConnection(Cell.ORIENTATION.LEFT) ||
                tile.isRightConnected && !celda.HasConnection(Cell.ORIENTATION.RIGHT) ||
                tile.isTopConnected && !celda.HasConnection(Cell.ORIENTATION.UP) ||
                tile.isBottomConnected && !celda.HasConnection(Cell.ORIENTATION.DOWN))
            {
                // Las tile tiene una o m�s orientaciones que no coinciden
                //options.Remove(tile);
                continue;
            }

            if (!tile.isLeftConnected && celda.HasConnection(Cell.ORIENTATION.LEFT) ||
                !tile.isRightConnected && celda.HasConnection(Cell.ORIENTATION.RIGHT) ||
                !tile.isTopConnected && celda.HasConnection(Cell.ORIENTATION.UP) ||
                !tile.isBottomConnected && celda.HasConnection(Cell.ORIENTATION.DOWN))
            {
                //Las tiles no tienen absolutamente todas las coincidencias
                //options.Remove(tile);
                continue;
            }

            options.Add(tile);

        }

        int num = UnityEngine.Random.Range(0, options.Count);
        //GameObject aux = new GameObject().gameObject;
        GameObject aux;
        try
        {
            aux = Instantiate(prefab);
            aux.GetComponent<SpriteRenderer>().sprite = options[num].GetComponent<SpriteRenderer>().sprite;
            aux.transform.parent = this.transform;
            aux.gameObject.transform.position = new Vector3(x, y, 0);
            //aux.AddComponent<SpriteRenderer>().sprite = options[num].GetComponent<SpriteRenderer>().sprite;
        }
        catch (Exception)
        {
            Debug.Log("Seed:" + seed);
        }
    }

    public void kill(int x, int y)
    {
        int xIndex = x;
        int yIndex = y;
        if (!this.cellMap[xIndex, yIndex].IsVisited())
            currentExploredNumber++;
        this.cellMap[xIndex, yIndex].Visited();
        while (canFindNeighbour(xIndex, yIndex))
        {
            int direction = UnityEngine.Random.Range(0, 4);
            switch (direction)
            {
                case 0:
                    if (xIndex + 1 < widthMap && !cellMap[xIndex + 1, yIndex].IsVisited())
                    {
                        this.cellMap[xIndex, yIndex].setConnection(Cell.ORIENTATION.RIGHT);
                        xIndex++;
                        this.cellMap[xIndex, yIndex].setConnection(Cell.ORIENTATION.LEFT);
                    }
                    break;
                case 1:
                    if (yIndex + 1 < heightMap && !cellMap[xIndex, yIndex + 1].IsVisited())
                    {
                        this.cellMap[xIndex, yIndex].setConnection(Cell.ORIENTATION.UP);
                        yIndex++;
                        this.cellMap[xIndex, yIndex].setConnection(Cell.ORIENTATION.DOWN);
                    }
                    break;
                case 2:
                    if (xIndex - 1 >= 0 && !cellMap[xIndex - 1, yIndex].IsVisited())
                    {
                        this.cellMap[xIndex, yIndex].setConnection(Cell.ORIENTATION.LEFT);
                        xIndex--;
                        this.cellMap[xIndex, yIndex].setConnection(Cell.ORIENTATION.RIGHT);
                    }

                    break;
                case 3:
                    if (yIndex - 1 >= 0 && !cellMap[xIndex, yIndex - 1].IsVisited())
                    {
                        this.cellMap[xIndex, yIndex].setConnection(Cell.ORIENTATION.DOWN);
                        yIndex--;
                        this.cellMap[xIndex, yIndex].setConnection(Cell.ORIENTATION.UP);
                    }

                    break;
            }
            if(!this.cellMap[xIndex, yIndex].IsVisited())
                currentExploredNumber++;
            this.cellMap[xIndex, yIndex].Visited();
            
        }
    }
    public bool canFindNeighbour(int x, int y)
    {
        if ((x + 1 < this.widthMap && !cellMap[x + 1, y].IsVisited())
            || (y + 1 < this.heightMap && !cellMap[x, y + 1].IsVisited())
            || (x - 1 > 0 && !cellMap[x - 1, y].IsVisited())
            || (y - 1 > 0 && !cellMap[x, y - 1].IsVisited()))
            return true;

        return false;
    }

    public bool ConnectNeighbor(int x, int y)
    {
        if (x + 1 < this.widthMap && cellMap[x + 1, y].IsVisited())
        {
            cellMap[x, y].setConnection(Cell.ORIENTATION.RIGHT);
            cellMap[x + 1, y].setConnection(Cell.ORIENTATION.LEFT);
            return true;
        }

        if (y + 1 < this.heightMap && cellMap[x, y + 1].IsVisited())
        {
            cellMap[x, y + 1].setConnection(Cell.ORIENTATION.DOWN);
            cellMap[x, y].setConnection(Cell.ORIENTATION.UP);
            return true;
        }

        if (x - 1 >= 0 && cellMap[x - 1, y].IsVisited())
        {
            cellMap[x, y].setConnection(Cell.ORIENTATION.LEFT);
            cellMap[x - 1, y].setConnection(Cell.ORIENTATION.RIGHT);
            return true;
        }

        if (y - 1 >= 0 && cellMap[x, y - 1].IsVisited())
        {
            cellMap[x, y - 1].setConnection(Cell.ORIENTATION.UP);
            cellMap[x, y].setConnection(Cell.ORIENTATION.DOWN);
            return true;
        }

        return false;
    }

    public Vector2Int hunt()
    {
        for (int i = 0; i < heightMap; i++)
            for (int j = 0; j < widthMap; j++)
            {
                if (!cellMap[j, i].IsVisited() && ConnectNeighbor(j, i))
                {
                    if (!cellMap[j, i].IsVisited())
                        currentExploredNumber++;
                    cellMap[j, i].Visited();
                    return new Vector2Int(j, i);
                }
            }
        return new Vector2Int(-1, -1);
    }

    public class Cell
    {
        private byte isvisited;
        private byte connections;
        public enum ORIENTATION { UP = 1, DOWN = 2, LEFT = 4, RIGHT = 8 }

        public Cell()
        {
            isvisited = Byte.MinValue;
            connections = Byte.MinValue;
        }
        public void Visited()
        {
            this.isvisited = Byte.MaxValue;
        }

        public bool IsVisited()
        {
            return this.isvisited == byte.MaxValue;
        }

        public void setConnection(ORIENTATION or)
        {
            connections |= (byte)or;
        }

        public bool HasConnection(ORIENTATION or)
        {
            return (connections & (byte)or) != 0;
        }
    }

    //public class Cell
    //{
    //    public bool isVisited;
    //    public List<Cell.ORIENTATION> connections;
    //    public enum ORIENTATION { UP, DOWN, LEFT, RIGHT }

    //    public Cell()
    //    {
    //        isVisited = false;
    //        connections = new List<Cell.ORIENTATION>();
    //    }

    //    public void setConnection(ORIENTATION or)
    //    {
    //        connections.Add(or);
    //    }

    //    public List<ORIENTATION> GetConnections()
    //    {
    //        return this.connections;
    //    }
    //}

    private void OnDrawGizmosSelected()
    {
        //if (this.cellMap != null && !Application.isPlaying)
        //{
        //    for (int i = 0; i < heightMap; i++)
        //        for (int j = 0; j < widthMap; j++)
        //        {
        //            try
        //            {
        //                if (cellMap[j, i].isVisited)
        //                    Gizmos.color = Color.black;
        //                else
        //                    Gizmos.color = Color.white;
        //                Gizmos.DrawCube(new Vector3(tileSize * j + 0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
        //                DrawBorder(cellMap[j, i], j, i);

        //            }
        //            catch
        //            {
        //                Gizmos.color = Color.gray;
        //                Gizmos.DrawCube(new Vector3(tileSize * j + 0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
        //            }
        //        }
        //}
    }

    //private void DrawBorder(Cell cell, int j, int i)
    //{
    //    Gizmos.color = Color.red;
    //    foreach (Cell.ORIENTATION or in cell.GetConnections())
    //    {
    //        switch (or)
    //        {
    //            case Cell.ORIENTATION.LEFT:
    //                Gizmos.DrawSphere(new Vector3((float)(j + 0.5 * tileSize), i, 0), 0.1f);
    //                break;
    //            case Cell.ORIENTATION.RIGHT:
    //                Gizmos.DrawSphere(new Vector3((float)(j + tileSize), (float)(i + 0.5 * tileSize), 0), 0.1f);
    //                break;
    //            case Cell.ORIENTATION.UP:
    //                Gizmos.DrawSphere(new Vector3((float)(j + 0.5 * tileSize), (float)(i + tileSize), 0), 0.1f);
    //                break;
    //            case Cell.ORIENTATION.DOWN:
    //                Gizmos.DrawSphere(new Vector3((float)(j + 0.5 * tileSize), i, 0), 0.1f);
    //                break;
    //        }
    //    }

    //}

}


# region GizmoEditor
#if UNITY_EDITOR
[CustomEditor(typeof(MazeAlgorithm))]
public class ScriptEditorMaze : Editor
{
    private MazeAlgorithm gizmoDrawing;

    public override void OnInspectorGUI()
    {
        gizmoDrawing = (MazeAlgorithm)target;
        gizmoDrawing.widthMap = EditorGUILayout.IntSlider("Width", gizmoDrawing.widthMap, 0, 600);
        gizmoDrawing.heightMap = EditorGUILayout.IntSlider("Height", gizmoDrawing.heightMap, 0, 600);
        gizmoDrawing.tileSize = EditorGUILayout.IntSlider("Tile Size", gizmoDrawing.tileSize, 1, 100);
        EditorGUILayout.FloatField("Execution time (ms)", gizmoDrawing.executionTime);
        //DrawDefaultInspector(); // Draw all public variables
        EditorGUILayout.Space();
        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("usableTiles"), new GUIContent("Sprite"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefab"), new GUIContent("prefab"));
        if (GUILayout.Button("Generate cellular automata"))
        {
            gizmoDrawing.Generate(gizmoDrawing.seed);

        }

        if (GUILayout.Button("Generate random cellular automata"))
        {
            gizmoDrawing.Generate();
        }

        if (GUILayout.Button("Meassure"))
        {
            gizmoDrawing.DataMeassure();
        }
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif
#endregion
