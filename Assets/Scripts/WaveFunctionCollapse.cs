using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class WaveFunctionCollapse : GenerationAlgorithm
{
    public List<Tiles> usableTiles;
    public Tiles DefaultTile;
    private Cell[,] cellMap;   // Map

    public override void Generate(int seed = -1)
    {

        // Clean prev output
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }

        this.seed = seed;
        GenerateSeed(seed);

        CleanMap();
        int randomCell;
        while (CheckEnd())
        {
            List<Cell> sortedTiles = SortedTiles(); // Find the tiles with the lowest entropy
            randomCell = UnityEngine.Random.Range(0, sortedTiles.Count); // Pick a random cell
            Cell actualCell = sortedTiles[randomCell];
            if(actualCell.options.Count == 0 ) // Doesn't have any tile options
            {
                actualCell.collapsed = true;
                actualCell.options = new List<Tiles>() { DefaultTile };
            }
            else
            {
                //Collapse
                actualCell.collapsed = true;
                Tiles seletedTile = actualCell.options[UnityEngine.Random.Range(0, actualCell.options.Count)];
                actualCell.options = new List<Tiles>() { seletedTile };
            }

            


            UpdateMap(actualCell);
        }

        if (Application.isPlaying)
        {
            for (int i = 0; i < heightMap; i++)
                for (int j = 0; j < widthMap; j++)
                {
                    SpawnTile(j, i);
                }
        }
    }

    public void SpawnTile(int x, int y)
    {
        GameObject aux = new GameObject().gameObject;
        try
        {
            aux.AddComponent<SpriteRenderer>().sprite = cellMap[x, y].options[0].GetComponent<SpriteRenderer>().sprite;
        }
        catch (Exception)
        {
            Debug.Log("Seed:" + seed);
        }
        aux.transform.parent = this.transform;
        aux.gameObject.transform.position = new Vector3(x, y, 0);
    }

    public bool CheckEnd()
    {
        bool notCollapsed = false;
        for (int i = 0; i < widthMap; i++)
        {
            for(int j = 0; j < heightMap; j++)
            {
                if (!cellMap[i, j].collapsed) { 
                notCollapsed = true;
                    break;
                }
            }
        }
        return notCollapsed;
    }

    public List<Cell> SortedTiles()
    {
        List<Cell> aux = new List<Cell>();
        for (int i = 0; i < widthMap; i++)
        {
            for (int j = 0; j < heightMap; j++)
            {
                if (!cellMap[i, j].collapsed)
                {
                    aux.Add(cellMap[i,j]);
                }
            }
        }
        aux.Sort((a, b) => { return a.options.Count - b.options.Count; });
        aux.RemoveAll(x => x.options.Count != aux[0].options.Count);

        return aux;
    }
    private void CleanMap()
    {
        cellMap = new Cell[widthMap, heightMap];
        for(int i = 0; i < widthMap; i++) {
            for (int j = 0; j < heightMap; j++)
            {
                cellMap[i,j] = new Cell(usableTiles, i, j);
            }
        }
    }
    public void CheckValidity(List<Tiles> optionList, List<Tiles> validOption)
    {
        for( int i = optionList.Count - 1; i >= 0 ; i--)
        {
            Tiles aux = optionList[i];
            if (!validOption.Contains(aux))
            {
                optionList.RemoveAt(i);
            }
        }
    }
    public void UpdateMap(Cell cell)
    {
        // Update bottom cell
        if( cell.yPos > 0 && !cellMap[cell.xPos, cell.yPos -1 ].collapsed)
        {
            bool valid;
            foreach(Tiles tilOption in new List<Tiles>(cellMap[cell.xPos, cell.yPos - 1].options))
            {
                valid = false;
                foreach(Tiles tile in tilOption.TopPosibilities)
                {
                    if(cell.options[0].GetComponent<SpriteRenderer>().sprite == tile.GetComponent<SpriteRenderer>().sprite)
                    {
                        valid = true;
                    }
                }
                if (!valid)
                    cellMap[cell.xPos, cell.yPos - 1].options.Remove(tilOption);
            }
        }


        // Update LeftCell
        if (cell.xPos > 0 && !cellMap[cell.xPos-1, cell.yPos].collapsed)
        {
            bool valid;
            foreach (Tiles tilOption in new List<Tiles>(cellMap[cell.xPos-1, cell.yPos].options))
            {
                valid = false;
                foreach (Tiles tile in tilOption.RightPosibilities)
                {
                    if (cell.options[0].GetComponent<SpriteRenderer>().sprite == tile.GetComponent<SpriteRenderer>().sprite)
                    {
                        valid = true;
                    }
                }
                if (!valid)
                    cellMap[cell.xPos-1, cell.yPos].options.Remove(tilOption);
            }
        }



        // Update Top cell
        if (cell.yPos < heightMap - 1 && !cellMap[cell.xPos, cell.yPos + 1].collapsed)
        {
            bool valid;
            foreach (Tiles tilOption in new List<Tiles>(cellMap[cell.xPos, cell.yPos + 1].options))
            {
                valid = false;
                foreach (Tiles tile in tilOption.BottomPosibilities)
                {
                    if (cell.options[0].GetComponent<SpriteRenderer>().sprite == tile.GetComponent<SpriteRenderer>().sprite)
                    {
                        valid = true;
                    }
                }
                if (!valid)
                    cellMap[cell.xPos, cell.yPos + 1].options.Remove(tilOption);
            }
        }


        // Update RightCell
        if (cell.xPos < widthMap - 1  && !cellMap[cell.xPos + 1, cell.yPos].collapsed)
        {
            bool valid;
            foreach (Tiles tilOption in new List<Tiles>(cellMap[cell.xPos + 1, cell.yPos].options))
            {
                valid = false;
                foreach (Tiles tile in tilOption.LeftPosibilities)
                {
                    if (cell.options[0].GetComponent<SpriteRenderer>().sprite == tile.GetComponent<SpriteRenderer>().sprite)
                    {
                        valid = true;
                    }
                }
                if (!valid)
                    cellMap[cell.xPos + 1, cell.yPos].options.Remove(tilOption);
            }
        }
    }
    public class Cell
    {
        public bool collapsed;
        public List<Tiles> options; // Tiles this cell can be collapsed into
        public int xPos, yPos;

        public Cell() { collapsed = false; this.options = new List<Tiles>(); }
        public Cell(List<Tiles> tiles, int xPos, int yPos)
        {
            this.collapsed = false;
            this.options = new List<Tiles>(tiles);
            this.xPos = xPos;
            this.yPos = yPos;
        }
    }
}








