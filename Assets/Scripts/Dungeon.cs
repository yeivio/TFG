using System.Collections;
using UnityEngine;

public class Dungeon
{
    private BitArray array;
    private int originalHeight;
    private int originalWidth;


    public Dungeon(bool[,] spaceContext)
    {
        bool[] aux = new bool[spaceContext.GetLength(0) * spaceContext.GetLength(1)];
        int index = 0;
        this.originalHeight = spaceContext.GetLength(0);
        this.originalWidth = spaceContext.GetLength(1);

        for (int i = 0; i < spaceContext.GetLength(0); i++)
        {
            for (int j = 0; j < spaceContext.GetLength(1); j++)
            {
                aux[index] = spaceContext[i, j];
                index++;
            }
        }

        array = new BitArray(aux);
    }

    public void SetValor(int x, int y, bool value)
    {
        array[x * GetColumnNum() + y] = value;
    }

    public void SetValor(int index, bool value)
    {
        array[index] = value;
    }
    public bool getValor(int x, int y)
    {
        return array[x * GetColumnNum() + y];
    }

    public bool getValor(int index)
    {
        return array[index];
    }


    public void FindTrue()
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i])
                Debug.Log("Encontrado" + i);
        }
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
