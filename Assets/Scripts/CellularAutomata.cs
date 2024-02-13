using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using Unity.VisualScripting;

public class CellularAutomata : GenerationAlgorithm
{

    #region Gizmo Settings
    [Header("DRAWING OPTIONS")]
    public int widthMap;   // Number of columns in the map
    public int heightMap;  // Number of rows in the map
    public int tileSize;    // Size of each tile on the canvas
    public long executionTime;
    #endregion   

    #region CELLULAR SETTINGS
    public float chanceToStartAsWall; // Chance to start as a Wall
    public int numberSteps; // Number of iterations
    public int MIN_CONVERSION_WALL; // Min number of walls the cell must be surrounded to become a wall
    public int MIN_CONVERSION_BLANK; // Min number of empty the cell must be surrounded to become an empty
    public int wallSizeThreshold = 0; // Min region size of walls that can exist
    public int roomSizeThreshold = 0; // Min region size of room that can exist
    #endregion

    private List<List<Coords>> wallRegions;
    private List<List<Coords>> roomRegions;
    private bool[,] map;
    private bool canDraw;

    private enum CELL_TYPE { WALL, ROOM };

    public virtual bool[,] Generate(int seed = -1)
    {
        canDraw = false;
        map = new bool[widthMap, heightMap];
        this.seed = seed;
        GenerateSeed(seed);
        var watch = System.Diagnostics.Stopwatch.StartNew();    // Start meassuring time

        GenerateRandomStart();
        for (int i = 0; i < this.numberSteps; i++)
        {
            simulationStep();
        }
        FilteringProcess(wallSizeThreshold, roomSizeThreshold);
        GenerateCorridors();

        //MeshGenerator meshGen = this.gameObject.GetComponent<MeshGenerator>();
        //meshGen.GenerateMesh(this.map, 1);

        watch.Stop();
        executionTime = watch.ElapsedMilliseconds;
        canDraw = true;
        if(GetAllRegionsOfType(false).Count > 1)
            Debug.LogError(GetAllRegionsOfType(false).Count + "," + this.seed);
        return new bool[widthMap,heightMap];
    }



    private void GenerateRandomStart()
    {
        for (int x = 0; x < widthMap; x++) // Ancho de filas
            for (int y = 0; y < heightMap; y++) // Ancho de columnas
                if (UnityEngine.Random.Range(0.0f, 1.0f) <= chanceToStartAsWall)
                    map[x, y] = true;
    }

    protected int CountNearWalls(int x, int y, int radius)
    {
        int contador = 0;

        for (int i = -1; i <= radius; i++)
            for (int j = -1; j <= radius; j++)
            {
                int casillaX = x + i;
                int casillaY = y + j;

                if (i == 0 && j == 0) { /* We are on the central position */ }

                else if (casillaX < 0 || casillaX >= this.widthMap
                        || casillaY < 0 || casillaY >= this.heightMap)
                {
                    contador++; // Out of bounds count as walls
                }
                else if (map[casillaX, casillaY])
                {
                    contador++;
                }
            }

        return contador;
    }
    protected virtual bool[,] simulationStep()
    {
        bool[,] copyMap = new bool[this.widthMap, this.heightMap];
        for (int x = 0; x < this.widthMap; x++)
        {
            for (int y = 0; y < this.heightMap; y++)
            {
                if (x - 1 < 0 || x + 1 == this.widthMap || y - 1 < 0 || y + 1 >= this.heightMap) // If we are limit walls, we can't be empty
                {
                    copyMap[x, y] = true;
                    continue;
                }
                int numWalls = this.CountNearWalls(x, y, 2);
                if (this.map[x, y])
                {
                    if (numWalls < MIN_CONVERSION_WALL)
                        copyMap[x, y] = false;
                    else
                        copyMap[x, y] = true;
                }
                else
                {
                    if (numWalls > MIN_CONVERSION_BLANK || numWalls == 0)
                        copyMap[x, y] = true;
                    else
                        copyMap[x, y] = false;
                }
            }
        }
        this.map = copyMap;
        return copyMap;
    }

