using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using static UnityEditor.PlayerSettings;

// 20 20 2 8 2 8 90139017 bug

public class BSPTree : GenerationAlgorithm
{
    [Header("OPTIONS FOR BSPTree")]
    public int min_room_width;
    public int max_room_width;
    public int min_room_height;
    public int max_room_height;
    public int tileSize;    // Size of each tile on the canvas
    public long executionTime;
    public int widthMap;   // Number of columns in the map
    public int heightMap;  // Number of rows in the map

    private bool[,] map;
    private RoomNode.TYPE_CELL[,] debugmap;

    public void Generate(int seed = -1)
    {
        this.seed = seed;
        GenerateSeed(seed);
        map = new bool[widthMap, heightMap];
        this.debugmap = new RoomNode.TYPE_CELL[widthMap, heightMap];
        for(int i = 0; i < widthMap; i++)
        {
            for(int j = 0; j < heightMap; j++)
            {
                this.debugmap[i,j] = RoomNode.TYPE_CELL.SALA;
            }
        }

        RoomNode.SetHouseProperties(min_room_height, max_room_width, min_room_width, max_room_height);
        RoomNode.map = map;

        RoomNode.debugList = this.debugmap;
        


        Debug.Log("s:" + this.seed);
        RoomNode a = new RoomNode(new Vector2Int(0,0), new Vector2Int(widthMap - 1, heightMap - 1), new List<Vector2Int>());
        this.map = RoomNode.map;
        this.debugmap = RoomNode.debugList;
    }
    private class RoomNode
    {
        public static bool[,] map;
        public static int min_room_width;
        public static int max_room_width;
        public static int min_room_height;
        public static int max_room_height;

        // Debugging Data
        public enum TYPE_CELL { PASILLO, SALA, PUERTA}
        public static TYPE_CELL[,] debugList; 


        // Child nodes
        private RoomNode leftNode;
        private RoomNode rightNode;

        // Coordinates the node is positioned
        private Vector2Int startContext;
        private Vector2Int endContext;

        // If the node is leaf and the room connected
        private Room room;
        private List<Vector2Int> listDoors;

        // Split information
        public bool splitHorizonal; // If the node makes an horizontal split
        public int splitLocation = -1;
        public int spaceBetween = 1;
        public int numDoors = 1; // possible doors on every wall


        public int FORCE_BREAK = 40000;
        public int brek = 0;
        public int FORCE_BREAK_2 = 40000;
        public int brek_2 = 0;

        public RoomNode(Vector2Int startContext, Vector2Int endContext, List<Vector2Int> listDoors)
        {
            brek++;
            if (brek >= FORCE_BREAK)
            {
                Debug.Log("Force:");
                return;
            }
            this.startContext = startContext;
            this.endContext = endContext;
            this.listDoors = new List<Vector2Int>(listDoors);

            //If there isnt enough space for a room, then he is a leaf
            if (((float)getHeightContext() / 2) > max_room_height + numDoors||
                ((float)getWidthContext() / 2) > max_room_width + numDoors)
            {
                this.Split();
            }
            else
            {
                //Draw room when you can't divide
                int houseWidth = Random.Range(min_room_width, Mathf.Min(max_room_width, getWidthContext()) + 1);
                int houseHeight = Random.Range(min_room_height, Mathf.Min(max_room_height, getHeightContext()) + 1);

                int startWidth = Random.Range(startContext.x, startContext.x + (getWidthContext() - houseWidth));
                int startheight = Random.Range(startContext.y, startContext.y + (getHeightContext() - houseHeight));
                
                this.room = new Room(new Vector2Int(startWidth, startheight), new Vector2Int(houseWidth, houseHeight));
                for (int i = room.getHouseSize().x; i > 0; i--)
                {
                    for (int j = room.getHouseSize().y; j > 0; j--)
                    {
                        
                        map[room.getStartHousePosition().x - 1 + i, room.getStartHousePosition().y - 1 + j] = true;
                    }
                }

                // Connect room to Door
                foreach (Vector2Int door in listDoors)
                    ConnectRoomToDoor(this.room, door);

            }
        }

