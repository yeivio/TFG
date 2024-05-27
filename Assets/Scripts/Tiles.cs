using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Tiles : MonoBehaviour
{
    [Header("Straight")]
    public List<Tiles> TopPosibilities;
    public List<Tiles> RightPosibilities;
    public List<Tiles> LeftPosibilities;
    public List<Tiles> BottomPosibilities;

    [Header("Diagonals")]
    public List<Tiles> TopLeftDiagonalPosibilities;
    public List<Tiles> TopRightPosibilities;
    public List<Tiles> BottomLeftPosibilities;
    public List<Tiles> BottomRightPosibilities;
    [Header("Maze")]
    public bool isLeftConnected;
    public bool isRightConnected;
    public bool isBottomConnected;
    public bool isTopConnected;

    [Header("WSP-Paint")]
    public GameObject MODEL_3D;

    [Header("WSP-Paint")]
    public GenerationAlgorithm.CELL_TYPE cellType;
}
