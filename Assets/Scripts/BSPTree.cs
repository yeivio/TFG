using NUnit.Framework.Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

// Bug -869890222 52 52 4 16 4 16

public class BSPTree : GenerationAlgorithm
{

    public bool[,] Generate(int numColumns, int numRows, int min_room_width, int min_room_height, int max_room_width, int max_room_height, int seed = -1)
    {
        this.widthMap = numColumns;
        this.heightMap = numRows;
        map = new bool[heightMap, widthMap];
        this.seed = seed.ToString();
        GenerateSeed(seed);
        Debug.Log("seed:" + this.seed);
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
    private bool iamleaf;
    public Vector2 startHousePosition;  // Height x Width
    public Vector2 houseSize;   // sizes in width x height

    public RoomNode(bool[,] spaceCtx, int min_room_width, int min_room_height, int max_room_width, int max_room_height)
    {
        this.min_room_height = min_room_height;
        this.max_room_width = max_room_width;
        this.min_room_width = min_room_width;
        this.max_room_height = max_room_height;

        this.spaceContext = new Dungeon(spaceCtx);
        //If there isnt enough space for a room, then he is a leaf
        if (((float)spaceContext.GetRowNum() / 2) > max_room_height ||
            ((float)spaceContext.GetColumnNum() / 2) > max_room_width)
        {
            this.Split();
        }
        else
        { 
            //Draw room when you can't divide
            int houseWidth = Random.Range(this.min_room_width, Mathf.Min(this.max_room_width, spaceContext.GetColumnNum()) + 1);
            int houseHeight = Random.Range(this.min_room_height, Mathf.Min(this.max_room_height, spaceContext.GetRowNum()) + 1);

            int startWidth = Random.Range(0, (spaceContext.GetColumnNum() - houseWidth)+1);
            int startheight = Random.Range(0, (spaceContext.GetRowNum() - houseHeight)+1);

            startHousePosition = new Vector2(startheight, startWidth);
            houseSize = new Vector2(houseWidth, houseHeight);
            //Debug.Log("Se va a construir una casita en[" + startheight + "," + startWidth + "]de" + houseHeight + "x" + houseWidth);
            //Debug.Log("Estoy en matriz de [" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
            for (int i = startheight; i < startheight + houseHeight; i++)
            {
                for (int j = startWidth; j < startWidth + houseWidth; j++)
                {
                    //Debug.Log("Accedo:[" + i + "," + j + "]->"  + ((i * spaceContext.GetColumnNum()) + j));
                    this.spaceContext.SetValor((i * spaceContext.GetColumnNum()) + j, true);
                }
            }
            iamleaf = true;
        }


    }


    public void Split()
    {
        // Calculate if we are getting an horizontal or vertical split 
        bool splitHorizonal;
        if (((float)spaceContext.GetRowNum() / 2) > min_room_height &&
            ((float)spaceContext.GetColumnNum() / 2) > min_room_width)
        {
            splitHorizonal = Random.Range(0.0f, 1.0f) > 0.5;    //Random election
        }
        else
        {
            splitHorizonal = ((float)spaceContext.GetRowNum() / 2) > min_room_height;
        }
        if (splitHorizonal)
        {
            int splitLoc_Height = (int)Random.Range(min_room_height, spaceContext.GetRowNum() - min_room_height);
            //Debug.Log("Corte Horizontal en:" + splitLoc_Height + ",Soy[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
            //Debug.Log("Se va a crear izq:" + splitLoc_Height + "X" + spaceContext.GetColumnNum());
            //Debug.Log("Se va a crear der:" + (spaceContext.GetRowNum() - splitLoc_Height - 1) + "X" + spaceContext.GetColumnNum());
            leftNode = new RoomNode(new bool[splitLoc_Height, spaceContext.GetColumnNum()], min_room_width, min_room_height, max_room_width, max_room_height);
            rightNode = new RoomNode(new bool[spaceContext.GetRowNum() - splitLoc_Height - 1, spaceContext.GetColumnNum()], min_room_width, min_room_height, max_room_width, max_room_height);
            
            for (int i = 0; i < spaceContext.GetRowNum(); i++)
            {
                for (int j = 0; j < spaceContext.GetColumnNum(); j++)
                {
                    if (i == splitLoc_Height)
                        i++;

                    if (i < splitLoc_Height)
                    {
                        this.spaceContext.SetValor(i, j, leftNode.spaceContext.getValor(i, j));
                    }
                    else
                    {
                        this.spaceContext.SetValor(i, j, rightNode.spaceContext.getValor(i - splitLoc_Height - 1, j));
                    }
                }
            }

            //Debug.Log("Se empieza union splitHor");
            // Connecting leaf nodes
            if (leftNode.isLeaf() && rightNode.isLeaf())
            {

                //Debug.Log("Soy[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
                //Debug.Log(leftNode.getHouseStartPosition() + "," + leftNode.getHouseSize());
                //Debug.Log(rightNode.getHouseStartPosition() + "," + rightNode.getHouseSize());

                int aux_start = (int) Mathf.Max(leftNode.getHouseStartPosition().y, rightNode.getHouseStartPosition().y);
                int aux_finish = (int) Mathf.Min(leftNode.getHouseStartPosition().y + (leftNode.getHouseSize().x-1),
                    rightNode.getHouseStartPosition().y + (rightNode.getHouseSize().x-1));
                int match_size = 0;

                //Debug.Log("aux_start:" + leftNode.getHouseStartPosition().y + "," + rightNode.getHouseStartPosition().y);
                //Debug.Log("aux_finish:" + (leftNode.getHouseStartPosition().y + (leftNode.getHouseSize().y - 1)) + "," + (rightNode.getHouseStartPosition().y + (rightNode.getHouseSize().y - 1)));
                //Debug.Log("Se empieza en:" + aux_start + ",Se acaba en:" + aux_finish);
                for (int i = Mathf.Min(aux_start, aux_finish); i <= Mathf.Max(aux_start, aux_finish); i++)
                {
                    //Debug.Log("Se accede IZQ:" + ((int)leftNode.getHouseSize().y - 1) + "," + i);
                    //Debug.Log("Se accede DER:" + ((int)rightNode.getHouseSize().y - 1) + "," + i);
                    if(leftNode.spaceContext.getValor((int)leftNode.getHouseSize().y-1,i) &&
                        rightNode.spaceContext.getValor((int)rightNode.getHouseStartPosition().x,i))
                    {
                        match_size++;
                    }
                }

                //Debug.Log("Coincidencia de longitud:" + match_size);

                if (match_size > 0)
                {
                    int thicc = Random.Range(1, match_size); // If we add +1 on match size, we can completely join two coincident rooms
                    int tunnelStart = Random.Range(0,(match_size - thicc)+1);
                    //Debug.Log("thicc:" + thicc + ",start:" + tunnelStart);

                    //Debug.Log(leftNode.getHouseStartPosition() + "," + leftNode.getHouseSize());

                    //Debug.Log("Comienza el bucle X en:" + (aux_start + tunnelStart) +
                    //",Acaba en " + (aux_start + (match_size - thicc)));
                    //Debug.Log("Comienza el bucle Y en:" + (leftNode.getHouseStartPosition().x + leftNode.getHouseSize().y) + 
                    //    ",Acaba en " + ((int)rightNode.getHouseStartPosition().x + splitLoc_Height));

                    for (int i = aux_start + tunnelStart; i < aux_start + tunnelStart + thicc; i++)
                    {
                        for(int j = (int)(leftNode.getHouseStartPosition().x + leftNode.getHouseSize().y); 
                            j <= (int)rightNode.getHouseStartPosition().x + splitLoc_Height; j++)
                        {
                            this.spaceContext.SetValor(j, i, true);
                        }
                    }
                    
                }
            }



        }
        else
        {
            int splitLoc_Width = (int)Random.Range(min_room_width, spaceContext.GetColumnNum() - min_room_width);


            //Debug.Log("Corte Vertical en:" + splitLoc_Width + ",Soy[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
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
                        this.spaceContext.SetValor(i, j, leftNode.spaceContext.getValor(i, j));
                    }
                    else
                    {
                        this.spaceContext.SetValor(i, j, rightNode.spaceContext.getValor(i, j - splitLoc_Width - 1));
                    }
                }
            }
            //Debug.Log("Se empieza union splitVert");

