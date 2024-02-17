using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System;

public class WaveFunctionCollapse : GenerationAlgorithm
{
    #region Gizmo Settings
    [Header("DRAWING OPTIONS")]
    public int widthMap;   // Number of columns in the map
    public int heightMap;  // Number of rows in the map
    public int tileSize;    // Size of each tile on the canvas
    public long executionTime;
    public List<Tiles> tiles;
    #endregion   

    private Cell evaluatingObj;
    private Cell[,] grid;
    

    private int startingX,startingY;

    private void Awake()
    {
        for(int i = 0; i < widthMap; i++) {
            for (int j = 0; j < heightMap; j++)
            {
                grid[i,j] = new Cell();
            }
        }


        startingX =UnityEngine.Random.Range(0, widthMap);
        startingY = UnityEngine.Random.Range(0, heightMap);
        
        grid[startingX, startingY] = new Cell(0); // First one is collapsed
    }


    private void Update()
    {
        Cell[,] auxMatrix = new Cell[widthMap,heightMap];
        Array.Copy(grid, auxMatrix,auxMatrix.Length);

        for (int i = 0; i < widthMap; i++)
        {
            for (int j = 0; j < heightMap; j++)
            {
                if (!grid[i,j].collapsed) // We find a non-collapsed object
                {
                    if(j > 0 && j +1 < this.widthMap)
                    {
                        Cell up = grid[i, j + 1];
                        
                    }
                }
            }
        }

    }

}





public class Cell
{
    public bool collapsed;
    public List<int> options; // Index of the global list of tiles

    public Cell() {  collapsed = false; this.options = new List<int>(); }
    public Cell(int tile) { collapsed = true; this.options = new List<int>(); options.Add(tile);  }
    public Cell(bool collapsed, List<int> tiles)
    {
        this.collapsed = collapsed;
        this.options = tiles;
    }
}
