using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;

public class WFC_Paint : MonoBehaviour
{

    // 10 10 0.8 0 2 2 619078697



    public GenerationAlgorithm baseMap;
    private int widthMap;
    private int heightMap;
    public List<Tiles> usableTiles;
    public Tiles DefaultTile;
    private Cell[,] cellMap;   // Map

    public int seed;

    public void Generate()
    {
        this.widthMap = baseMap.widthMap;
        this.heightMap = baseMap.heightMap;

        //GenerateSeed(-953493837);

        // Clean prev output
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }
        //((CellularAutomata)baseMap).Generate(-1950587812);
        //((CellularAutomata)baseMap).Generate(-434656559); FINISH
        ((CellularAutomata)baseMap).Generate(((CellularAutomata)baseMap).seed);


        CleanMap();
        int randomCell;
        while (CheckEnd())
        {
            List<Cell> sortedTiles = SortedTiles(); // Find the tiles with the lowest entropy
            randomCell = UnityEngine.Random.Range(0, sortedTiles.Count); // Pick a random cell
            Cell actualCell = sortedTiles[randomCell];
            if (actualCell.options.Count == 0) // Doesn't have any tile options
            {
                actualCell.collapsed = true;
                actualCell.options = new List<Tiles>() { DefaultTile };
            }
            else
            {
                //Collapse
                actualCell.collapsed = true;
                Tiles seletedTile = FilterCellOptions(actualCell);
                actualCell.options = new List<Tiles>() { seletedTile };
            }




            UpdateMap(actualCell);
        }

