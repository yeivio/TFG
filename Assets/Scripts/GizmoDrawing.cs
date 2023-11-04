using System;
using UnityEngine;
using UnityEditor;


public class GizmoDrawing : MonoBehaviour
{

    #region Gizmo Settings
    [Header("DRAWING OPTIONS")]
    [Range(1, 100)]
    public int width;   // Max width of the canvas
    [Range(1, 100)]
    public int height;  // Max height of the canvas
    [Range(1, 100)]
    public int tileSize;    // Size of each tile on the canvas
    #endregion


    #region Cellular Automata Settings
    [Header("OPTIONS FOR CELLULAR AUTOMATA")]
    public float chanceToStartAsWall = .45f; // Chance to start as a Wall
    public int numberSteps = 5; // Number of iterations
    public int MIN_CONVERSION_WALL = 3; // Min number of walls the cell must be surrounded to become a wall
    public int MIN_CONVERSION_BLANK = 5; // Min number of empty the cell must be surrounded to become an empty
    public string seed;
    #endregion

    public bool[,] mapValue;   // Seed
    private CellularAutomata cellularAutomata;

    private void OnEnable()
    {
        cellularAutomata = new CellularAutomata();
    }

    private void OnDrawGizmosSelected()
    {
        DrawMap();
    }

    private void DrawMap()
    {
        if (mapValue != null)
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if (mapValue[i, j])
                        Gizmos.color = Color.black;
                    else
                        Gizmos.color = Color.white;
                    Gizmos.DrawCube(new Vector3(tileSize * i, tileSize * j, 0), new Vector3(tileSize, tileSize, 1));
                }
    }
    public void GenerateCellular(){
        Debug.Log("s" + seed);
        if(Int32.TryParse(this.seed , out int parsedSeed)) { 
            this.mapValue = this.cellularAutomata.
                Generate(this.width, this.height, this.chanceToStartAsWall, this.numberSteps, this.MIN_CONVERSION_WALL, this.MIN_CONVERSION_BLANK, parsedSeed);
        }else{
            this.mapValue = this.cellularAutomata.
                Generate(this.width, this.height, this.chanceToStartAsWall, this.numberSteps, this.MIN_CONVERSION_WALL, this.MIN_CONVERSION_BLANK);
        }

        this.seed = this.cellularAutomata.seed;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GizmoDrawing))]
public class ScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GizmoDrawing gizmoDrawing = (GizmoDrawing)target;
        

        DrawDefaultInspector(); // Draw all public variables
        
        if (GUILayout.Button("Generate cellular automata")) {
            gizmoDrawing.GenerateCellular();
        }
    }
}
#endif
