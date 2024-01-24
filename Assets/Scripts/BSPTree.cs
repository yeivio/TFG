using UnityEngine;


public class BSPTree : GenerationAlgorithm
{

    public bool[,] Generate(int numColumns, int numRows, int min_room_width, int min_room_height, int max_room_width, int max_room_height, int seed = -1)
    {
        this.widthMap = numColumns;
        this.heightMap = numRows;
        map = new bool[heightMap, widthMap];
        this.seed = seed.ToString();
        GenerateSeed(seed);
        
        return new RoomNode(map, min_room_width, min_room_height, max_room_width, max_room_height).getMap().ToBoolMatrix(); 
    }
}


public class RoomNode
{
    private int min_room_width;
    private int max_room_width;
    private int min_room_height;
    private int max_room_height;

    private RoomNode leftNode;
    private RoomNode rightNode;

    private Dungeon spaceContext;

    private static int nod = 0;
    private int id;
    public RoomNode(bool[,] spaceCtx, int min_room_width, int min_room_height, int max_room_width, int max_room_height)
    {
        this.min_room_height = min_room_height;
        this.max_room_width = max_room_width;
        this.min_room_width = min_room_width;
        this.max_room_height = max_room_height;
        //Debug.Log("Me llega spaceCtx de dimensiones:"+ spaceCtx.GetLength(0) + "X" + spaceCtx.GetLength(1));
        
        this.spaceContext = new Dungeon(spaceCtx);
        id = nod;
        nod++;
        //Debug.Log("Soy el nodo:" + id + "Y mis dimensiones son:" + spaceContext.GetRowNum() +"X" +spaceContext.GetColumnNum());
        //Debug.Log(((float)spaceContext.GetHeight() / 2));
        //If there isnt enough space for a room, then he is a leaf

        if (((float)spaceContext.GetRowNum() / 2) > max_room_height||
            ((float)spaceContext.GetColumnNum() / 2) > max_room_width)
        {
            this.Split();
        }
        else
        {
            //Debug.Log("[" + id + "]Soy una:" + spaceContext.GetColumnSize() + "X" + spaceContext.GetRowSize());

            int houseWidth = (int)Random.Range(this.min_room_width, Mathf.Min(this.max_room_width, spaceContext.GetColumnNum()));
            int houseHeight = (int)Random.Range(this.min_room_height, Mathf.Min(this.max_room_height, spaceContext.GetRowNum()));

            int startWidth = (int) Random.Range(0, spaceContext.GetColumnNum() - houseWidth);
            int startheight = (int) Random.Range(0, spaceContext.GetRowNum() - houseHeight);
            //Debug.Log(" [" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
            //Debug.Log("Se construye una casa de: " + houseHeight + "x" + houseHeight +" [" + startheight + ","+ startWidth + "]");

            //Draw room when you can't divide
            for (int i = startheight; i < startheight + houseHeight; i++)
            {
                for(int j = startWidth; j < startWidth + houseWidth; j++)
                {
                    this.spaceContext.SetValor((i*spaceContext.GetColumnNum())+j, true);
                }
            }
        }
        
    }


    public void Split()
    {

        // Calculate if we are getting an horizontal or vertical split 
        bool splitHorizonal;
        if(((float)spaceContext.GetRowNum() / 2) > min_room_height &&
            ((float)spaceContext.GetColumnNum() / 2) > min_room_width)
        {
            splitHorizonal = Random.Range(0.0f, 1.0f) > 0.5;    //Random election
        }
        else 
        { 
            splitHorizonal = ((float)spaceContext.GetRowNum() / 2) > min_room_height;
        }
        
        //Debug.Log("Horizontal?" + splitHorizonal);

        if(splitHorizonal)
        {
            int splitLoc_Height = (int) Random.Range(min_room_height, spaceContext.GetRowNum() - min_room_height);
            //Debug.Log("Corte en:" + splitLoc_Height);
            //Debug.Log("Se va a crear izq:" + splitLoc_Height + "X" + spaceContext.GetColumnNum());
            //Debug.Log("Se va a crear der:" + (spaceContext.GetRowNum() - splitLoc_Height - 1) + "X" + spaceContext.GetColumnNum());
            leftNode = new RoomNode(new bool[splitLoc_Height, spaceContext.GetColumnNum()], min_room_width,min_room_height, max_room_width, max_room_height);
            rightNode = new RoomNode(new bool[spaceContext.GetRowNum() - splitLoc_Height - 1, spaceContext.GetColumnNum()], min_room_width, min_room_height, max_room_width, max_room_height);

            //Debug.Log("[" + id + "]voy a reconstruir" + spaceContext.GetRowNum() + "x" + spaceContext.GetColumnNum());
            for (int i = 0; i < spaceContext.GetRowNum(); i++)
            {
                for (int j = 0; j < spaceContext.GetColumnNum(); j++)
                {
                    if (i == splitLoc_Height)
                        i++;

                    if (i < splitLoc_Height)
                    {
                        //Debug.Log("[" + i + "," + j + "] Va por izq");
                        this.spaceContext.SetValor(i,j, leftNode.spaceContext.ToBoolMatrix()[i,j]);
                    }
                    else
                    {
                        //Debug.Log("[" + i + "," + j + "]Va por der, se busca:[" + (i-splitLoc_Height-1) + "," + j + "]");
                        this.spaceContext.SetValor(i, j, rightNode.spaceContext.ToBoolMatrix()[i-splitLoc_Height-1, j]);
                    }
                }
            }

        }
        else
        {
            int splitLoc_Width = (int)Random.Range(min_room_width, spaceContext.GetColumnNum() - min_room_width);
            //Debug.Log("Corte en:" + splitLoc_Width);
            //Debug.Log("Se va a crear izq:" + spaceContext.GetRowNum() + "X" + splitLoc_Width);
            //Debug.Log("Se va a crear der:" + spaceContext.GetRowNum() + "X" + (spaceContext.GetColumnNum() - splitLoc_Width - 1));

            leftNode = new RoomNode(new bool[spaceContext.GetRowNum(), splitLoc_Width], min_room_width, min_room_height, max_room_width, max_room_height);
            rightNode = new RoomNode(new bool[spaceContext.GetRowNum(), spaceContext.GetColumnNum() - splitLoc_Width - 1], min_room_width, min_room_height, max_room_width, max_room_height);

            for (int i = 0; i < spaceContext.GetRowNum(); i++)
            {
                for (int j = 0; j < spaceContext.GetColumnNum(); j++)
                {
                    if (j == splitLoc_Width)
                        j++;

                    if (j < splitLoc_Width)
                    {
                        //Debug.Log("[" + i + "," + j + "] Va por izq");
                        this.spaceContext.SetValor(i, j, leftNode.spaceContext.ToBoolMatrix()[i, j]);
                    }
                    else
                    {
                        //Debug.Log("[" + i + "," + j + "]Va por der, se busca:[" + (i-splitLoc_Height-1) + "," + j + "]");
                        this.spaceContext.SetValor(i, j, rightNode.spaceContext.ToBoolMatrix()[i, j - splitLoc_Width - 1]);
                    }
                }
            }
        }     
    }

    public Dungeon getMap() {
        return this.spaceContext; 
    }
}