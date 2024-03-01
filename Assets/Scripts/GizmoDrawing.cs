using System;
using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class GizmoDrawing : MonoBehaviour
{

    // True es Pared
    // False es Libre

    #region Gizmo Settings
    [Header("DRAWING OPTIONS")]
    public int mapWidth;   // Number of columns in the cellMap
    public int mapHeight;  // Number of rows in the cellMap
    public int tileSize;    // Size of each tile on the canvas
    public float executionTime;
    #endregion

    #region Cellular Automata Settings
    
    [Header("OPTIONS FOR CELLULAR AUTOMATA")]
    public float chanceToStartAsWall = 0; // Chance to start as a Wall
    public int numberSteps= 0; // Number of iterations
    public int MIN_CONVERSION_WALL = 0; // Min number of walls the cell must be surrounded to become a wall
    public int MIN_CONVERSION_BLANK = 0; // Min number of empty the cell must be surrounded to become an empty
    public int seed = 0;
    public int wallSizeThreshold = 0; // Min region size of walls that can exist
    public int roomSizeThreshold = 0; // Min region size of room that can exist
    #endregion

    #region BSPTree Settings
    [Header("OPTIONS FOR BSPTree")]
    public int min_room_width;
    public int max_room_width;
    public int min_room_height;    
    public int max_room_height;
    #endregion

    public enum TILE_TYPE { EMPTY, WALL }

    public bool[,] mapValue;   // Seed

    private void OnDrawGizmosSelected()
    {
        DrawMap();
    }

    private void DrawMap()
    {
        if (mapValue != null) { 
            for (int i = 0; i < mapHeight ; i++)
                for (int j = 0; j < mapWidth; j++)
                {
                    try
                    {
                        if (mapValue[j,i])
                            Gizmos.color = Color.black;
                        else
                            Gizmos.color = Color.white;
                        Gizmos.DrawCube(new Vector3(tileSize * j+0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                    }catch
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(new Vector3(tileSize * j + 0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                    }
                }
        }
    }

    public void GenerateBSP()
    { 
        BSPTree tree = new BSPTree();
        var watch = System.Diagnostics.Stopwatch.StartNew();    // Start meassuring time
        //this.mapValue = tree.Generate(this.mapWidth, this.mapHeight, min_room_width, min_room_height,max_room_width, max_room_height, seed);
        watch.Stop();
        executionTime = watch.ElapsedMilliseconds;
        this.seed = tree.seed;
    }

    public void GenerateRandomBSP()
    {
        BSPTree tree = new BSPTree();
        var watch = System.Diagnostics.Stopwatch.StartNew();    // Start meassuring time
        //this.mapValue = tree.Generate(this.mapWidth, this.mapHeight, min_room_width, min_room_height, max_room_width, max_room_height);
        watch.Stop();
        executionTime = watch.ElapsedMilliseconds;
        this.seed = tree.seed;
    }



}


