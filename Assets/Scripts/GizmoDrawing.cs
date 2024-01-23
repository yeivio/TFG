using System;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
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
    /*[Header("OPTIONS FOR CELLULAR AUTOMATA")]
    public float chanceToStartAsWall = .45f; // Chance to start as a Wall
    public int numberSteps = 5; // Number of iterations
    public int MIN_CONVERSION_WALL = 3; // Min number of walls the cell must be surrounded to become a wall
    public int MIN_CONVERSION_BLANK = 5; // Min number of empty the cell must be surrounded to become an empty
    public int seed;*/
    [Header("OPTIONS FOR CELLULAR AUTOMATA")]
    public float chanceToStartAsWall; // Chance to start as a Wall
    public int numberSteps; // Number of iterations
    public int MIN_CONVERSION_WALL; // Min number of walls the cell must be surrounded to become a wall
    public int MIN_CONVERSION_BLANK; // Min number of empty the cell must be surrounded to become an empty
    public int seed;
    #endregion


    public bool[,] mapValue;   // Seed

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
    public void GenerateCellular()
    {
        CellularAutomata cellularAutomata = new CellularAutomata();
        this.mapValue = 
            cellularAutomata.Generate(this.width, this.height, chanceToStartAsWall, numberSteps, MIN_CONVERSION_WALL, MIN_CONVERSION_BLANK, seed);
    }


    public int GenerateRandomCellular()
    {
        CellularAutomata cellularAutomata = new CellularAutomata();
        this.mapValue = cellularAutomata.
                Generate(this.width, this.height, chanceToStartAsWall, numberSteps, MIN_CONVERSION_WALL, MIN_CONVERSION_BLANK);
        this.seed = cellularAutomata.getSeed();
        return cellularAutomata.getSeed();
    }

    public void GenerateBSP()
    {
        
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
        if (GUILayout.Button("Generate BSP"))
        {
            gizmoDrawing.GenerateBSP();
        }
    }
}
#endif