        public void Split()
        {
            // Calculate if we are getting an horizontal or vertical split 

            if (((float)getHeightContext() / 2) > min_room_height + numDoors &&
                ((float)getWidthContext() / 2) > min_room_width + numDoors)
            {
                splitHorizonal = Random.Range(0.0f, 1.0f) > 0.5;    //Random election
            }
            else
            {
                splitHorizonal = ((float)getHeightContext() / 2) > min_room_height + numDoors;
            }
            
            if (splitHorizonal)
            {
                splitLocation = (int)Random.Range(startContext.y + min_room_height, endContext.y - min_room_height + spaceBetween);
                Vector2Int door = new Vector2Int(UnityEngine.Random.Range(startContext.x, endContext.x), splitLocation);
                while(DoorOverlapping(splitLocation, splitHorizonal))
                {
                    brek_2++;
                    if (brek_2 >= FORCE_BREAK_2)
                    {
                        Debug.Log("Force2:");
                        return;
                    }
                    splitLocation = (int)Random.Range(startContext.y + min_room_height, endContext.y - min_room_height + spaceBetween);
                    door = new Vector2Int(UnityEngine.Random.Range(startContext.x, endContext.x), splitLocation);
                }
                debugList[door.x, door.y] = TYPE_CELL.PUERTA;

                if (this.listDoors.Count == 0)
                { // First division
                    this.listDoors.Add(door);
                    leftNode = new RoomNode(startContext, new Vector2Int(endContext.x, splitLocation - spaceBetween), this.listDoors); // Down
                    rightNode = new RoomNode(new Vector2Int(startContext.x, splitLocation + spaceBetween), endContext, this.listDoors); // Up
                }
                else
                {
                    this.listDoors.Add(door);
                    List<Vector2Int> LeftList = new List<Vector2Int>();
                    List<Vector2Int> RightList = new List<Vector2Int>();
                    foreach (Vector2Int puerta in new List<Vector2Int>(this.listDoors))
                    {
                        if (splitLocation < puerta.y)
                        {
                            RightList.Add(puerta);
                        }
                        else if (splitLocation > puerta.y)
                        {
                            LeftList.Add(puerta);
                        }
                        if (splitLocation == puerta.y)
                        {
                            RightList.Add(puerta);
                            LeftList.Add(puerta);
                        }

                    }
                    leftNode = new RoomNode(startContext, new Vector2Int(endContext.x, splitLocation - spaceBetween), LeftList); // Down
                    rightNode = new RoomNode(new Vector2Int(startContext.x, splitLocation + spaceBetween), endContext, RightList); // Up

                }


            }
            else
            {
                splitLocation = (int)Random.Range(startContext.x + min_room_width, endContext.x - min_room_width + spaceBetween);
                Vector2Int door = new Vector2Int(splitLocation, UnityEngine.Random.Range(startContext.y, endContext.y));

                while (DoorOverlapping(splitLocation, splitHorizonal))
                {
                    brek_2++;
                    if (brek_2 >= FORCE_BREAK_2)
                    {
                        Debug.Log("Force2:");
                        return;
                    }
                    splitLocation = (int)Random.Range(startContext.x + min_room_width, endContext.x - min_room_width + spaceBetween);
                    door = new Vector2Int(splitLocation, UnityEngine.Random.Range(startContext.x, endContext.x));
                }
                debugList[door.x, door.y] = TYPE_CELL.PUERTA;
                
                

                if (this.listDoors.Count == 0)
                { // First division
                    this.listDoors.Add(door);
                    leftNode = new RoomNode(startContext, new Vector2Int(splitLocation - spaceBetween, endContext.y), this.listDoors); // Left
                    rightNode = new RoomNode(new Vector2Int(splitLocation + spaceBetween, startContext.y), endContext, this.listDoors); // Right
                }
                else
                {
                    this.listDoors.Add(door);
                    List<Vector2Int> LeftList = new List<Vector2Int>();
                    List<Vector2Int> RightList = new List<Vector2Int>();

                    foreach(Vector2Int puerta in new List<Vector2Int>(this.listDoors))
                    {
                        if (splitLocation < puerta.x)
                        {
                            RightList.Add(puerta);
                        }
                        else if(splitLocation > puerta.x)
                        {
                            LeftList.Add(puerta);
                        }
                        if(splitLocation == puerta.x)
                        {
                            RightList.Add(puerta);
                            LeftList.Add(puerta);
                        }
                            
                    }
                    leftNode = new RoomNode(startContext, new Vector2Int(splitLocation - spaceBetween, endContext.y), LeftList); // Left
                    rightNode = new RoomNode(new Vector2Int(splitLocation + spaceBetween, startContext.y), endContext, RightList); // Right
                }

                
            }
        }

