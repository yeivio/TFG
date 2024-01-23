using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class BSPTree : GenerationAlgorithm
{
    


    public bool[,] Generate(int width, int height, int seed = -1)
    {
        this.widthMap = width;
        this.heightMap = height;
        map = new bool[width, height];
        this.seed = seed.ToString();
        GenerateSeed(seed);
        return new RoomNode(map).getMap();
    }



}


public class RoomNode
{
    private float min_room_width= 8;
    private float max_room_width = 20;
    private float min_room_height = 8;
    private float max_room_height= 20;

    private RoomNode leftNode;
    private RoomNode rightNode;

    private bool[,] spaceContext;

    private bool isLeaf;

    public RoomNode(bool[,] spaceCtx)
    {
        this.spaceContext = spaceCtx;

        //If there isnt enough space for a room, then he is a leaf
        if(spaceContext.GetLength(0) / 2 < min_room_width ||
                spaceContext.GetLength(1) / 2 < min_room_height)
        {
            Split();

        }
        else
        {
            isLeaf = true;

            //Draw room
            for(int i = 0; i < spaceContext.GetLength(0); i++)
            {
                for(int j = 0; j < spaceContext.GetLength(1); j++)
                {
                    this.spaceContext[i, j] = true;
                }
            }
        }
    }


    public void Split()
    {

        // Calculate if we are getting an horizontal or vertical split 
        bool splitHorizonal = false;
        if ((spaceContext.GetLength(0) / spaceContext.GetLength(1)) < 1)
        {
            // More width than height
            //splitHorizonal = false;

        }
        else if ((spaceContext.GetLength(0) / spaceContext.GetLength(1)) > 1)
        {
            // More height than width
            splitHorizonal = true;
        }
        else
        {
            //splitHorizonal = Random.Range(0.0f, 1.0f) > 0.5;    //Random election
        }

        if(splitHorizonal)
        {
            int splitLoc_Height = (int) Random.Range(min_room_height, spaceContext.GetLength(1) - min_room_height);
            
            leftNode = new RoomNode(new bool[0,splitLoc_Height]);
            rightNode = new RoomNode(new bool[splitLoc_Height+1, spaceContext.GetLength(1) - splitLoc_Height]);

            for (int i = 0; i < spaceContext.GetLength(1); i++)
            {
                for(int j = 0; j < spaceContext.GetLength(0); j++)
                {
                    if (i <= splitLoc_Height)
                    {
                        this.spaceContext[i,j] = leftNode.spaceContext[j, i];
                    }
                    else
                    {
                        this.spaceContext[i, j] = leftNode.spaceContext[j, i];
                    }
                }
            }

        }
        else
        {
            int splitLoc_Width = (int) Random.Range(min_room_width, spaceContext.GetLength(0) - min_room_width);
            leftNode = new RoomNode(new bool[0, splitLoc_Width]);
            rightNode = new RoomNode(new bool[splitLoc_Width, spaceContext.GetLength(1) - splitLoc_Width]);
        }

        
        
    }
    public bool IsLeaf()
    {
        return this.isLeaf;
    }
    public bool[,] getMap() { return this.spaceContext; }
}



public class Room
{
    private float height, width;
    public float getWidth() { return height; }
    public float getHeight() { return width; }

    public Room(float height, float width)
    {
        this.height = height;
        this.width = width;
    }
}