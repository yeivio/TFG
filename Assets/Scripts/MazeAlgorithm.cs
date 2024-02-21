using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

// 2 3 1528892033
public class MazeAlgorithm : GenerationAlgorithm
{
    #region Gizmo Settings
    [Header("DRAWING OPTIONS")]
    public int widthMap;   // Number of columns in the map
    public int heightMap;  // Number of rows in the map
    public int tileSize;    // Size of each tile on the canvas
    public long executionTime;
    #endregion   
    private Cell[,] map; // True is Used, False is not Used
    public List<Tiles> usableTiles;

    


    public void Generate(int seed = -1)
    {
        // Clean prev output
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }



        this.seed = seed;
        GenerateSeed(seed);

        map = new Cell[widthMap, heightMap];
        for(int i = 0; i < widthMap; i++)
        {
            for (int j = 0; j < heightMap; j++)
            {
                map[i, j] = new Cell();
            }
        }
        int startingX = UnityEngine.Random.Range(0, widthMap);
        int startingY = UnityEngine.Random.Range(0, heightMap);
        while(!allCellsExplored() )
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


        if (Application.isPlaying){
            for (int i = 0; i < heightMap; i++)
                for (int j = 0; j < widthMap; j++)
                {
                    if (map[j, i].isVisited)
                    {
                        SpawnTile(j,i);
                    }
                }
        }
    }

    public void SpawnTile(int x, int y)
    {
        List<Tiles> options = new List<Tiles>(usableTiles);

        // Filter Candidates
        foreach(Cell.ORIENTATION or in Enum.GetValues(typeof(Cell.ORIENTATION)))
        {
            if(!map[x, y].GetConnections().Contains(or)) {
                // Add new tiles
                foreach(Tiles tile in this.usableTiles)
                {
                    switch (or)
                    {
                        case Cell.ORIENTATION.LEFT:
                            if(tile.isLeftConnected)
                                options.Remove(tile);
                            break;
                        case Cell.ORIENTATION.RIGHT:
                            if (tile.isRightConnected)
                                options.Remove(tile);
                            break;
                        case Cell.ORIENTATION.UP:
                            if (tile.isTopConnected)
                                options.Remove(tile);
                            break;
                        case Cell.ORIENTATION.DOWN:
                            if (tile.isBottomConnected)
                                options.Remove(tile);
                            break;
                    }
                }
            }
        }
        // Now we can have Tiles that maybe only satisfy 1 condition
        // We must filter cells now that doesn't satisfy ALL the conditions

        foreach (Cell.ORIENTATION or in Enum.GetValues(typeof(Cell.ORIENTATION)))
        {
            if (map[x, y].GetConnections().Contains(or))
            {
                // Add new tiles
                foreach (Tiles tile in this.usableTiles)
                {
                    switch (or)
                    {
                        case Cell.ORIENTATION.LEFT:
                            if (!tile.isLeftConnected)
                                options.Remove(tile);
                            break;
                        case Cell.ORIENTATION.RIGHT:
                            if (!tile.isRightConnected)
                                options.Remove(tile);
                            break;
                        case Cell.ORIENTATION.UP:
                            if (!tile.isTopConnected)
                                options.Remove(tile);
                            break;
                        case Cell.ORIENTATION.DOWN:
                            if (!tile.isBottomConnected)
                                options.Remove(tile);
                            break;
                    }
                }
            }
        }



        int num = UnityEngine.Random.Range(0, options.Count);
        GameObject aux = new GameObject().gameObject;
        try { 
            aux.AddComponent<SpriteRenderer>().sprite = options[num].GetComponent<SpriteRenderer>().sprite;
        }catch (Exception ex)
        {
            Debug.Log("Seed:" + seed);
        }
        aux.transform.parent = this.transform;
        aux.gameObject.transform.position = new Vector3(x, y, 0);
    }

    public void kill(int x, int y)
    {
        int xIndex = x;
        int yIndex = y;

        this.map[xIndex, yIndex].isVisited = true;
        while (canFindNeighbour(xIndex,yIndex))
        {
            int direction = UnityEngine.Random.Range(0, 4);
            switch(direction)
            {
                case 0:
                    if(xIndex+1 < widthMap && !map[xIndex+1,yIndex].isVisited) {
                        this.map[xIndex, yIndex].setConnection(Cell.ORIENTATION.RIGHT);
                        xIndex++;
                        this.map[xIndex, yIndex].setConnection(Cell.ORIENTATION.LEFT);
                    }
                    break;
                case 1:
                    if (yIndex+ 1 < heightMap && !map[xIndex, yIndex+1].isVisited)
                    {
                        this.map[xIndex, yIndex].setConnection(Cell.ORIENTATION.UP);
                        yIndex++;
                        this.map[xIndex, yIndex].setConnection(Cell.ORIENTATION.DOWN);
                    }
                    break;
                case 2:
                    if (xIndex-1 >= 0 && !map[xIndex-1, yIndex].isVisited)
                    {
                        this.map[xIndex, yIndex].setConnection(Cell.ORIENTATION.LEFT);
                        xIndex--;
                        this.map[xIndex, yIndex].setConnection(Cell.ORIENTATION.RIGHT);
                    }
                        
                    break;
                case 3:
                    if (yIndex - 1 >= 0 && !map[xIndex, yIndex-1].isVisited)
                    {
                        this.map[xIndex, yIndex].setConnection(Cell.ORIENTATION.DOWN);
                        yIndex--;
                        this.map[xIndex, yIndex].setConnection(Cell.ORIENTATION.UP);
                    }
                        
                    break;
            }
            this.map[xIndex, yIndex].isVisited = true;
        }
    }
    public bool canFindNeighbour(int x, int y)
    {
        if ((x + 1 < this.widthMap && !map[x + 1, y].isVisited)
            || (y + 1 < this.heightMap && !map[x, y + 1].isVisited)
            || (x - 1 > 0 && !map[x - 1, y].isVisited)
            || (y - 1 > 0 && !map[x, y - 1].isVisited))
                return true;

        return false;
    }

    public bool ConnectNeighbor(int x, int y)
    {
        if (x+1 < this.widthMap && map[x + 1, y].isVisited)
        {
            map[x, y].setConnection(Cell.ORIENTATION.RIGHT);
            map[x + 1, y].setConnection(Cell.ORIENTATION.LEFT);
            return true;
        }
            
        if (y+1 < this.heightMap&& map[x, y + 1].isVisited)
        {
            map[x, y + 1].setConnection(Cell.ORIENTATION.DOWN);
            map[x,y].setConnection(Cell.ORIENTATION.UP);
            return true;
        }
            
        if (x-1 >= 0 && map[x - 1, y].isVisited)
        {
            map[x, y].setConnection(Cell.ORIENTATION.LEFT);
            map[x - 1, y].setConnection(Cell.ORIENTATION.RIGHT);
            return true;
        }
            
        if (y - 1 >= 0 && map[x, y - 1].isVisited)
        {
            map[x, y - 1].setConnection(Cell.ORIENTATION.UP);
            map[x, y].setConnection(Cell.ORIENTATION.DOWN);
            return true;
        }

        return false;
    }

    public Vector2Int hunt()
    {
        for (int i = 0; i < heightMap; i++)
            for (int j = 0; j < widthMap; j++)
            {
                if (!map[j, i].isVisited && ConnectNeighbor(j, i)) {
                    map[j, i].isVisited = true;
                    return new Vector2Int(j,i);
                }
            }
       return new Vector2Int(-1,-1);
    }


    public bool allCellsExplored()
    {
        for (int i = 0; i < heightMap; i++)
            for (int j = 0; j < widthMap; j++)
            {
                if (!map[j,i].isVisited)
                    return false;
            }

        return true;
    }


    public class Cell
    {
        public bool isVisited;
        public List<Cell.ORIENTATION> connections;
        public enum ORIENTATION {UP, DOWN, LEFT, RIGHT}

        public Cell()
        {
            isVisited = false;
            connections = new List<Cell.ORIENTATION>();
        }

        public void setConnection(ORIENTATION or)
        {
            connections.Add(or);
        }

        public List<ORIENTATION> GetConnections()
        {
            return this.connections;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (this.map != null && !Application.isPlaying)
        {
            for (int i = 0; i < heightMap; i++)
                for (int j = 0; j < widthMap; j++)
                {
                    try
                    {
                        if (map[j, i].isVisited)
                            Gizmos.color = Color.black;
                        else
                            Gizmos.color = Color.white;
                        Gizmos.DrawCube(new Vector3(tileSize * j + 0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                        DrawBorder(map[j,i],j ,i);
                        
                    }
                    catch
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(new Vector3(tileSize * j + 0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                    }
                }
        }
    }

    private void DrawBorder(Cell cell, int j, int i)
    {
        Gizmos.color = Color.red;
        foreach (Cell.ORIENTATION or in cell.GetConnections())
        {
            switch (or)
            {
                case Cell.ORIENTATION.LEFT:
                    Gizmos.DrawSphere(new Vector3((float)(j + 0.5 * tileSize), i, 0), 0.1f);
                    break;
                case Cell.ORIENTATION.RIGHT:
                    Gizmos.DrawSphere(new Vector3((float)(j + tileSize), (float)(i + 0.5 * tileSize), 0), 0.1f);
                    break;
                case Cell.ORIENTATION.UP:
                    Gizmos.DrawSphere(new Vector3((float)(j + 0.5 * tileSize), (float)(i + tileSize), 0), 0.1f);
                    break;
                case Cell.ORIENTATION.DOWN:
                    Gizmos.DrawSphere(new Vector3((float)(j + 0.5 * tileSize), i, 0), 0.1f);
                    break;
            }
        }
        
    }

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
        gizmoDrawing.widthMap = EditorGUILayout.IntSlider("Width", gizmoDrawing.widthMap, 0, 300);
        gizmoDrawing.heightMap = EditorGUILayout.IntSlider("Height", gizmoDrawing.heightMap, 0, 300);
        gizmoDrawing.tileSize = EditorGUILayout.IntSlider("Tile Size", gizmoDrawing.tileSize, 1, 100);
        EditorGUILayout.FloatField("Execution time (ms)", gizmoDrawing.executionTime);
        //DrawDefaultInspector(); // Draw all public variables
        EditorGUILayout.Space();
        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("usableTiles"), new GUIContent("Sprite"));

        if (GUILayout.Button("Generate cellular automata"))
        {
            gizmoDrawing.Generate(gizmoDrawing.seed);

        }

        if (GUILayout.Button("Generate random cellular automata"))
        {
            gizmoDrawing.Generate();
        }
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif
#endregion