        private bool DoorOverlapping(int splitLocation, bool horizontalSplit)
        {
            bool isOverlapping = false;
            foreach (Vector2Int door in listDoors)
            {

                if (horizontalSplit && door.y == splitLocation
                    || !horizontalSplit && door.x == splitLocation)
                {
                    isOverlapping = true;
                    break;
                }
            }
            return isOverlapping;
        }

        private void ConnectRoomToDoor(Room room, Vector2Int door)
        {
            // Door aligns vertically with room
            if (room.getStartHousePosition().x <= door.x && room.getStartHousePosition().x + room.getHouseSize().x -1>= door.x)
            {
                if(room.getStartHousePosition().y < door.y)
                {
                    for (int index = door.y; index > room.getStartHousePosition().y; index--)
                    {
                        map[door.x, index] = true;
                    }
                }
                else
                {
                    // The door is right down from the room
                    for(int index = door.y; index < room.getStartHousePosition().y; index++) 
                    { 
                        map[door.x, index] = true;
                    }
                }
                return;
            }
            
            // Door aligns horizontally with room
            if (room.getStartHousePosition().y <= door.y && room.getStartHousePosition().y + room.getHouseSize().y -1 >= door.y)
            {
                if (room.getStartHousePosition().x < door.x)
                {
                    // The door is right up from the room
                    for (int index = door.x; index > room.getStartHousePosition().x; index--)
                    {
                        map[index, door.y] = true;
                    }
                }
                else
                {

                    for (int index = door.x; index < room.getStartHousePosition().x; index++)
                    {
                        map[index, door.y] = true;
                    }
                }
                return;
            }
           
            // Room is not align with door
            for(int i = Mathf.Min(room.getHouseCenter().x, door.x); i <= Mathf.Max(room.getHouseCenter().x, door.x); i++)
            {
                map[i, Mathf.Min(room.getHouseCenter().y, door.y)] = true;
            }

            
            if(room.getHouseCenter().x < door.x && room.getHouseCenter().y < door.y ||
                room.getHouseCenter().x > door.x && room.getHouseCenter().y > door.y)
            {
                for (int i = Mathf.Min(room.getHouseCenter().y, door.y); i <= Mathf.Max(room.getHouseCenter().y, door.y); i++)
                {
                    map[Mathf.Max(room.getHouseCenter().x, door.x), i] = true;
                }
            }
            else
            {
                for (int i = Mathf.Min(room.getHouseCenter().y, door.y); i <= Mathf.Max(room.getHouseCenter().y, door.y); i++)
                {
                    map[Mathf.Min(room.getHouseCenter().x, door.x), i] = true;
                }
            }

        }
        private int getHeightContext() { return endContext.y - startContext.y + 1 /*+1 is because we need to count the first one also*/ ; }
        private int getWidthContext() { return endContext.x - startContext.x + 1; /*+1 is because we need to count the first one also*/}

