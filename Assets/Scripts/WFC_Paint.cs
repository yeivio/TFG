using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;
using static GenerationAlgorithm;
using Unity.VisualScripting;

public class WFC_Paint : MonoBehaviour
{

    // wrong 10 10 0.893 0 2 2 728405002 5 0
    public GenerationAlgorithm baseMap;
    private int widthMap;
    private int heightMap;
    public List<Tiles> usableTiles;
    public Tiles DefaultTile;
    public Tiles ErrorTile;
    private Cell[,] cellMap;   // Map
    private SpriteRenderer[,] tileMaps; // Visual representation of cellMap

    public int seed;

    private int limitBuc;

    public void Generate()
    {
        limitBuc = 200000;
        this.widthMap = baseMap.widthMap;
        this.heightMap = baseMap.heightMap;
        tileMaps = new SpriteRenderer[widthMap, heightMap];
        //GenerateSeed(-953493837);

        // Clean prev output
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if (Application.isPlaying)
                Destroy(this.transform.GetChild(0).gameObject);
            else
                DestroyImmediate(this.transform.GetChild(0).gameObject, false);
        }

        this.seed = baseMap.seed;
        baseMap.Generate(seed);

        CleanMap();
        while (CheckEnd() && limitBuc > 0)
        {
            //List<Cell> sortedTiles = SortedTiles(); // Find the tiles with the lowest entropy
            List<Cell> sortedTiles = SortedTiles2(); // Find the tiles with the lowest entropy
            Cell actualCell = sortedTiles[0];

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
            limitBuc--;
        }

