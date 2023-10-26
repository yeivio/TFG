using System;
using UnityEngine;


public class GizmoDrawing : MonoBehaviour
{
    [Range(1, 100)]
    public int width;
    [Range(1, 100)]
    public int height;

    [Range(1, 100)]
    public int tileSize;

    private int[,] positions;

    private void Start()
    {
        positions = new int[width, height];
        generateRandomPattern(positions);
    }
    private void OnDrawGizmosSelected()
    {
        if(positions != null)
            for (int i = 0; i < width; i++)
                for(int j = 0; j < height; j++)
                {
                    if (positions[i,j] == 0)
                        Gizmos.color = Color.black;
                    else 
                        Gizmos.color = Color.white;
                    Gizmos.DrawCube(new Vector3(tileSize * i, tileSize * j, 0), new Vector3(tileSize, tileSize, 1));
                }

    }
    private void generateRandomPattern(int[,] matrix)
    {
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                    positions[i, j] = UnityEngine.Random.Range(0, 2);
    }
}
