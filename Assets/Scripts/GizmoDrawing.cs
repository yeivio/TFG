using System;
using UnityEngine;


public class GizmoDrawing : MonoBehaviour
{
    [Range(1, 100)]
    public int width;   // Max width of the canvas
    [Range(1, 100)]
    public int height;  // Max height of the canvas

    [Range(1, 100)]
    public int tileSize;    // Size of each tile on the canvas

    public bool[,] mapValue;   // Seed

    private void Start()
    {
        mapValue = new bool[width,height];
        generateRandomPattern();
    }
    private void OnDrawGizmosSelected()
    {
        if (mapValue != null)
            for (int i = 0; i < width; i++)
                for(int j = 0; j < height; j++)
                {
                    if (mapValue[i,j])
                        Gizmos.color = Color.black;
                    else 
                        Gizmos.color = Color.white;
                    Gizmos.DrawCube(new Vector3(tileSize * i, tileSize * j, 0), new Vector3(tileSize, tileSize, 1));
                }

    }
    public void generateRandomPattern()
    {
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                    this.mapValue[i, j] = UnityEngine.Random.Range(0, 2) == 1;
    }
}