    public void FilteringProcess(int thresholdWalls, int thresholdRooms)
    {
        wallRegions = GetAllRegionsOfType(true);

        foreach (List<Coords> region in new List<List<Coords>>(wallRegions))
        {
            if (region.Count <= thresholdWalls)
            {
                foreach (Coords coord in region)
                {
                    map[coord.X, coord.Y] = false;
                }

                wallRegions.Remove(region);
            }
        }

        roomRegions = GetAllRegionsOfType(false);
        foreach (List<Coords> region in new List<List<Coords>>(roomRegions))
        {
            if (region.Count <= thresholdRooms)
            {
                foreach (Coords coord in region)
                {
                    map[coord.X, coord.Y] = true;
                }
                roomRegions.Remove(region);
            }
        }
    }

    private List<List<Coords>> GetAllRegionsOfType(bool isWall)
    {
        List<List<Coords>> coordsLists = new List<List<Coords>>();
        bool[,] controlMap = new bool[widthMap, heightMap];

        for (int x = 0; x < widthMap; x++)
        {
            for (int y = 0; y < heightMap; y++)
            {
                if (!controlMap[x, y] && this.map[x, y] == isWall)
                {

                    List<Coords> reg = ExpandRegion(x, y);
                    coordsLists.Add(reg);
                    foreach (Coords coord in reg)
                    {
                        controlMap[coord.X, coord.Y] = true;
                    }
                }
            }
        }
        return coordsLists;
    }

    /// <summary>
    /// Given two coordinates on the map, this function starts expanding from that point until reaching a limit of
    /// the map, or encounters cells which are not the same type as the original one. 
    /// </summary>
    /// <param name="firstX"></param>
    /// <param name="firstY"></param>
    /// <returns>Returns a List of Coords with every position that can be directly accesed.</returns>
    private List<Coords> ExpandRegion(int firstX, int firstY)
    {
        //Flood fill algorithm https://en.wikipedia.org/wiki/Flood_fill
        List<Coords> regionCoords = new List<Coords>();
        bool[,] controlMap = new bool[widthMap, heightMap];
        Queue<Coords> coordsQueue = new Queue<Coords>();
        bool cellType = map[firstX, firstY];

        coordsQueue.Enqueue(new Coords(firstX, firstY));
        while (coordsQueue.Count != 0)
        {
            Coords aux = coordsQueue.Dequeue();

            if (aux.X < 0 || aux.X >= this.widthMap || aux.Y < 0 || aux.Y >= this.heightMap
                || controlMap[aux.X, aux.Y] || map[aux.X, aux.Y] != cellType)
                continue;

            regionCoords.Add(aux);
            controlMap[aux.X, aux.Y] = true;
            coordsQueue.Enqueue(new Coords(aux.X + 1, aux.Y));
            coordsQueue.Enqueue(new Coords(aux.X - 1, aux.Y));
            coordsQueue.Enqueue(new Coords(aux.X, aux.Y + 1));
            coordsQueue.Enqueue(new Coords(aux.X, aux.Y - 1));

        }
        return regionCoords;
    }

