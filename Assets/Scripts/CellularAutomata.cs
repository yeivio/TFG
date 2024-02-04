using System.Collections.Generic;
using UnityEngine;
using System;

// 50 50 0.62 3 5 5 -272735705 Bug
// 21 27 0.699 3 5 5 -1478784931
// 29 17 0.227 3 5 5 -1083805850
public class CellularAutomata : GenerationAlgorithm
{
    private float chanceToStartAsWall; // Chance to start as a Wall
    private float numberSteps; // Number of iterations
    protected int MIN_CONVERSION_WALL; // Min number of walls the cell must be surrounded to become a wall
    protected int MIN_CONVERSION_BLANK; // Min number of empty the cell must be surrounded to become an empty


    public virtual bool[,] Generate(int width, int height, float wallStart, int numberSteps, int conversionWall, int conversionBlank, int seed = -1)
    {
        this.chanceToStartAsWall = wallStart;
        this.numberSteps = numberSteps;
        this.MIN_CONVERSION_WALL = conversionWall;
        this.MIN_CONVERSION_BLANK = conversionBlank;
        this.widthMap = width;
        this.heightMap = height;
        map = new bool[width, height];
        this.seed = seed.ToString();
        GenerateSeed(seed);
        GenerateRandomStart();
        for (int i = 0; i < this.numberSteps; i++)
        {
            simulationStep();
        }
        
        //FilteringProcess(0, 5);
        GenerateCorridors();

        Debug.Log(GetAllRegionsOfType(false).Count + ","+ this.seed);
        return this.map;
    }

  

    private void GenerateRandomStart()
    {
        for (int x = 0; x < widthMap; x++) // Ancho de filas
            for (int y = 0; y < heightMap; y++) // Ancho de columnas
                if (UnityEngine.Random.Range(0.0f,1.0f) <= chanceToStartAsWall)
                    map[x, y] = true;
    }

