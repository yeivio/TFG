using System.Collections;
using UnityEngine;

public class Dungeon
{
    private BitArray array;
    private int originalHeight;
    private int originalWidth;

    public Dungeon(Vector2Int startPos, Vector2Int endPos)
    {

        originalHeight = (endPos.y - startPos.y + 1);
        originalWidth = (endPos.x - startPos.x + 1);
        array = new BitArray(originalWidth * originalHeight);
        
    }

    public void SetValor(int x, int y, bool value)
    {
        array[y * GetColumnNum() + x] = value;
    }

    public void SetValor(int index, bool value)
    {
        array[index] = value;
    }
    public bool getValor(int x, int y)
    {
        return array[y * GetColumnNum() + x];
    }

    public bool getValor(int index)
    {
        return array[index];
    }

    public int GetColumnNum()
    {
        return this.originalWidth;
    }
    public int GetRowNum()
    {
        return this.originalHeight;
    }

    public bool[,] ToBoolMatrix()
    {
        bool[,] aux = new bool[GetRowNum(), GetColumnNum()];

        for (int i = 0; i < GetRowNum(); i++)
        {
            for (int j = 0; j < GetColumnNum(); j++)
            {
                aux[i, j] = array[i * GetColumnNum() + j];
            }
        }
        return aux;
    }
}
