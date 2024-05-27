using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;


public abstract class GenerationAlgorithm : MonoBehaviour
{
    #region Gizmo Settings
    public int widthMap;
    public int heightMap;
    public int tileSize;    // Size of each tile on the canvas
    public long executionTime; // in ms
    public long memoryConsumption; // in bytes
    public int seed;
    #endregion

    protected CELL_TYPE[,] map;
    public enum CELL_TYPE { NOTHING, WALL, FLOOR, CORRIDOR, DOOR };

    public int startGridSize = 50;
    public int endGridSize = 400;
    public int intervalGridSize = 50;

    protected void GenerateSeed(int seed = -1)
    {
        int tempSeed = (int)DateTime.Now.Ticks;
        if (seed == -1) // No seed 
        {
            UnityEngine.Random.InitState(tempSeed);
            this.seed = tempSeed;
        }
        else
            UnityEngine.Random.InitState(seed);
    }

    public abstract void Generate(int seed = -1);
    public int getSeed() { return seed; }
    public CELL_TYPE[,] getMap() { return this.map; }

    public CELL_TYPE[,] getDrawMap()
    {
        CELL_TYPE[,] returnmap = new CELL_TYPE[widthMap + 2, heightMap + 2];

        for (int x = 0; x < widthMap + 2; x++)
            for (int y = 0; y < heightMap + 2; y++)
            {
                
                if (x == 0 || y == 0 || x == widthMap + 1 || y == heightMap + 1)
                {
                    UnityEngine.Debug.Log("1:" + x + "," + y);
                    returnmap[x,y] = CELL_TYPE.WALL;
                }
                else
                {
                    UnityEngine.Debug.Log("2:" + x + "," + y);
                    int i = x - 1;
                    int j = y - 1;
                    UnityEngine.Debug.Log("map:" + i + "," + j);
                    returnmap[x, y] = map[i, j];
                }
            }

        return returnmap;
    }

    protected void OnDrawGizmos()
    {
        if (this.map != null)
        {
            for (int x = 0; x <= widthMap + 1; x++)
                for (int y = 0; y <= heightMap + 1; y++)
                {
                    if (x == 0 || y == 0 || x >= widthMap + 1 || y >= heightMap + 1)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawCube(new Vector3(tileSize * x + 0.5f, tileSize * y + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                    }
                    else
                    {
                        int i = x - 1;
                        int j = y - 1;
                        try
                        {
                            switch (map[i, j])
                            {
                                case CELL_TYPE.WALL:
                                    Gizmos.color = Color.black;
                                    break;
                                case CELL_TYPE.FLOOR:
                                    Gizmos.color = Color.white;
                                    break;
                                case CELL_TYPE.CORRIDOR:
                                    Gizmos.color = Color.grey;
                                    break;
                                case CELL_TYPE.NOTHING:
                                    Gizmos.color = Color.red;
                                    break;
                            }
                            Gizmos.DrawCube(new Vector3(tileSize * x + 0.5f, tileSize * y + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                            //Gizmos.DrawCube(new Vector3(tileSize * i + 0.5f, tileSize * j + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                        }
                        catch
                        {
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawCube(new Vector3(tileSize * x + 0.5f, tileSize * y + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                            //Gizmos.DrawCube(new Vector3(tileSize * i + 0.5f, tileSize * j + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                        }
                    }
                }
        }
    }
}
