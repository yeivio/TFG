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
    private float min_room_width = 49;
    private float min_room_height = 49;

    private RoomNode leftNode;
    private RoomNode rightNode;

    private bool[,] spaceContext;

    private static int nod = 0;
    private int id;
    public RoomNode(bool[,] spaceCtx)
    {
        
        this.spaceContext = spaceCtx;
        id = nod;

        Debug.Log("nodeCounter:" + nod);
        nod++;

        
        //If there isnt enough space for a room, then he is a leaf
        if (spaceContext.GetLength(0) / 2 > min_room_width)
        {
            Split();
        }
        else
        {
            // Debug.Log("[" + id + "]Nazco y Soy hoja:" + spaceContext.GetLength(0) +"," + spaceContext.GetLength(1));

            //Draw room
            for(int i = 0; i < spaceContext.GetLength(0); i++)
            {
                for(int j = 0; j < spaceContext.GetLength(1); j++)
                {
                    if(i != 0 && i != spaceContext.GetLength(0)-1 &&
                        j != 0 && j != spaceContext.GetLength(1)-1)
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
            splitHorizonal = false;

        }
        else if ((spaceContext.GetLength(0) / spaceContext.GetLength(1)) > 1)
        {
            // More height than width
            splitHorizonal = true;
        }
        else
        {
            splitHorizonal = Random.Range(0.0f, 1.0f) > 0.5;    //Random election
        }

        splitHorizonal = true;

        if(splitHorizonal)
        {
            int splitLoc_Height = (int) Random.Range(min_room_height, spaceContext.GetLength(0) - min_room_height);
            //Debug.Log("[" + id + "]Se splitea en:" + (splitLoc_Height+1));

            //Debug.Log("[" + id + "]Matrices divididas:" + (splitLoc_Height+1) + "," + spaceContext.GetLength(1));
            leftNode = new RoomNode(new bool[splitLoc_Height, spaceContext.GetLength(1)]);
            rightNode = new RoomNode(new bool[spaceContext.GetLength(0) - splitLoc_Height, spaceContext.GetLength(1)]);

            Debug.Log("[" + id + "]voy a reconstruir");
            for (int i = 0; i < spaceContext.GetLength(0); i++)
            {
                for(int j = 0; j < spaceContext.GetLength(1); j++)
                {
                    if (i < splitLoc_Height)
                    {
                        this.spaceContext[i, j] = leftNode.spaceContext[i, j];
                    }
                    else
                    {
                        this.spaceContext[i, j] = rightNode.spaceContext[i - splitLoc_Height, j];
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

    public bool[,] getMap() {
        return this.spaceContext; 
    }
}



public class Room
{
    private float height, width;
    private int x, y;
    public float getWidth() { return height; }
    public float getHeight() { return width; }
    public int getX() { return x; }
    public int getY() { return y; }

    public Room(int x, int y, float width, float height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }
}