    private void GenerateCorridors()
    {
        // There are no walls
        if (this.roomRegions.Count == 0)
            return;

        List<Region> regionsList = new List<Region>();

        foreach (List<Coords> region in this.roomRegions)
        {
            regionsList.Add(new Region(region, map));
        }



        RegionDistance[,] distanceMatrix = new RegionDistance[regionsList.Count, regionsList.Count];

        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList.Count; j++)
            {
                if (i == j)
                    continue;
                if (distanceMatrix[i, j].distance == 0)
                {
                    RegionDistance minDistance = DistanceBetweenRegions(regionsList[i], regionsList[j]);
                    distanceMatrix[i, j] = minDistance;
                    distanceMatrix[j, i] = minDistance;
                }
            }
        }

        int index = 0;
        List<int> usedIndex = new List<int>();
        while (usedIndex.Count != regionsList.Count - 1)
        {
            int secondIndex = -1;
            float minRegiondistance = float.MaxValue;
            RegionDistance regDist = new RegionDistance();

            for (int i = 0; i < regionsList.Count; i++)
            {
                //Debug.Log("Se accede:" + index + "," + secondIndex);
                if (distanceMatrix[index, i].distance == 0 || regionsList[index].getConnectedList().Contains(regionsList[i])
                    || usedIndex.Contains(i))
                {
                    //Debug.Log("Es igual:" + index + "," + i);
                    continue;
                }


                if (distanceMatrix[index, i].distance < minRegiondistance)
                {
                    minRegiondistance = distanceMatrix[index, i].distance;
                    regDist = distanceMatrix[index, i];
                    secondIndex = i;
                }
            }

            distanceMatrix[index, secondIndex].distance = 0;
            distanceMatrix[secondIndex, index].distance = 0;
            // Update distance matrix
            for (int i = 0; i < regionsList.Count; i++)
            {
                if (i == index || i == secondIndex)
                    continue;
                if (distanceMatrix[index, i].distance > distanceMatrix[secondIndex, i].distance)
                {
                    distanceMatrix[index, i] = distanceMatrix[secondIndex, i];
                    distanceMatrix[i, index] = distanceMatrix[i, secondIndex];
                }
                else
                {
                    distanceMatrix[secondIndex, i] = distanceMatrix[index, i];
                    distanceMatrix[i, secondIndex] = distanceMatrix[i, index];
                }
            }
            usedIndex.Add(secondIndex);
            DigTunnel(regDist);
            regionsList[index].JoinRegion(regionsList[secondIndex]);
            regionsList[secondIndex].JoinRegion(regionsList[index]);
            index++;
            while (usedIndex.Contains(index) || index >= regionsList.Count)
            {
                index++;
                if (index >= regionsList.Count)
                    index = 0;
            }
        }
    }
    private void DigTunnel(RegionDistance regDist)
    {
        //Bresenham algorithm
        List<Coords> line = new List<Coords>();
        Coords pointA = regDist.posRegionA;
        Coords pointB = regDist.posRegionB;

        int x = pointA.X;
        int y = pointA.Y;

        int dx = pointB.X - pointA.X;
        int dy = pointB.Y - pointA.Y;

        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);
        bool inverted = false;

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }
        int gradientAccumulation = longest / 2;

        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coords(x, y));
            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }
            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }
        Paint(line);

    }

    public void Paint(List<Coords> line)
    {
        int r = 1;
        foreach (Coords coords in line)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        int drawX = coords.X + x;
                        int drawY = coords.Y + y;

                        if (drawX >= 0 && drawX < widthMap && drawY >= 0 && drawY <= heightMap)
                        {
                            map[drawX, drawY] = false;
                        }
                    }
                }
            }
        }
    }

    private RegionDistance DistanceBetweenRegions(Region regionA, Region regionB)
    {
        float minDistanceAbs = float.MaxValue;
        float minDistanceTemp;

        Coords coordsRegionA = new Coords(-1, -1);
        Coords coordsRegionB = new Coords(-1, -1);

        foreach (Coords borderA in regionA.getBorderList())
        {
            foreach (Coords borderB in regionB.getBorderList())
            {
                minDistanceTemp = Mathf.Abs(Mathf.Pow(
                    Mathf.Pow(borderB.X - borderA.X, 2)
                    + Mathf.Pow(borderB.Y - borderA.Y, 2), 2));

                if (minDistanceTemp < minDistanceAbs)
                {
                    minDistanceAbs = minDistanceTemp;
                    coordsRegionA = borderA;
                    coordsRegionB = borderB;
                }

            }
        }

        return new RegionDistance(coordsRegionA, coordsRegionB, minDistanceAbs);
    }

    public struct RegionDistance
    {
        public Coords posRegionA;
        public Coords posRegionB;
        public float distance;

        public RegionDistance(Coords A, Coords B, float distance)
        {
            this.posRegionA = A;
            this.posRegionB = B;
            this.distance = distance;
        }

    }

    public class Region
    {
        private List<Coords> regionCoords;
        private List<Coords> borderCoords;
        private List<Region> connectedRegion;
        private bool[,] map;
        public Region(List<Coords> regionCoords, bool[,] map)
        {
            this.regionCoords = regionCoords;
            this.connectedRegion = new List<Region>();
            this.borderCoords = new List<Coords>();
            this.map = map;
            foreach (Coords coord in regionCoords)
            {
                if (CountNearWalls(coord.X, coord.Y, 1) >= 1)
                {
                    this.borderCoords.Add(coord);
                }
            }

        }
        private int CountNearWalls(int x, int y, int radius)
        {
            int contador = 0;

            for (int i = -1; i <= radius; i++)
                for (int j = -1; j <= radius; j++)
                {
                    int casillaX = x + i;
                    int casillaY = y + j;

                    if (i == 0 && j == 0) { /* We are on the central position */ }

                    else if (casillaX < 0 || casillaX >= this.map.GetLength(0)
                            || casillaY < 0 || casillaY >= this.map.GetLength(1))
                    {
                        contador++; // Out of bounds count as walls
                    }
                    else if (map[casillaX, casillaY])
                    {
                        contador++;
                    }
                }

            return contador;
        }
        public void JoinRegion(Region other)
        {
            this.connectedRegion.Add(other);
            List<Coords> aux = new List<Coords>(other.getBorderList());
            foreach (Coords coord in aux)
                this.borderCoords.Add(coord);
        }
        public List<Coords> getBorderList() { return this.borderCoords; }
        public List<Coords> getRegionList() { return this.regionCoords; }
        public List<Region> getConnectedList() { return this.connectedRegion; }
    }

    public struct Coords
    {

        public Coords(int x, int y, int region = -1)
        {
            X = x;
            Y = y;
            Region = region;
        }

        public int X { get; }
        public int Y { get; }

        public int Region { get; set; }
    }

    private void OnDrawGizmosSelected()
    {
        if (this.map != null && canDraw)
        {
            for (int i = 0; i < heightMap; i++)
                for (int j = 0; j < widthMap; j++)
                {
                    try
                    {
                        if (map[j, i])
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
[CustomEditor(typeof(CellularAutomata))]
public class ScriptEditorCA : Editor
{
    private CellularAutomata gizmoDrawing;

    public override void OnInspectorGUI()
    {
        gizmoDrawing = (CellularAutomata)target;
        gizmoDrawing.widthMap = EditorGUILayout.IntSlider("Width", gizmoDrawing.widthMap, 0, 300);
        gizmoDrawing.heightMap = EditorGUILayout.IntSlider("Height", gizmoDrawing.heightMap, 0, 300);
        gizmoDrawing.tileSize = EditorGUILayout.IntSlider("Tile Size", gizmoDrawing.tileSize, 1, 100);
        EditorGUILayout.FloatField("Execution time (ms)", gizmoDrawing.executionTime);
        //DrawDefaultInspector(); // Draw all public variables
        EditorGUILayout.Space();

        gizmoDrawing.chanceToStartAsWall = EditorGUILayout.Slider("Chance to Start as Wall", gizmoDrawing.chanceToStartAsWall, 0, 1);
        gizmoDrawing.numberSteps = EditorGUILayout.IntField("Number of iterations", gizmoDrawing.numberSteps);
        gizmoDrawing.MIN_CONVERSION_WALL = EditorGUILayout.IntField("Walls around to Convert", gizmoDrawing.MIN_CONVERSION_WALL);
        gizmoDrawing.MIN_CONVERSION_BLANK = EditorGUILayout.IntField("Empty around to Convert", gizmoDrawing.MIN_CONVERSION_BLANK);
        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);

        gizmoDrawing.wallSizeThreshold = EditorGUILayout.IntSlider("Wall size - filtering", gizmoDrawing.wallSizeThreshold, 0, Mathf.Max(gizmoDrawing.widthMap, gizmoDrawing.heightMap));
        gizmoDrawing.roomSizeThreshold = EditorGUILayout.IntSlider("Room size - filtering", gizmoDrawing.roomSizeThreshold, 0, Mathf.Max(gizmoDrawing.widthMap, gizmoDrawing.heightMap));

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
