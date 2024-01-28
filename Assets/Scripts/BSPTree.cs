using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// Bug -1796554237 9 9 4 4 3 3 

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
        RoomNode a = new RoomNode(map, min_room_width, min_room_height, max_room_width, max_room_height);
        return a.getMap().ToBoolMatrix(); ;
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

    private Room room;

    public Vector2 absolutePos;   // position in the root matrix

    public bool splitHorizonal; // If the node makes an horizontal split
    public int splitLocation;
    public static int numNodo = 0;
    public int id;


    public RoomNode(bool[,] spaceCtx, int min_room_width, int min_room_height, int max_room_width, int max_room_height)
    {
         
        this.min_room_height = min_room_height;
        this.max_room_width = max_room_width;
        this.min_room_width = min_room_width;
        this.max_room_height = max_room_height;
        id = numNodo;
        numNodo++;
        
        this.spaceContext = new Dungeon(spaceCtx);

        //Debug.Log(id + "Nazco[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
        //If there isnt enough space for a room, then he is a leaf
        if (((float)spaceContext.GetRowNum() / 2) > max_room_height ||
            ((float)spaceContext.GetColumnNum() / 2) > max_room_width)
        {
            this.Split();
        }
        else
        {
            //Debug.Log(id + "Soy dibujo[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
            //Draw room when you can't divide
            int houseWidth = Random.Range(this.min_room_width, Mathf.Min(this.max_room_width, spaceContext.GetColumnNum()) + 1);
            int houseHeight = Random.Range(this.min_room_height, Mathf.Min(this.max_room_height, spaceContext.GetRowNum()) + 1);

            int startWidth = Random.Range(0, (spaceContext.GetColumnNum() - houseWidth) + 1);
            int startheight = Random.Range(0, (spaceContext.GetRowNum() - houseHeight) + 1);

            this.room = new Room(new Vector2(startheight, startWidth), new Vector2(houseWidth, houseHeight));

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
            splitLocation = (int)Random.Range(min_room_height, spaceContext.GetRowNum() - min_room_height);

            //Debug.Log(id + "Corte Horizontal en:" + splitLocation + ",Soy[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
            //Debug.Log("Se va a crear izq:" + splitLocation + "X" + spaceContext.GetColumnNum());
            //Debug.Log("Se va a crear der:" + (spaceContext.GetRowNum() - splitLocation - 1) + "X" + spaceContext.GetColumnNum());
            
            leftNode = new RoomNode(new bool[splitLocation, spaceContext.GetColumnNum()], min_room_width, min_room_height, max_room_width, max_room_height);
            rightNode = new RoomNode(new bool[spaceContext.GetRowNum() - splitLocation - 1, spaceContext.GetColumnNum()], min_room_width, min_room_height, max_room_width, max_room_height);

            for (int i = 0; i < spaceContext.GetRowNum(); i++)
            {
                for (int j = 0; j < spaceContext.GetColumnNum(); j++)
                {
                    if (i == splitLocation)
                        i++;

                    if (i < splitLocation)
                    {
                        this.spaceContext.SetValor(i, j, leftNode.spaceContext.getValor(i, j));
                    }
                    else
                    {
                        this.spaceContext.SetValor(i, j, rightNode.spaceContext.getValor(i - splitLocation - 1, j));
                    }
                }
            }

        }
        else
        {
            splitLocation = (int)Random.Range(min_room_width, spaceContext.GetColumnNum() - min_room_width);

            //Debug.Log(id + "Corte Vertical en:" + splitLocation + ",Soy[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
            //Debug.Log("Se va a crear izq:" + spaceContext.GetRowNum() + "X" + splitLocation);
            //Debug.Log("Se va a crear der:" + spaceContext.GetRowNum() + "X" + (spaceContext.GetColumnNum() - splitLocation - 1));

            leftNode = new RoomNode(new bool[spaceContext.GetRowNum(), splitLocation], min_room_width, min_room_height, max_room_width, max_room_height);
            rightNode = new RoomNode(new bool[spaceContext.GetRowNum(), spaceContext.GetColumnNum() - splitLocation - 1], min_room_width, min_room_height, max_room_width, max_room_height);

            for (int i = 0; i < spaceContext.GetRowNum(); i++)
            {
                for (int j = 0; j < spaceContext.GetColumnNum(); j++)
                {
                    if (j == splitLocation)
                        j++;

                    if (j < splitLocation)
                    {
                        this.spaceContext.SetValor(i, j, leftNode.spaceContext.getValor(i, j));
                    }
                    else
                    {
                        this.spaceContext.SetValor(i, j, rightNode.spaceContext.getValor(i, j - splitLocation - 1));
                    }
                }
            }
            
        }
    }

    private void JoinRooms()
    {
        if (splitHorizonal)
        {

            Debug.Log(id + "Se empieza union splitHor,Soy[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");

            //Debug.Log("Soy[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
            //Debug.Log("Izq[" + leftNode.spaceContext.GetRowNum() + "," + leftNode.spaceContext.GetColumnNum() + "]");
            //Debug.Log("Der[" + rightNode.spaceContext.GetRowNum() + "," + rightNode.spaceContext.GetColumnNum() + "]");

            //Debug.Log(leftNode.getHouseStartPosition() + "," + leftNode.getHouseSize());
            //Debug.Log(rightNode.getHouseStartPosition() + "," + rightNode.getHouseSize());

            // Connecting leaf nodes
            int aux_start = (int)Mathf.Max(leftNode.getHouseStartPosition().y, rightNode.getHouseStartPosition().y);
            int aux_finish = (int)Mathf.Min(leftNode.getHouseStartPosition().y + (leftNode.getHouseSize().x - 1),
                rightNode.getHouseStartPosition().y + (rightNode.getHouseSize().x - 1));
            int match_size = 0;
            for (int i = Mathf.Min(aux_start, aux_finish); i <= Mathf.Max(aux_start, aux_finish); i++)
            {
                if (leftNode.spaceContext.getValor((int)leftNode.getHouseSize().y - 1, i) &&
                    rightNode.spaceContext.getValor((int)rightNode.getHouseStartPosition().x, i))
                {
                    match_size++;
                }
            }

            if (match_size > 0)
            {
                //int thicc = Random.Range(1, match_size); // If we add +1 on match size, we can completely join two coincident rooms
                int thicc = 1;
                int tunnelStart = Random.Range(0, (match_size - thicc) + 1);

                for (int i = aux_start + tunnelStart; i < aux_start + tunnelStart + thicc; i++)
                {
                    for (int j = (int)(leftNode.getHouseStartPosition().x + leftNode.getHouseSize().y);
                        j <= (int)rightNode.getHouseStartPosition().x + splitLocation; j++)
                    {
                        this.spaceContext.SetValor(j, i, true);
                    }
                }

            }
            else
            {
                // There is no matching sides so a Z tunnel should be 
                //Fill up or down until split
                for (int i = (int)(leftNode.getHouseStartPosition().y + (leftNode.getHouseSize().y - 1)); i <= splitLocation; i++)
                {
                    this.spaceContext.SetValor(i, (int)(leftNode.getHouseStartPosition().y + (leftNode.getHouseSize().y - 1)), true);
                }
                for (int i = (int)rightNode.getHouseStartPosition().x + splitLocation + 1; i >= splitLocation; i--)
                {
                    this.spaceContext.SetValor(i, (int)rightNode.getHouseStartPosition().x + splitLocation + 1, true);
                }

                if (leftNode.getHouseStartPosition().y < rightNode.getHouseStartPosition().y)
                {
                    //LeftNode is more left than right
                    //Fill until reaching the other point through the split point
                    for (int i = (int)(leftNode.getHouseStartPosition().y + (leftNode.getHouseSize().y - 1));
                        i <= (int)rightNode.getHouseStartPosition().y; i++)
                    {
                        this.spaceContext.SetValor(splitLocation, i, true);
                    }
                }
                else
                {
                    for (int i = (int)(rightNode.getHouseStartPosition().y);
                        i >= ((int)leftNode.getHouseStartPosition().y + (leftNode.getHouseSize().x -1)); i--)
                    {
                        this.spaceContext.SetValor(splitLocation, i, true);
                    }
                }
                
            }


            //Debug.Log("Me guardo un nodo,Soy[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
            int posibilities = 2; // LeftNode Rightnode
            switch (Random.Range(0, posibilities))
            {
                case 0: //Leftnode
                    //Debug.Log("izuerdo");
                    this.houseSize = leftNode.houseSize;
                    this.startHousePosition = leftNode.startHousePosition;
                    //Debug.Log("HouseSize[" + houseSize.x + "," + houseSize.y + "], StartPos:[" + startHousePosition.x + "," + startHousePosition.y + "]");

                    break;
                case 1: //Rightnode
                    //Debug.Log("izuerdo");

                    this.houseSize = rightNode.houseSize;
                    this.startHousePosition = new Vector2(rightNode.startHousePosition.x + splitLocation + 1, rightNode.startHousePosition.y);
                    //Debug.Log("HouseSize[" + houseSize.x + "," + houseSize.y + "], StartPos:[" + startHousePosition.x + "," + startHousePosition.y + "]");

                    break;
            }
        }
        else
        {

            //Debug.Log(id + "Se empieza union splitVert,Soy[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");

            // Connecting leaf nodes
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
                //int thicc = Random.Range(1, match_size); // If we add +1 on match size, we can completely join two coincident rooms
                int thicc = 1;
                int tunnelStart = Random.Range(0, (match_size - thicc) + 1);
                //Debug.Log("thicc:" + thicc + ",start:" + tunnelStart);

                //Debug.Log(leftNode.getHouseStartPosition() + "," + leftNode.getHouseSize());

                //Debug.Log("Comienza el bucle X en:" + (aux_start + tunnelStart) +
                //",Acaba en " + (aux_start + (match_size - thicc)));
                //Debug.Log("Comienza el bucle Y en:" + (leftNode.getHouseStartPosition().y + leftNode.getHouseSize().x) + 
                //    ",Acaba en " + ((int)rightNode.getHouseStartPosition().y + splitLocation));

                for (int i = aux_start + tunnelStart; i < aux_start + tunnelStart + thicc; i++)
                {
                    for (int j = (int)(leftNode.getHouseStartPosition().y + leftNode.getHouseSize().x);
                        j <= (int)rightNode.getHouseStartPosition().y + splitLocation; j++)
                    {
                        this.spaceContext.SetValor(i, j, true);
                    }
                }

            }
            else
            {
                // There is no matching sides so a Z tunnel should be 
                // Fill up or down until split
                for (int i = (int)(leftNode.getHouseStartPosition().x + (leftNode.getHouseSize().x - 1)); i <= splitLocation; i++)
                {
                    this.spaceContext.SetValor((int)(leftNode.getHouseStartPosition().x + (leftNode.getHouseSize().x - 1)), i, true);
                }
                for (int i = (int)rightNode.getHouseStartPosition().y + splitLocation + 1; i >= splitLocation; i--)
                {
                    this.spaceContext.SetValor((int)rightNode.getHouseStartPosition().y + splitLocation + 1, i, true);
                }

                if (leftNode.getHouseStartPosition().x < rightNode.getHouseStartPosition().x)
                {
                    //LeftNode is more left than right
                    //Fill until reaching the other point through the split point
                    for (int i = (int)(leftNode.getHouseStartPosition().x + (leftNode.getHouseSize().x - 1));
                        i <= (int)rightNode.getHouseStartPosition().x; i++)
                    {
                        this.spaceContext.SetValor( i, splitLocation, true);
                    }
                }
                else
                {
                    for (int i = (int)(rightNode.getHouseStartPosition().x);
                        i >= ((int)leftNode.getHouseStartPosition().x + (leftNode.getHouseSize().y - 1)); i--)
                    {
                        this.spaceContext.SetValor( i, splitLocation, true);
                    }
                }

            }

            //Debug.Log("Me guardo un nodo,Soy[" + spaceContext.GetRowNum() + "," + spaceContext.GetColumnNum() + "]");
            int posibilities = 2; // LeftNode Rightnode
            switch (Random.Range(0, posibilities))
            {
                case 0: //Leftnode
                    //Debug.Log("izuerdo empiueza en:StartPos: [" + leftNode.startHousePosition.x + ", " + leftNode.startHousePosition.y + "]");
                    //Debug.Log("izuerdo Tamaño: [" + leftNode.houseSize.x + ", " + leftNode.houseSize.y + "]");
                    this.houseSize = leftNode.houseSize;
                    this.startHousePosition = leftNode.startHousePosition;
                    //Debug.Log("HouseSize[" + houseSize.x + "," + houseSize.y + "], StartPos:[" + startHousePosition.x + "," + startHousePosition.y + "]");

                    break;
                case 1: //Rightnode
                    //Debug.Log("derecho empiueza en:StartPos: [" + rightNode.startHousePosition.x + ", " + rightNode.startHousePosition.y + "]");
                    //Debug.Log("derecho Tamaño: [" + rightNode.houseSize.x + ", " + rightNode.houseSize.y + "]");
                    this.houseSize = rightNode.houseSize;
                    this.startHousePosition = new Vector2(rightNode.startHousePosition.x, rightNode.startHousePosition.y + splitLocation + 1);
                    //Debug.Log("HouseSize[" + houseSize.x + "," + houseSize.y + "], StartPos:[" + startHousePosition.x + "," + startHousePosition.y + "]");

                    break;
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


public class Room
{
    private Vector2 startHousePosition;  // Height x Width
    private Vector2 houseSize;   // sizes in width x height


    public Room(Vector2 startPos, Vector2 houseSize)
    {
        this.startHousePosition = new Vector2(startPos.x, startPos.y);
        this.houseSize = new Vector2(houseSize.x, houseSize.y);
    }

    public void setStartHousePosition(Vector2 startPos)
    {
        this.startHousePosition = new Vector2(startPos.x, startPos.y);
    }
    public void setHouseSize(Vector2 houseSize)
    {
        this.houseSize = new Vector2(houseSize.x, houseSize.y);
    }
}