using System;
using UnityEditor;
using UnityEngine;


public abstract class GenerationAlgorithm : MonoBehaviour
{
    #region Gizmo Settings
    public int widthMap;
    public int heightMap;
    public int tileSize;    // Size of each tile on the canvas
    public long executionTime;
    public int seed;
    #endregion

    public CELL_TYPE[,] map;
    public enum CELL_TYPE { NOTHING, WALL, FLOOR, CORRIDOR, DOOR};

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

    public int getSeed() { return seed; }
}
