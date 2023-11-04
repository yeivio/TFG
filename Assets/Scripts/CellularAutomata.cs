using System;
using UnityEngine;

public class CellularAutomata
{
    private float chanceToStartAsWall = .45f; // Chance to start as a Wall
    private float numberSteps = 5; // Number of iterations
    private int MIN_CONVERSION_WALL = 3; // Min number of walls the cell must be surrounded to become a wall
    private int MIN_CONVERSION_BLANK = 5; // Min number of empty the cell must be surrounded to become an empty
    
    private bool[,] map; // False == Empty | True == Wall
    private int widthMap;
    private int heightMap;

    public string seed;


    public bool[,] Generate(int width, int height, float wallStart, int numberSteps, int conversionWall, int conversionBlank, int seed = -1)
    {
        this.chanceToStartAsWall = wallStart;
        this.numberSteps = numberSteps;
        this.MIN_CONVERSION_WALL = conversionWall;
        this.MIN_CONVERSION_BLANK = conversionBlank;
        this.widthMap = width;
        this.heightMap = height;
        map = new bool[width, height];
        this.seed = seed.ToString();
        Debug.Log("llega" + seed);
        GenerateSeed(seed);    
        GenerateRandomStart();

        for (int i = 0; i < this.numberSteps; i++)
        {
            simulationStep();
        }
        return this.map;
    }

    private void GenerateRandomStart()
    {
        for (int x = 0; x < widthMap; x++) // Ancho de filas
            for (int y = 0; y < heightMap; y++) // Ancho de columnas
                if (UnityEngine.Random.Range(0.0f,1.0f) < chanceToStartAsWall)
                    map[x,y] = true;
        
    }

    private int CountNearWalls(int x, int y)
    {
        int contador = 0;
        
        for(int i = -1; i < 2; i++)
            for(int j = -1; j< 2; j++)
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

    private bool[,] simulationStep()
    {
        bool[,] copyMap = new bool[this.widthMap, this.heightMap];
        for(int x = 0; x < this.widthMap; x++) { 
            for(int y = 0; y < this.heightMap; y++)
            {
                int numWalls = this.CountNearWalls(x,y);

                if (this.map[x, y])
                {
                    if (numWalls < MIN_CONVERSION_WALL)
                        copyMap[x,y] = false;
                    else
                        copyMap[x, y] = true;
                }
                else
                {
                    if (numWalls > MIN_CONVERSION_BLANK)
                        copyMap[x, y] = true;
                    else
                        copyMap[x, y] = false;
                }
            }
        }
        this.map = copyMap;
        return copyMap;
    }

    private void GenerateSeed(int seed = -1)
    {
        int tempSeed = (int)DateTime.Now.Ticks;
        if (seed == -1) // No seed 
        {
            Debug.Log("Se resetea la seed" + tempSeed);
            UnityEngine.Random.InitState(tempSeed);
            this.seed = tempSeed.ToString();
        }
        else
            UnityEngine.Random.InitState(seed);
    }
}