    protected int CountNearWalls(int x, int y, int radius)
    {
        int contador = 0;
        
        for(int i = -1; i <= radius; i++)
            for(int j = -1; j<= radius; j++)
            {
                int casillaX = x + i;
                int casillaY = y + j;

                if(i == 0 && j == 0) { /* We are on the central position */ } 
                
                else if(casillaX < 0 || casillaX >= this.widthMap
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
        for(int x = 0; x < this.widthMap; x++) { 
            for(int y = 0; y < this.heightMap; y++)
            {
                int numWalls = this.CountNearWalls(x, y, 2);
                if (this.map[x, y])
                {
                    if (numWalls < MIN_CONVERSION_WALL)
                        copyMap[x,y] = false;
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
        int wallThresholdsize = thresholdWalls;
        int roomThresholdsize = thresholdRooms;

        List<List<Coords>> wallRegions = GetAllRegionsOfType(true);
        

        foreach (List<Coords> region in wallRegions)
        {
            if(region.Count <= wallThresholdsize)
            {
                foreach(Coords coord in region)
                {
                    map[coord.X, coord.Y] = false;
                }
            }
        }

        List<List<Coords>> roomRegions = GetAllRegionsOfType(false);
        foreach (List<Coords> region in roomRegions)
        {
            if(region.Count <= roomThresholdsize)
            {
                foreach(Coords coord in region)
                {
                    map[coord.X, coord.Y] = true;
                }
            }
        }
        
        //Debug.Log("A" + roomRegions.Count);
    }

    private List<List<Coords>> GetAllRegionsOfType(bool isWall)
    {
        List<List<Coords>> coordsLists = new List<List<Coords>>();
        bool[,] controlMap = new bool[widthMap, heightMap];

        for(int x = 0; x< widthMap; x++)
        {
            for(int y = 0; y< heightMap; y++)
            {
                if (!controlMap[x, y] && this.map[x,y] == isWall)
                {
                    
                    List<Coords> reg = ExpandRegion(x, y);
                    coordsLists.Add(reg);
                    foreach(Coords coord in reg)
                    {
                        controlMap[coord.X, coord.Y] = true;
                    }
                }
            }
        }
        return coordsLists;
    }


    private List<Coords> ExpandRegion(int firstX, int firstY)
    {

        List<Coords> regionCoords = new List<Coords>();
        bool[,] controlMap= new bool[widthMap,heightMap];
        Queue<Coords> coordsQueue = new Queue<Coords>();
        bool cellType = map[firstX, firstY];

        coordsQueue.Enqueue(new Coords(firstX, firstY));
        while(coordsQueue.Count != 0)
        {
            Coords aux  = coordsQueue.Dequeue();
            
            
            if (aux.X < 0 || aux.X >= this.widthMap || aux.Y < 0 || aux.Y >= this.heightMap 
                || controlMap[aux.X,aux.Y] || map[aux.X,aux.Y] != cellType)
            {
                
            }
            else
            {
                //Debug.Log("se añaden coords:" + aux.X + "," + aux.Y);
                regionCoords.Add(aux);
                controlMap[aux.X, aux.Y] = true;
                coordsQueue.Enqueue(new Coords(aux.X + 1, aux.Y));
                coordsQueue.Enqueue(new Coords(aux.X - 1, aux.Y));
                coordsQueue.Enqueue(new Coords(aux.X, aux.Y + 1));
                coordsQueue.Enqueue(new Coords(aux.X, aux.Y - 1));
            }
        }
        //Debug.Log("se termina region");
        return regionCoords;
    }

    private void GenerateCorridors()
    {
        List<List<Coords>> wallRegions = GetAllRegionsOfType(false);
        if (wallRegions.Count == 0)
            return;



        List<Region> regionsList = new List<Region>();

        foreach (List<Coords> region in wallRegions)
        {
            regionsList.Add(new Region(region, map));
        }



        RegionDistance[,] distanceMatrix = new RegionDistance[regionsList.Count,regionsList.Count];

        for (int i = 0; i < regionsList.Count; i++)
        {
            for (int j = 0; j < regionsList.Count; j++)
            {
                if (i == j)
                    continue;
                if (distanceMatrix[i,j].distance == 0)
                {
                    RegionDistance minDistance = DistanceBetweenRegions(regionsList[i], regionsList[j]);
                    distanceMatrix[i, j] = minDistance;
                    distanceMatrix[j, i] = minDistance;
                }
            }
        }
        
        int index = 0;
        List<int> usedIndex = new List<int>();
        while(usedIndex.Count != regionsList.Count - 1 )
        {
            int secondIndex = -1;
            float minRegiondistance = float.MaxValue;
            RegionDistance regDist = new RegionDistance();

            for(int i = 0; i < regionsList.Count; i++)
            {
                //Debug.Log("Se accede:" + index + "," + secondIndex);
                if (distanceMatrix[index, i].distance == 0 || regionsList[index].getConnectedList().Contains(regionsList[i])
                    || usedIndex.Contains(i)) 
                {
                    //Debug.Log("Es igual:" + index + "," + i);
                    continue;
                }
                    

                if (distanceMatrix[index,i].distance < minRegiondistance)
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
                if (distanceMatrix[index,i].distance > distanceMatrix[secondIndex, i].distance)
                {
                    distanceMatrix[index,i] = distanceMatrix[secondIndex,i];
                    distanceMatrix[i,index] = distanceMatrix[i,secondIndex];
                }
                else
                {
                    distanceMatrix[secondIndex, i] = distanceMatrix[index, i];
                    distanceMatrix[i, secondIndex] = distanceMatrix[i, index];
                }
            }


            usedIndex.Add(secondIndex);
            //Debug.Log("Se unen:" + distanceMatrix[index, secondIndex].posRegionA + " y " + distanceMatrix[index, secondIndex].posRegionB + ",quedan:" + regionsList.Count);
            //Debug.Log("Se añade a indices usados el:" + secondIndex);
            DigTunnel(regDist);
            regionsList[index].JoinRegion(regionsList[secondIndex]);
            regionsList[secondIndex].JoinRegion(regionsList[index]);
            //regionsList.Remove(regionsList[secondIndex]);
            //Debug.Log(index + "," + usedIndex.Contains(index));
            index++;
            while (usedIndex.Contains(index) || index >= regionsList.Count)
            {
                index++;
                if (index >= regionsList.Count)
                    index = 0;
                
            }
            //Debug.Log("siguiente index:" + index);

        }

    }
    
    /*private void DigTunnel(RegionDistance regDist)
    {
        Coords pointA = regDist.posRegionA;
        Coords pointB = regDist.posRegionB;
        //Debug.Log("Se calcula tunel de:" + pointA + pointB);
        if(pointB.X == pointA.X)
        {
            for (int j = Mathf.Min(pointA.Y, pointB.Y); j <= Mathf.Max(pointA.Y, pointB.Y); j++)
            {
                map[pointB.X, j] = false;
                if (j + 1 < this.widthMap)
                    map[pointB.X, (j+ 1)] = false;
                else
                    map[pointB.X, (j - 1)] = false;
            }
        }
        else
        {
            float m = (float)(pointB.Y - pointA.Y) / (float)(pointB.X - pointA.X);

            float posY;
            float lastPos = -1;

            for (float i = Mathf.Min(pointA.X, pointB.X); i <= Mathf.Max(pointA.X, pointB.X); i += 0.1f)
            {
                //posY = (int)(m * i - (m * pointA.X + pointA.Y));
                posY = ((m*(i-pointA.X))+pointA.Y);
                //Debug.Log("Cavo:" + (int)i + "," + (int)posY);   
                map[(int)i, (int)posY] = false;

                if(lastPos != -1)
                {
                    
                }
                lastPos = posY;
            }
        }

        
    }*/

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

        if(longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }
        int gradientAccumulation = longest / 2;

        for(int i = 0; i< longest; i++)
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
            if(gradientAccumulation>= longest)
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
        foreach(Coords coords in line)
        {
            for(int x = -r; x <= r; x++)
            {
                for(int y = -r; y <= r; y++)
                {
                    if(x*x + y*y <= r * r)
                    {
                        int drawX = coords.X + x;
                        int drawY = coords.Y + y;

                        if(drawX >= 0 && drawX < widthMap && drawY >= 0 && drawY <= heightMap)
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

        Coords coordsRegionA = new Coords(-1,-1);
        Coords coordsRegionB = new Coords(-1,-1);

        foreach (Coords borderA in regionA.getBorderList())
        {
            foreach (Coords borderB in regionB.getBorderList())
            {
                minDistanceTemp = Mathf.Abs(Mathf.Pow(
                    Mathf.Pow(borderB.X - borderA.X, 2) 
                    + Mathf.Pow(borderB.Y - borderA.Y,2),2));

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
            foreach(Coords coord in aux)
                this.borderCoords.Add(coord);
        }
        public List<Coords> getBorderList(){ return this.borderCoords; }
        public List<Coords> getRegionList(){ return this.regionCoords; }
        public List<Region> getConnectedList(){ return this.connectedRegion; }
    }

    public struct Coords
    {

        public Coords(int x, int y, int region=-1)
        {
            X = x;
            Y = y;
            Region = region;
        }

        public int X { get; }
        public int Y { get; }

        public int Region { get; set; }

        public override string ToString() => $"({X}, {Y})";
    }
}


