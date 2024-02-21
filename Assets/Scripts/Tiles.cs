using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Tiles : MonoBehaviour
{
    public Tiles[] TopPosibilities;
    public Tiles[] RightPosibilities;
    public Tiles[] LeftPosibilities;
    public Tiles[] BottomPosibilities;


    public bool isLeftConnected;
    public bool isRightConnected;
    public bool isBottomConnected;
    public bool isTopConnected;
}