            // Connecting leaf nodes
            if (leftNode.isLeaf() && rightNode.isLeaf())
            {
                //Debug.Log("Soy[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
                //Debug.Log("Izq[" + leftNode.spaceContext.GetRowNum() + "," + leftNode.spaceContext.GetColumnNum() + "]");
                //Debug.Log("Der[" + rightNode.spaceContext.GetRowNum() + "," + rightNode.spaceContext.GetColumnNum() + "]");

                //Debug.Log(leftNode.getHouseStartPosition() + "," + leftNode.getHouseSize());
                //Debug.Log(rightNode.getHouseStartPosition() + "," + rightNode.getHouseSize());

                int aux_start = (int)Mathf.Max(leftNode.getHouseStartPosition().x, rightNode.getHouseStartPosition().x);
                int aux_finish = (int)Mathf.Min(leftNode.getHouseStartPosition().x + (leftNode.getHouseSize().y - 1),
                    rightNode.getHouseStartPosition().x + (rightNode.getHouseSize().y - 1));
                int match_size = 0;

                //Debug.Log("aux_start:" + leftNode.getHouseStartPosition().x + "," + rightNode.getHouseStartPosition().x);
                //Debug.Log("aux_finish:" + (leftNode.getHouseStartPosition().x + (leftNode.getHouseSize().x - 1)) + "," + (rightNode.getHouseStartPosition().x + (rightNode.getHouseSize().x - 1)));
                //Debug.Log("Se empieza en:" + aux_start + ",Se acaba en:" + aux_finish);
                
                for (int i = Mathf.Min(aux_start, aux_finish); i <= Mathf.Max(aux_start, aux_finish); i++)
                {
                    //Debug.Log("Se accede IZQ:" + (i + "," + ((int)leftNode.getHouseSize().x - 1)));
                    //Debug.Log("Se accede DER:" + (i + "," + ((int)rightNode.getHouseSize().x - 1)));
                    //Debug.Log(i * leftNode.spaceContext.GetColumnNum() + ((int)rightNode.getHouseSize().x - 1));
                    if (leftNode.spaceContext.getValor(i, (int)leftNode.getHouseSize().x - 1) &&
                        rightNode.spaceContext.getValor(i, (int)rightNode.getHouseStartPosition().y))
                    {
                        match_size++;
                    }
                }

                //Debug.Log("Coincidencia de longitud:" + match_size);

                if (match_size > 0)
                {
                    int thicc = Random.Range(1, match_size); // If we add +1 on match size, we can completely join two coincident rooms
                    int tunnelStart = Random.Range(0, (match_size - thicc) + 1);
                    //Debug.Log("thicc:" + thicc + ",start:" + tunnelStart);

                    //Debug.Log(leftNode.getHouseStartPosition() + "," + leftNode.getHouseSize());

                    //Debug.Log("Comienza el bucle X en:" + (aux_start + tunnelStart) +
                    //",Acaba en " + (aux_start + (match_size - thicc)));
                    //Debug.Log("Comienza el bucle Y en:" + (leftNode.getHouseStartPosition().y + leftNode.getHouseSize().x) + 
                    //    ",Acaba en " + ((int)rightNode.getHouseStartPosition().y + splitLoc_Width));

                    for (int i = aux_start + tunnelStart; i < aux_start + tunnelStart + thicc; i++)
                    {
                        for (int j = (int)(leftNode.getHouseStartPosition().y + leftNode.getHouseSize().x);
                            j <= (int)rightNode.getHouseStartPosition().y + splitLoc_Width; j++)
                        {
                            this.spaceContext.SetValor(i,j, true);
                        }
                    }

                }
            }
        }
    }

    public Dungeon getMap()
    {
        return this.spaceContext;
    }

    public bool isLeaf() { return iamleaf; }

    public Vector2 getHouseStartPosition() { return this.startHousePosition; }
    public Vector2 getHouseSize() { return this.houseSize; }
}