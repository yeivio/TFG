using System;
using UnityEngine;

public class CellularAutomataEmptyFilling : CellularAutomata
{
    private int MIN_RADIO_EMPTY = 4; // Radius the algorithm will check for empty spaces

    public bool[,] Generate(int width, int height, float wallStart, int numberSteps, int conversionWall, int conversionBlank, int seed = -1, int minRadio = 2)
    {
        this.MIN_RADIO_EMPTY = minRadio;
         return base.Generate(width, height, wallStart, numberSteps, conversionWall, conversionBlank, seed);
    }

    protected override bool[,] simulationStep()
    {
        bool[,] copyMap = new bool[this.widthMap, this.heightMap];
        for (int x = 0; x < this.widthMap; x++)
        {
            for (int y = 0; y < this.heightMap; y++)
            {
                int numWalls = this.CountNearWalls(x, y, 2);
                int nearByNumWalls = this.CountNearWalls(x, y, this.MIN_RADIO_EMPTY);
                if (this.map[x, y])
                {
                    if (numWalls < MIN_CONVERSION_WALL)
                        copyMap[x, y] = false;
                    else
                        copyMap[x, y] = true;
                }
                else
                {
                    if (numWalls > MIN_CONVERSION_BLANK || nearByNumWalls == 0)
                        copyMap[x, y] = true;
                    else
                        copyMap[x, y] = false;
                }
            }
        }
        this.map = copyMap;
        return copyMap;
    }


}
