using System;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GizmoDrawing : MonoBehaviour
{

    // True es Pared
    // False es Libre

    #region Gizmo Settings
    [Header("DRAWING OPTIONS")]
    public int columnNum;   // Number of columns in the map
    public int rowNum;  // Number of rows in the map
    public int tileSize;    // Size of each tile on the canvas
    public float executionTime;
    #endregion

    #region Cellular Automata Settings
    
    [Header("OPTIONS FOR CELLULAR AUTOMATA")]
    public float chanceToStartAsWall; // Chance to start as a Wall
    public int numberSteps; // Number of iterations
    public int MIN_CONVERSION_WALL; // Min number of walls the cell must be surrounded to become a wall
    public int MIN_CONVERSION_BLANK; // Min number of empty the cell must be surrounded to become an empty
    public int seed;
    #endregion

    #region BSPTree Settings

    [Header("OPTIONS FOR BSPTree")]
    public int min_room_width;
    public int max_room_width;
    public int min_room_height;    
    public int max_room_height;
    #endregion


    public bool[,] mapValue;   // Seed

    private void OnDrawGizmosSelected()
    {
        DrawMap();
    }

    private void DrawMap()
    {
        if (mapValue != null)
            for (int i = 0; i < rowNum ; i++)
                for (int j = 0; j < columnNum; j++)
                {
                    try
                    {
                        if (mapValue[j, i])
                            Gizmos.color = Color.black;
                        else
                            Gizmos.color = Color.white;
                        Gizmos.DrawCube(new Vector3(tileSize * i+0.5f, tileSize * j + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                    }catch
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(new Vector3(tileSize * i + 0.5f, tileSize * j + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                    }
                }
    }
    public void GenerateCellular()
    {
        CellularAutomata cellularAutomata = new CellularAutomata();
        var watch = System.Diagnostics.Stopwatch.StartNew();    // Start meassuring time
        this.mapValue = cellularAutomata.Generate(this.columnNum, this.rowNum, chanceToStartAsWall, numberSteps, MIN_CONVERSION_WALL, MIN_CONVERSION_BLANK, seed); ;
        watch.Stop();
        executionTime = watch.ElapsedMilliseconds;
    }


    public int GenerateRandomCellular()
    {
        
        CellularAutomata cellularAutomata = new CellularAutomata();
        var watch = System.Diagnostics.Stopwatch.StartNew();    // Start meassuring time
        this.mapValue =
            cellularAutomata.Generate(this.columnNum, this.rowNum, chanceToStartAsWall, numberSteps, MIN_CONVERSION_WALL, MIN_CONVERSION_BLANK);
        watch.Stop();
        executionTime = watch.ElapsedMilliseconds;
        this.seed = cellularAutomata.getSeed();
        return cellularAutomata.getSeed();
        
    }

    public void GenerateBSP()
    { 

        BSPTree tree = new BSPTree();
        var watch = System.Diagnostics.Stopwatch.StartNew();    // Start meassuring time
        this.mapValue = tree.Generate(this.columnNum, this.rowNum, min_room_width, min_room_height,max_room_width, max_room_height, seed);
        watch.Stop();
        executionTime = watch.ElapsedMilliseconds;
        this.seed = tree.getSeed();

        

    }

    public void GenerateRandomBSP()
    {
        

        BSPTree tree = new BSPTree();
        var watch = System.Diagnostics.Stopwatch.StartNew();    // Start meassuring time
        this.mapValue = tree.Generate(this.columnNum, this.rowNum, min_room_width, min_room_height, max_room_width, max_room_height);
        watch.Stop();
        executionTime = watch.ElapsedMilliseconds;
        this.seed = tree.getSeed();

       
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(GizmoDrawing))]
public class ScriptEditor : Editor
{
    private string[] tab_names = { "Cellular Automata", "BSPTrees" };
    private int currentSelected = -1;

    public override void OnInspectorGUI()
    {
        GizmoDrawing gizmoDrawing = (GizmoDrawing)target;
        gizmoDrawing.columnNum = EditorGUILayout.IntSlider("Width", gizmoDrawing.columnNum, 0, 300);
        gizmoDrawing.rowNum = EditorGUILayout.IntSlider("Height", gizmoDrawing.rowNum, 0, 300);
        gizmoDrawing.tileSize = EditorGUILayout.IntSlider("Tile Size", gizmoDrawing.tileSize, 0, 100);
        EditorGUILayout.FloatField("Execution time (ms)", gizmoDrawing.executionTime);
        //DrawDefaultInspector(); // Draw all public variables
        EditorGUILayout.BeginVertical();
        currentSelected = GUILayout.SelectionGrid(currentSelected, tab_names,2);
        EditorGUILayout.EndVertical();

        switch (currentSelected)
        {
            case 0:
                TabCellularAutomata();
                break;

            case 1:
                TabBSPTree();
                break;

        }

        if(GUI.changed)
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    private void TabCellularAutomata()
    {
        GizmoDrawing gizmoDrawing = (GizmoDrawing)target;

        gizmoDrawing.chanceToStartAsWall = EditorGUILayout.Slider("Chance to Start as Wall",gizmoDrawing.chanceToStartAsWall, 0, 1);
        gizmoDrawing.numberSteps = EditorGUILayout.IntField("Number of iterations", gizmoDrawing.numberSteps);
        gizmoDrawing.MIN_CONVERSION_WALL = EditorGUILayout.IntField("Walls around to Convert", gizmoDrawing.MIN_CONVERSION_WALL);
        gizmoDrawing.MIN_CONVERSION_BLANK = EditorGUILayout.IntField("Empty around to Convert", gizmoDrawing.MIN_CONVERSION_BLANK);
        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);

        if (GUILayout.Button("Generate cellular automata")) 
        {
                gizmoDrawing.GenerateCellular();
        }

        if (GUILayout.Button("Generate random cellular automata"))
        {
                gizmoDrawing.GenerateRandomCellular();
        }
    }

    private void TabBSPTree()
    {
        GizmoDrawing gizmoDrawing = gizmoDrawing = (GizmoDrawing)target;
        gizmoDrawing.min_room_width = EditorGUILayout.IntField("Min Room Width", gizmoDrawing.min_room_width);
        gizmoDrawing.max_room_width = EditorGUILayout.IntField("Max Room Width", gizmoDrawing.max_room_width);

        gizmoDrawing.min_room_height= EditorGUILayout.IntField("Min Room Height", gizmoDrawing.min_room_height);
        gizmoDrawing.max_room_height = EditorGUILayout.IntField("Max Room Height", gizmoDrawing.max_room_height);

        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);

        if (GUILayout.Button("Generate BSP"))
        {
            gizmoDrawing.GenerateBSP();
        }
        if (GUILayout.Button("Generate Random BSP"))
        {
            gizmoDrawing.GenerateRandomBSP();
        }
    }
}
#endif