        public static void SetHouseProperties(int min_room_height, int max_room_width, int min_room_width, int max_room_height)
        {
            RoomNode.min_room_height = min_room_height;
            RoomNode.max_room_width = max_room_width;
            RoomNode.min_room_width = min_room_width;
            RoomNode.max_room_height = max_room_height;
        }

    }

    private class Room
    {
        private Vector2Int startHousePosition;  // Height x Width
        private Vector2Int houseSize;   // sizes in width x height

        public Room(Vector2Int startPos, Vector2Int houseSize)
        {
            this.startHousePosition = new Vector2Int(startPos.x, startPos.y);
            this.houseSize = new Vector2Int(houseSize.x, houseSize.y);
        }
        public Vector2Int getStartHousePosition() { return this.startHousePosition; }
        public Vector2Int getHouseSize() { return this.houseSize; }
        public Vector2Int getHouseCenter() { return new Vector2Int((int)System.Math.Truncate((System.Decimal)(getStartHousePosition().x + (getStartHousePosition().x + getHouseSize().x - 1)) / 2),
            (int)System.Math.Truncate((System.Decimal)(getStartHousePosition().y + (getStartHousePosition().y + getHouseSize().y - 1)) / 2)); }
    }

    private void OnDrawGizmosSelected()
    {
        if (this.map != null)
        {
            for (int i = 0; i < heightMap; i++)
                for (int j = 0; j < widthMap; j++)
                {
                    try
                    {
                        
                        if (this.map[j, i]) { 
                            Gizmos.color = Color.black;
                            switch (debugmap[j, i])
                            {
                                case RoomNode.TYPE_CELL.PASILLO:
                                    Gizmos.color = Color.blue;
                                    break;
                                case RoomNode.TYPE_CELL.PUERTA:
                                    Gizmos.color = Color.black;
                                    break;
                                default:
                                    Gizmos.color = Color.black;
                                    break;
                            }
                        }
                        else
                            Gizmos.color = Color.white;

                        
                        Gizmos.DrawCube(new Vector3(tileSize * j + 0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                    }
                    catch
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawCube(new Vector3(tileSize * j + 0.5f, tileSize * i + 0.5f, 0), new Vector3(tileSize, tileSize, 1));
                    }
                }
        }
    }
}


# region GizmoEditor
#if UNITY_EDITOR
[CustomEditor(typeof(BSPTree))]
public class ScriptEditorBSP : Editor
{
    private BSPTree gizmoDrawing;

    public override void OnInspectorGUI()
    {
        gizmoDrawing = (BSPTree)target;
        gizmoDrawing.widthMap = EditorGUILayout.IntSlider("Width", gizmoDrawing.widthMap, 0, 300);
        gizmoDrawing.heightMap = EditorGUILayout.IntSlider("Height", gizmoDrawing.heightMap, 0, 300);
        gizmoDrawing.tileSize = EditorGUILayout.IntSlider("Tile Size", gizmoDrawing.tileSize, 1, 100);
        EditorGUILayout.FloatField("Execution time (ms)", gizmoDrawing.executionTime);
        //DrawDefaultInspector(); // Draw all public variables
        EditorGUILayout.Space();
        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);

        gizmoDrawing.min_room_width = EditorGUILayout.IntField("Min Room Width", gizmoDrawing.min_room_width);
        gizmoDrawing.max_room_width = EditorGUILayout.IntField("Max Room Width", gizmoDrawing.max_room_width);

        gizmoDrawing.min_room_height = EditorGUILayout.IntField("Min Room Height", gizmoDrawing.min_room_height);
        gizmoDrawing.max_room_height = EditorGUILayout.IntField("Max Room Height", gizmoDrawing.max_room_height);

        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);

        if (GUILayout.Button("Generate BSP"))
        {
            gizmoDrawing.Generate(gizmoDrawing.seed);
        }
        if (GUILayout.Button("Generate Random BSP"))
        {
            gizmoDrawing.Generate();
        }

        if (GUI.changed)
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif
#endregion





