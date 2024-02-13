using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

// Bug -1383767366 18 18 4 4 4 4

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

    public void Generate(int seed = -1)
    {
        this.seed = seed;
        GenerateSeed(seed);
        map = new bool[widthMap, heightMap];

        RoomNode.SetHouseProperties(min_room_height, max_room_width, min_room_width, max_room_height);
        RoomNode.map = map;
        // This is maybe wrong and should be heightmap first
        RoomNode a = new RoomNode(new Vector2Int(0,0), new Vector2Int(widthMap - 1, heightMap - 1));

        this.map = RoomNode.map;
    }
    private class RoomNode
    {
        public static bool[,] map;

        private static int min_room_width;
        private static int max_room_width;
        private static int min_room_height;
        private static int max_room_height;

        // Child nodes
        private RoomNode leftNode;
        private RoomNode rightNode;

        // Coordinates the node is positioned
        private Vector2Int startContext;
        private Vector2Int endContext;
        private Dungeon spaceContext;

        // If the node is leaf and the room connected
        private bool iamleaf;
        private Room room;

        // Split information
        public bool splitHorizonal; // If the node makes an horizontal split
        public int splitLocation;


        public RoomNode(Vector2Int startContext, Vector2Int endContext)
        {

            this.startContext = startContext;
            this.endContext = endContext;
            this.spaceContext = new Dungeon(startContext, endContext);

            //Debug.Log("Nazco[" + startContext + "," + endContext + "]");
            //Debug.Log("Nazco[" + spaceContext.GetColumnNum() + "," + spaceContext.GetRowNum() + "]");
            //If there isnt enough space for a room, then he is a leaf
            if (((float)spaceContext.GetRowNum() / 2) > max_room_height ||
                ((float)spaceContext.GetColumnNum() / 2) > max_room_width)
            {
                this.Split();
            }
            else
            {
                //Debug.Log("Soy dibujo[" + spaceContext.GetColumnNum() + "," + spaceContext.GetRowNum() + "]");
                //Draw room when you can't divide
                int houseWidth = Random.Range(min_room_width, Mathf.Min(max_room_width, spaceContext.GetColumnNum()) + 1);
                int houseHeight = Random.Range(min_room_height, Mathf.Min(max_room_height, spaceContext.GetRowNum()) + 1);

                int startWidth = Random.Range(startContext.x, startContext.x + (spaceContext.GetColumnNum() - houseWidth));
                int startheight = Random.Range(startContext.y, startContext.y + (spaceContext.GetRowNum() - houseHeight));

                //Debug.Log("Soy originalmente[" + startContext + "," + endContext + "]");
                //Debug.Log("Soy dibujo[" + startWidth + "," + startheight + "]");
                //Debug.Log("Soy tama�o[" + houseWidth + "," + houseHeight + "]");

                
                this.room = new Room(new Vector2Int(startWidth, startheight), new Vector2Int(houseWidth, houseHeight));
                iamleaf = true;

                for (int i = room.getHouseSize().x; i > 0; i--)
                {
                    for (int j = room.getHouseSize().y; j > 0; j--)
                    {
                        map[room.getStartHousePosition().x - 1 + i, room.getStartHousePosition().y - 1 + j] = true;
                    }
                }
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
                splitLocation = (int)Random.Range(startContext.y + min_room_height, endContext.y - min_room_height + 1);
                
                //Debug.Log("Corte Horizontal en:" + splitLocation + ",Soy[" + startContext + "," + endContext + "]");
                //Debug.Log("Se va a crear izq:" + startContext + "X" + new Vector2Int(endContext.x, splitLocation - 1));
                //Debug.Log("Se va a crear der:" + new Vector2Int(startContext.x, splitLocation + 1) + "X" + endContext);

                leftNode = new RoomNode(startContext, new Vector2Int(endContext.x, splitLocation - 1)); // Down
                rightNode = new RoomNode(new Vector2Int(startContext.x, splitLocation + 1), endContext); // Up

            }
            else
            {
                splitLocation = (int)Random.Range(startContext.x + min_room_width, endContext.x - min_room_width + 1);

                //Debug.Log("Corte Vertical en:" + splitLocation + ",Soy[" + startContext + "," + endContext + "]");
                //Debug.Log("Se va a crear izq:" + startContext + "X" + new Vector2Int(splitLocation - 1, endContext.y));
                //Debug.Log("Se va a crear der:" + new Vector2Int(splitLocation + 1, startContext.y) + "X" + endContext);

                leftNode = new RoomNode(startContext, new Vector2Int(splitLocation - 1, endContext.y)); // Left
                rightNode = new RoomNode(new Vector2Int(splitLocation + 1, startContext.y), endContext); // Right
            }

            // Joining rooms

        }

        public Dungeon GetMap()
        {
            return this.spaceContext;
        }

        public bool IsLeaf() { return iamleaf; }

        public Vector2 GetHouseStartPosition() { return this.room.getStartHousePosition(); }
        public Vector2 GetHouseSize() { return this.room.getHouseSize(); }

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

        private List<Vector2Int> roomPivotes; // Corners of the house.
        // 1. Left - Down
        // 2. Left - Up
        // 3. Right - Down
        // 4. Right -Up



        public Room(Vector2Int startPos, Vector2Int houseSize)
        {
            this.startHousePosition = new Vector2Int(startPos.x, startPos.y);
            this.houseSize = new Vector2Int(houseSize.x, houseSize.y);
            roomPivotes = new List<Vector2Int>()
            { 
                new Vector2Int(startHousePosition.x, startHousePosition.y),
                new Vector2Int(startHousePosition.x, startHousePosition.y + houseSize.y), 
                new Vector2Int(startHousePosition.x + houseSize.x, startHousePosition.y),
                new Vector2Int(startHousePosition.x + houseSize.x, startHousePosition.y + houseSize.y)

            };
        }
        /// <summary>
        /// Returns the position where the room is placed. Height is X-axe, and width is Y-axe
        /// </summary>
        /// <param name="startPos"></param>
        public void setStartHousePosition(Vector2Int startPos)
        {
            this.startHousePosition = new Vector2Int(startPos.x, startPos.y);
        }
        public void setHouseSize(Vector2Int houseSize)
        {
            this.houseSize = new Vector2Int(houseSize.x, houseSize.y);
        }

        public Vector2Int getStartHousePosition() { return this.startHousePosition; }
        public Vector2Int getHouseSize() { return this.houseSize; }
        /// <summary>
        /// Return the corners of the house. The order is the following
        /// 1. Left - Down
        /// 2. Left - Up
        /// 3. Right - Down
        /// 4. Right -Up
        /// </summary>
        /// <returns></returns>
        public List<Vector2Int> getPivotList() { return this.roomPivotes; }
    }

    private class Tunnel
    {
        public List<Vector2Int> positions;
        public Tunnel()
        {
            this.positions = new List<Vector2Int>();
        }
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
                        if (this.map[j, i])
                            Gizmos.color = Color.black;
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