        for (int i = 0; i < heightMap; i++)
            for (int j = 0; j < widthMap; j++)
            {
                SpawnTile(j, i);
            }
    }

    private Tiles FilterCellOptions(Cell actualCell)
    {
        //actualCell.options[UnityEngine.Random.Range(0, actualCell.options.Count)]
        switch (baseMap.map[actualCell.xPos, actualCell.yPos])
        {
            case GenerationAlgorithm.CELL_TYPE.FLOOR:
                actualCell.options.RemoveAll(x => x.cellType != GenerationAlgorithm.CELL_TYPE.FLOOR);
                break;
            case GenerationAlgorithm.CELL_TYPE.WALL:
                actualCell.options.RemoveAll(x => x.cellType != GenerationAlgorithm.CELL_TYPE.WALL);
                break;
        }

        // Check top
        if (actualCell.yPos < heightMap - 1)
        {
           // We get only the tiles which have a wall connection on top
            actualCell.options.RemoveAll(x => !x.TopPosibilities.Any(y => y.cellType == baseMap.map[actualCell.xPos, actualCell.yPos + 1]));
        }

        // Check bottom
        if (actualCell.yPos > 0 )
        {
            actualCell.options.RemoveAll(x => !x.BottomPosibilities.Any(y => y.cellType == baseMap.map[actualCell.xPos, actualCell.yPos - 1]));
        }

        // Check left
        if (actualCell.xPos > 0 )
        {
            actualCell.options.RemoveAll(x => !x.LeftPosibilities.Any(y => y.cellType == baseMap.map[actualCell.xPos - 1, actualCell.yPos]));
        }

        // Check right
        if (actualCell.xPos < widthMap - 1)
        {
            actualCell.options.RemoveAll(x => !x.RightPosibilities.Any(y => y.cellType == baseMap.map[actualCell.xPos + 1, actualCell.yPos]));
        }
        

        if(actualCell.options.Count > 1 ) {
            // Now we select the tile with most % of being correct. That is the 
            // the one which have more valid sides
            List<Tiles> finalTiles = new List<Tiles>();
            float Maxprob = float.MinValue;
            float tileProb;
            foreach (Tiles tile in new List<Tiles>(actualCell.options))
            {
                tileProb = 0;
                if (actualCell.xPos + 1 >= widthMap && tile.RightPosibilities.Any(x => x.cellType != GenerationAlgorithm.CELL_TYPE.WALL)
                    ||
                    actualCell.xPos < widthMap - 1 && tile.RightPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos + 1, actualCell.yPos]))
                {
                    tileProb--;
                }

                if (actualCell.yPos + 1 >= heightMap && tile.TopPosibilities.Any(x => x.cellType != GenerationAlgorithm.CELL_TYPE.WALL)
                    ||
                    actualCell.yPos < heightMap - 1 && tile.TopPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos, actualCell.yPos + 1]))
                {
                    tileProb--;
                }

                if (actualCell.xPos - 1 < 0 && tile.LeftPosibilities.Any(x => x.cellType != GenerationAlgorithm.CELL_TYPE.WALL)
                    ||
                    actualCell.xPos > 0 && tile.LeftPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos - 1, actualCell.yPos]))
                {
                    tileProb--;
                }

                if (actualCell.yPos - 1 < 0 && tile.BottomPosibilities.Any(x => x.cellType != GenerationAlgorithm.CELL_TYPE.WALL)
                    ||
                    actualCell.yPos > 0 && tile.BottomPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos, actualCell.yPos - 1]))
                {
                    tileProb--;
                }
                if (tileProb > Maxprob)
                {
                    finalTiles.Clear();
                    finalTiles.Add(tile);
                    Maxprob = tileProb;
                    
                }
                else if (tileProb == Maxprob)
                {
                    finalTiles.Add(tile);
                }
                else
                {
                    actualCell.options.Remove(tile);
                }
            }

            //if(finalTiles.Count > 1 && finalTiles.Any(x => x.GetComponent<SpriteRenderer>().sprite == DefaultTile.GetComponent<SpriteRenderer>().sprite))
            //{
            //    if((actualCell.xPos > 0  && actualCell.yPos > 0 && baseMap.map[actualCell.xPos - 1, actualCell.yPos - 1] == GenerationAlgorithm.CELL_TYPE.FLOOR)
            //        || (actualCell.xPos > 0 && actualCell.yPos < heightMap - 1 && baseMap.map[actualCell.xPos - 1, actualCell.yPos + 1] == GenerationAlgorithm.CELL_TYPE.FLOOR)
            //         || (actualCell.xPos < widthMap - 1  && actualCell.yPos > 0 && baseMap.map[actualCell.xPos + 1, actualCell.yPos - 1] == GenerationAlgorithm.CELL_TYPE.FLOOR)
            //         || (actualCell.xPos < widthMap - 1 && actualCell.yPos < heightMap - 1 && baseMap.map[actualCell.xPos + 1, actualCell.yPos + 1] == GenerationAlgorithm.CELL_TYPE.FLOOR)
            //        )
            //    {
            //        return DefaultTile;
            //    }
            //    else
            //    {
            //        actualCell.options.Remove(DefaultTile);
            //    }
            //}
        }


        try {
             //return actualCell.options[UnityEngine.Random.Range(0, actualCell.options.Count)];
             return actualCell.options[UnityEngine.Random.Range(0, actualCell.options.Count)];
        }catch(Exception e)
        {
            Debug.LogError($"{actualCell.xPos}, {actualCell.yPos}:{e}");
            return DefaultTile;
        }
    }

    public void SpawnTile(int x, int y)
    {
        GameObject aux = new GameObject().gameObject;
        try
        {
            aux.AddComponent<SpriteRenderer>().sprite = cellMap[x, y].options[0].GetComponent<SpriteRenderer>().sprite;
        }
        catch (Exception ex)
        {
            Debug.Log("Error");
        }
        aux.transform.parent = this.transform;
        aux.gameObject.transform.position = new Vector3(x+0.5f, y+0.5f, 0);
    }

    public bool CheckEnd()
    {
        bool notCollapsed = false;
        for (int i = 0; i < widthMap; i++)
        {
            for (int j = 0; j < heightMap; j++)
            {
                if (!cellMap[i, j].collapsed)
                {
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
                    aux.Add(cellMap[i, j]);
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
        for (int i = 0; i < widthMap; i++)
        {
            for (int j = 0; j < heightMap; j++)
            {
                cellMap[i, j] = new Cell(usableTiles, i, j, baseMap.map[i,j]);
            }
        }
    }
    public void CheckValidity(List<Tiles> optionList, List<Tiles> validOption)
    {
        for (int i = optionList.Count - 1; i >= 0; i--)
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
        if (cell.yPos > 0 && !cellMap[cell.xPos, cell.yPos - 1].collapsed)
        {
            bool valid;
            foreach (Tiles tilOption in new List<Tiles>(cellMap[cell.xPos, cell.yPos - 1].options))
            {
                valid = false;
                foreach (Tiles tile in tilOption.TopPosibilities)
                {
                    if (cell.options[0].GetComponent<SpriteRenderer>().sprite == tile.GetComponent<SpriteRenderer>().sprite)
                    {
                        valid = true;
                    }
                }
                if (!valid)
                    cellMap[cell.xPos, cell.yPos - 1].options.Remove(tilOption);
            }
        }


        // Update LeftCell
        if (cell.xPos > 0 && !cellMap[cell.xPos - 1, cell.yPos].collapsed)
        {
            bool valid;
            foreach (Tiles tilOption in new List<Tiles>(cellMap[cell.xPos - 1, cell.yPos].options))
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
                    cellMap[cell.xPos - 1, cell.yPos].options.Remove(tilOption);
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
        if (cell.xPos < widthMap - 1 && !cellMap[cell.xPos + 1, cell.yPos].collapsed)
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
        public Cell(List<Tiles> tiles, int xPos, int yPos, GenerationAlgorithm.CELL_TYPE type)
        {
            this.collapsed = false;
            this.options = new List<Tiles>(tiles);
            this.options.RemoveAll(x => x.cellType != type);
            this.xPos = xPos;
            this.yPos = yPos;
        }
    }
    protected void GenerateSeed(int seed = -1)
    {
        int tempSeed = (int)DateTime.Now.Ticks;
        if (seed == -1) // No seed 
        {
            Debug.Log("No hay seed");
            UnityEngine.Random.InitState(tempSeed);
            this.seed = tempSeed;
        }
        else
            UnityEngine.Random.InitState(seed);
    }



    public void TileVerifier()
    {
        foreach(Tiles tile in usableTiles)
        {
            foreach(Tiles topTile in tile.TopPosibilities)
            {
                if (!topTile.BottomPosibilities.Any(x => x == tile))
                    Debug.LogError($"{tile} top -> {topTile} pero no el inverso");
            }

            foreach (Tiles topTile in tile.BottomPosibilities)
            {
                if (!topTile.TopPosibilities.Any(x => x == tile))
                    Debug.LogError($"{tile} bottom -> {topTile} pero no el inverso");
            }

            foreach (Tiles topTile in tile.LeftPosibilities)
            {
                if (!topTile.RightPosibilities.Any(x => x == tile))
                    Debug.LogError($"{tile} left -> {topTile} pero no el inverso");
            }
            foreach (Tiles topTile in tile.RightPosibilities)
            {
                if (!topTile.LeftPosibilities.Any(x => x == tile))
                    Debug.LogError($"{tile} right -> {topTile} pero no el inverso");
            }
        }
        
    }
}

# region GizmoEditor
#if UNITY_EDITOR
[CustomEditor(typeof(WFC_Paint))]
public class ScriptEditorWFC_Paint : Editor
{
    private WFC_Paint gizmoDrawing;

    public override void OnInspectorGUI()
    {
        gizmoDrawing = (WFC_Paint)target;
        //DrawDefaultInspector(); // Draw all public variables
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseMap"), new GUIContent("BaseMap"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("usableTiles"), new GUIContent("Sprite"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DefaultTile"), new GUIContent("Sprite Default"));

        if (GUILayout.Button("Paint"))
        {
            gizmoDrawing.Generate();

        }
        if (GUILayout.Button("Verify tile rules"))
        {
            gizmoDrawing.TileVerifier();

        }
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif
#endregion