# region GizmoEditor
#if UNITY_EDITOR
[CustomEditor(typeof(WaveFunctionCollapse))]
public class ScriptEditorWFC : Editor
{
    private WaveFunctionCollapse gizmoDrawing;

    public override void OnInspectorGUI()
    {
        gizmoDrawing = (WaveFunctionCollapse)target;
        gizmoDrawing.widthMap = EditorGUILayout.IntSlider("Width", gizmoDrawing.widthMap, 0, 300);
        gizmoDrawing.heightMap = EditorGUILayout.IntSlider("Height", gizmoDrawing.heightMap, 0, 300);
        gizmoDrawing.tileSize = EditorGUILayout.IntSlider("Tile Size", gizmoDrawing.tileSize, 1, 100);
        EditorGUILayout.FloatField("Execution time (ms)", gizmoDrawing.executionTime);
        //DrawDefaultInspector(); // Draw all public variables
        EditorGUILayout.Space();

        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("usableTiles"), new GUIContent("Sprite"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DefaultTile"), new GUIContent("Sprite Default"));
     
        if (GUILayout.Button("Generate cellular automata"))
        {
            gizmoDrawing.Generate(gizmoDrawing.seed);

        }

        if (GUILayout.Button("Generate random cellular automata"))
        {
            gizmoDrawing.Generate();
        }
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif
#endregion