        if(limitBuc <= 0)
        {
            Debug.LogError("LIMITBREAK");
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
           // Removing any tile option which doesn't match the celltype on the adyacent position.
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


        //Diagonals

        // Check right-top
        if (actualCell.xPos < widthMap - 1 && actualCell.yPos < heightMap - 1)
        {
            actualCell.options.RemoveAll(x => !x.TopRightPosibilities.Any(y => y.cellType == baseMap.map[actualCell.xPos + 1, actualCell.yPos + 1]));
        }
        
        // Check left-top
        if (actualCell.xPos > 0 && actualCell.yPos < heightMap - 1)
        {
            actualCell.options.RemoveAll(x => !x.TopLeftDiagonalPosibilities.Any(y => y.cellType == baseMap.map[actualCell.xPos - 1 , actualCell.yPos + 1]));
        }

        // Check right-bottom
        if (actualCell.xPos < widthMap - 1 && actualCell.yPos > 0)
        {
            actualCell.options.RemoveAll(x => !x.BottomRightPosibilities.Any(y => y.cellType == baseMap.map[actualCell.xPos + 1, actualCell.yPos - 1]));
        }
        
        // Check left-bottom
        if (actualCell.xPos > 0 && actualCell.yPos > 0)
        {
            actualCell.options.RemoveAll(x => !x.BottomLeftPosibilities.Any(y => y.cellType == baseMap.map[actualCell.xPos - 1 , actualCell.yPos - 1]));
        }


        if (actualCell.options.Count > 1)
        {
            // Now we select the tile with most % of being correct. That is the 
            // the one which have more valid sides
            List<Tiles> finalTiles = new List<Tiles>();
            float Maxprob = float.MinValue;
            float tileProb;

            foreach (Tiles tile in new List<Tiles>(actualCell.options))
            {
                tileProb = 0;

                //Right
                if (actualCell.xPos + 1 >= widthMap && tile.RightPosibilities.Any(x => x.cellType == CELL_TYPE.FLOOR)
                    ||
                    actualCell.xPos < widthMap - 1 && tile.RightPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos + 1, actualCell.yPos]))
                {
                    tileProb--;
                }

                // top
                if (actualCell.yPos + 1 >= heightMap && tile.TopPosibilities.Any(x => x.cellType == CELL_TYPE.FLOOR)
                    ||
                    actualCell.yPos < heightMap - 1 && tile.TopPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos, actualCell.yPos + 1]))
                {
                    tileProb--;
                }
                // Left
                if (actualCell.xPos - 1 < 0 && tile.LeftPosibilities.Any(x => x.cellType == CELL_TYPE.FLOOR)
                    ||
                    actualCell.xPos > 0 && tile.LeftPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos - 1, actualCell.yPos]))
                {
                    tileProb--;
                }

                //Bottom
                if (actualCell.yPos - 1 < 0 && tile.BottomPosibilities.Any(x => x.cellType == CELL_TYPE.FLOOR)
                    ||
                    actualCell.yPos > 0 && tile.BottomPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos, actualCell.yPos - 1]))
                {
                    tileProb--;
                }

                //Bottom-Right
                if((actualCell.yPos - 1 < 0 || actualCell.xPos + 1 >= widthMap) && tile.BottomRightPosibilities.Any(x => x.cellType == CELL_TYPE.FLOOR) 
                    ||
                    actualCell.yPos > 0 && actualCell.xPos < widthMap - 1 && tile.BottomRightPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos + 1, actualCell.yPos - 1]))
                {
                    tileProb--;
                }

                //Bottom-Left
                if ((actualCell.yPos - 1 < 0 || actualCell.xPos - 1 < 0) && tile.BottomLeftPosibilities.Any(x => x.cellType == CELL_TYPE.FLOOR)
                    ||
                    actualCell.yPos > 0 && actualCell.xPos > 0 && tile.BottomLeftPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos - 1, actualCell.yPos - 1]))
                {
                    tileProb--;
                }

                //Top-Right
                if ((actualCell.yPos + 1 >= heightMap || actualCell.xPos + 1 >= widthMap) && tile.TopRightPosibilities.Any(x => x.cellType == CELL_TYPE.FLOOR)
                    ||
                    actualCell.yPos < heightMap - 1 && actualCell.xPos < widthMap - 1 && tile.TopRightPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos + 1, actualCell.yPos + 1]))
                {
                    tileProb--;
                }

                //Top-Left
                if ((actualCell.yPos + 1 >= heightMap || actualCell.xPos - 1 < 0) && tile.TopLeftDiagonalPosibilities.Any(x => x.cellType == CELL_TYPE.FLOOR)
                    ||
                     actualCell.yPos < heightMap - 1 && actualCell.xPos > 0 && tile.TopLeftDiagonalPosibilities.Any(x => x.cellType != baseMap.map[actualCell.xPos - 1, actualCell.yPos + 1]))
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

            if (finalTiles.Count > 1 && finalTiles.Any(x => x.GetComponent<SpriteRenderer>().sprite == DefaultTile.GetComponent<SpriteRenderer>().sprite)
                && CountNearWalls(actualCell.xPos, actualCell.yPos) == 8)
            {
                Debug.Log("Se pone Default");
                return DefaultTile;
            }
            return finalTiles[UnityEngine.Random.Range(0, finalTiles.Count)];
        }


        try
        {
             //return actualCell.options[UnityEngine.Random.Range(0, actualCell.options.Count)];
             return actualCell.options[UnityEngine.Random.Range(0, actualCell.options.Count)];
        }catch(Exception e)
        {
            Debug.LogError($"{actualCell.xPos}, {actualCell.yPos},{actualCell.options.Count}:{e}");
            return ErrorTile;
        }
    }
    public void SpawnTile(int x, int y)
    {
        GameObject aux = new GameObject().gameObject;
        try
        {
            aux.AddComponent<SpriteRenderer>().sprite = cellMap[x, y].options[0].GetComponent<SpriteRenderer>().sprite;
            tileMaps[x, y] = aux.GetComponent<SpriteRenderer>();
        }
        catch (Exception)
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
    public List<Cell> SortedTiles2()
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
        //First sort by number of walls
        aux.Sort((a, b) => { return CountNearWalls(a.xPos, a.yPos) - CountNearWalls(b.xPos, b.yPos); });
        int aux2 = CountNearWalls(aux[0].xPos, aux[0].yPos);
        aux.RemoveAll(x => CountNearWalls(x.xPos, x.yPos) != aux2);
        
        // Then sort by number of options
        aux.Sort((a, b) => { return a.options.Count - b.options.Count; });
        aux.RemoveAll(x => x.options.Count != aux[0].options.Count);

        return aux;
    }
    protected int CountNearWalls(int x, int y)
    {
        int radius = 1;
        int contador = 0;

        for (int i = -1; i <= radius; i++)
            for (int j = -1; j <= radius; j++)
            {
                int casillaX = x + i;
                int casillaY = y + j;

                if (i == 0 && j == 0)
                { /* We are on the central position */
                }

                else if (casillaX < 0 || casillaX >= this.widthMap
                        || casillaY < 0 || casillaY >= this.heightMap)
                {
                    contador++; // Out of bounds count as walls
                }
                else if (baseMap.map[casillaX, casillaY] == CELL_TYPE.WALL)
                {
                    contador++;
                }
            }
        return contador;
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

        // Update Top Right Cell
        if (cell.yPos < heightMap - 1 && cell.xPos < widthMap - 1 && !cellMap[cell.xPos + 1, cell.yPos + 1].collapsed)
        {
            bool valid;
            foreach (Tiles tilOption in new List<Tiles>(cellMap[cell.xPos + 1, cell.yPos + 1].options))
            {
                valid = false;
                foreach (Tiles tile in tilOption.BottomLeftPosibilities)
                {
                    if (cell.options[0].GetComponent<SpriteRenderer>().sprite == tile.GetComponent<SpriteRenderer>().sprite)
                    {
                        valid = true;
                    }
                }
                if (!valid)
                    cellMap[cell.xPos + 1, cell.yPos + 1].options.Remove(tilOption);
            }
        }


        // Update Bottom Right Cell
        if (cell.yPos > 0 && cell.xPos < widthMap - 1 && !cellMap[cell.xPos + 1, cell.yPos - 1].collapsed)
        {
            bool valid;
            foreach (Tiles tilOption in new List<Tiles>(cellMap[cell.xPos + 1, cell.yPos - 1].options))
            {
                valid = false;
                foreach (Tiles tile in tilOption.TopLeftDiagonalPosibilities)
                {
                    if (cell.options[0].GetComponent<SpriteRenderer>().sprite == tile.GetComponent<SpriteRenderer>().sprite)
                    {
                        valid = true;
                    }
                }
                if (!valid)
                    cellMap[cell.xPos + 1, cell.yPos - 1].options.Remove(tilOption);
            }
        }


        // Update Top Left Cell
        if (cell.yPos < heightMap - 1 && cell.xPos > 0 && !cellMap[cell.xPos - 1, cell.yPos + 1].collapsed)
        {
            bool valid;
            foreach (Tiles tilOption in new List<Tiles>(cellMap[cell.xPos - 1, cell.yPos + 1].options))
            {
                valid = false;
                foreach (Tiles tile in tilOption.BottomRightPosibilities)
                {
                    if (cell.options[0].GetComponent<SpriteRenderer>().sprite == tile.GetComponent<SpriteRenderer>().sprite)
                    {
                        valid = true;
                    }
                }
                if (!valid)
                    cellMap[cell.xPos - 1, cell.yPos + 1].options.Remove(tilOption);
            }
        }

        // Update Bottom Left Cell
        if (cell.yPos > 0 && cell.xPos > 0 && !cellMap[cell.xPos - 1, cell.yPos - 1].collapsed)
        {
            bool valid;
            foreach (Tiles tilOption in new List<Tiles>(cellMap[cell.xPos - 1, cell.yPos - 1].options))
            {
                valid = false;
                foreach (Tiles tile in tilOption.TopRightPosibilities)
                {
                    if (cell.options[0].GetComponent<SpriteRenderer>().sprite == tile.GetComponent<SpriteRenderer>().sprite)
                    {
                        valid = true;
                    }
                }
                if (!valid)
                    cellMap[cell.xPos - 1, cell.yPos - 1].options.Remove(tilOption);
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
                {
                    if(!topTile.BottomPosibilities.Contains(tile))
                        topTile.BottomPosibilities.Add(tile);
                    Debug.LogError($"{tile} top -> {topTile} pero no el inverso");
                }
            }

            foreach (Tiles topTile in tile.BottomPosibilities)
            {
                if (!topTile.TopPosibilities.Any(x => x == tile))
                {
                    if (!topTile.TopPosibilities.Contains(tile))
                        topTile.TopPosibilities.Add(tile);
                    Debug.LogError($"{tile} bottom -> {topTile} pero no el inverso");
                }
                    
            }

            foreach (Tiles topTile in tile.LeftPosibilities)
            {
                if (!topTile.RightPosibilities.Any(x => x == tile))
                {
                    if (!topTile.RightPosibilities.Contains(tile))
                        topTile.RightPosibilities.Add(tile);
                    Debug.LogError($"{tile} left -> {topTile} pero no el inverso");
                }
                    

                    
            }
            foreach (Tiles topTile in tile.RightPosibilities)
            {
                if (!topTile.LeftPosibilities.Any(x => x == tile))
                {
                    if (!topTile.LeftPosibilities.Contains(tile))
                        topTile.LeftPosibilities.Add(tile);
                    Debug.LogError($"{tile} right -> {topTile} pero no el inverso");
                }
                    
            }

            foreach (Tiles topTile in tile.TopLeftDiagonalPosibilities)
            {
                if (!topTile.BottomRightPosibilities.Any(x => x == tile))
                {
                    if (!topTile.BottomRightPosibilities.Contains(tile))
                        topTile.BottomRightPosibilities.Add(tile);
                    Debug.LogError($"{tile} right -> {topTile} pero no el inverso");
                }
                    
            }

            foreach (Tiles topTile in tile.BottomRightPosibilities)
            {
                if (!topTile.TopLeftDiagonalPosibilities.Any(x => x == tile))
                {
                    if (!topTile.TopLeftDiagonalPosibilities.Contains(tile))
                        topTile.TopLeftDiagonalPosibilities.Add(tile);
                    Debug.LogError($"{tile} right -> {topTile} pero no el inverso");
                }
                   
            }

            foreach (Tiles topTile in tile.BottomLeftPosibilities)
            {
                if (!topTile.TopRightPosibilities.Any(x => x == tile))
                {
                    if (!topTile.TopRightPosibilities.Contains(tile))
                        topTile.TopRightPosibilities.Add(tile);
                    Debug.LogError($"{tile} right -> {topTile} pero no el inverso");
                }
                    
            }

            foreach (Tiles topTile in tile.TopRightPosibilities)
            {
                if (!topTile.BottomLeftPosibilities.Any(x => x == tile))
                {
                    if (!topTile.BottomLeftPosibilities.Contains(tile))
                        topTile.BottomLeftPosibilities.Add(tile);
                    Debug.LogError($"{tile} right -> {topTile} pero no el inverso");
                }
                    
            }
            
        }
        
    }
    public void TileLearner()
    {
        
        for (int x = 0; x < widthMap; x++)
        {
            for(int y = 0; y < heightMap; y++)
            {
                Tiles learningTile = usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y].sprite);
                // Check top
                if (y < heightMap - 1 && !learningTile.TopPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y + 1].sprite)))
                {
                    learningTile.TopPosibilities.Add(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y + 1].sprite));
                    Debug.LogWarning($"TL:{learningTile} top -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y + 1].sprite)}");
                }

                // Check bottom
                if (y > 0 && !learningTile.BottomPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y - 1].sprite)))
                {
                    learningTile.BottomPosibilities.Add(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y - 1].sprite));
                    Debug.LogWarning($"TL:{learningTile} bottom -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y - 1].sprite)}");
                }

                // Check left
                if (x > 0 && !learningTile.LeftPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y].sprite)))
                {
                    learningTile.LeftPosibilities.Add(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y ].sprite));
                    Debug.LogWarning($"TL:{learningTile} Left -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y].sprite)}");
                }

                // Check right
                if (x < widthMap - 1 && !learningTile.RightPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y].sprite)))
                {
                    learningTile.RightPosibilities.Add(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y].sprite));
                    Debug.LogWarning($"TL:{learningTile} Right -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y].sprite)}");
                }


                //Diagonals
                // Check right-top
                if (x < widthMap - 1 && y < heightMap - 1 && !learningTile.TopRightPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y + 1].sprite)))
                {
                    learningTile.TopRightPosibilities.Add(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y + 1].sprite));
                    Debug.LogWarning($"TL:{learningTile} Right-Top -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y + 1].sprite)}");
                }

                // Check left-top
                if (x > 0 && y < heightMap - 1 && !learningTile.TopLeftDiagonalPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y + 1].sprite)))
                {
                    learningTile.TopLeftDiagonalPosibilities.Add(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y + 1].sprite));
                    Debug.LogWarning($"TL:{learningTile} Right-Left -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y + 1].sprite)}");

                }

                // Check right-bottom
                if (x < widthMap - 1 && y > 0 && !learningTile.BottomRightPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y - 1].sprite)))
                {
                    learningTile.BottomRightPosibilities.Add(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y - 1].sprite));
                    Debug.LogWarning($"TL:{learningTile} Bottom-Right -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y - 1].sprite)}");

                }

                // Check left-bottom
                if (x > 0 && y > 0 && !learningTile.BottomLeftPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y - 1].sprite)))
                {
                    learningTile.BottomLeftPosibilities.Add(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y - 1].sprite));
                    Debug.LogWarning($"TL:{learningTile} Bottom-Left -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y - 1].sprite)}");

                }
            }
        }
        Debug.Log("Tiles Learned");
    }



    public void TileFinder()
    {

        for (int x = 0; x < widthMap; x++)
        {
            for (int y = 0; y < heightMap; y++)
            {
                Tiles learningTile = usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y].sprite);
                // Check top
                if (y < heightMap - 1 && !learningTile.TopPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y + 1].sprite)))
                {
                    Debug.LogWarning($"TL:{learningTile} top -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y + 1].sprite)} || {x}, {y}");
                }

                // Check bottom
                if (y > 0 && !learningTile.BottomPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y - 1].sprite)))
                {
                    Debug.LogWarning($"TL:{learningTile} bottom -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x, y - 1].sprite)} || {x}, {y}");
                }

                // Check left
                if (x > 0 && !learningTile.LeftPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y].sprite)))
                {
                    Debug.LogWarning($"TL:{learningTile} Left -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y].sprite)} || {x}, {y}");
                }

                // Check right
                if (x < widthMap - 1 && !learningTile.RightPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y].sprite)))
                {
                    Debug.LogWarning($"TL:{learningTile} Right -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y].sprite)} || {x}, {y}");
                }


                //Diagonals
                // Check right-top
                if (x < widthMap - 1 && y < heightMap - 1 && !learningTile.TopRightPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y + 1].sprite)))
                {
                    Debug.LogWarning($"TL:{learningTile} Right-Top -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y + 1].sprite)} || {x}, {y}");
                }

                // Check left-top
                if (x > 0 && y < heightMap - 1 && !learningTile.TopLeftDiagonalPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y + 1].sprite)))
                {
                    Debug.LogWarning($"TL:{learningTile} Right-Left -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y + 1].sprite)} || {x}, {y}");

                }

                // Check right-bottom
                if (x < widthMap - 1 && y > 0 && !learningTile.BottomRightPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y - 1].sprite)))
                {
                    Debug.LogWarning($"TL:{learningTile} Bottom-Right -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x + 1, y - 1].sprite)} || {x}, {y}");

                }

                // Check left-bottom
                if (x > 0 && y > 0 && !learningTile.BottomLeftPosibilities.Contains(usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y - 1].sprite)))
                {
                    Debug.LogWarning($"TL:{learningTile} Bottom-Left -> {usableTiles.Find(til => til.GetComponent<SpriteRenderer>().sprite == tileMaps[x - 1, y - 1].sprite)} || {x}, {y}");

                }
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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ErrorTile"), new GUIContent("Sprite Error"));
        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);

        if (GUILayout.Button("Paint"))
        {
            gizmoDrawing.Generate();

        }
        if (GUILayout.Button("Verify tile rules"))
        {
            gizmoDrawing.TileVerifier();
            foreach (Tiles til in gizmoDrawing.usableTiles)
            {
                EditorUtility.SetDirty(til);
                PrefabUtility.RecordPrefabInstancePropertyModifications(til);
            }

        }

        if (GUILayout.Button("FIND NEW RULES"))
        {
            gizmoDrawing.TileFinder();
        }

        if (GUILayout.Button("Learn"))
        {
            gizmoDrawing.TileLearner();
            foreach (Tiles til in gizmoDrawing.usableTiles)
            {
                EditorUtility.SetDirty(til);
                PrefabUtility.RecordPrefabInstancePropertyModifications(til);
            }

        }
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif
#endregion

