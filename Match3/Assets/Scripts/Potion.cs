using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public PotionType potionType;

    public int xIndex;
    public int yIndex;

    public bool isMatched;
    public bool isMoving;

    private Vector2 currentPos;
    private Vector2 targetPos;

    public Potion(int _x,int _y)
    {
        xIndex = _x;
        yIndex = _y;

    }

    public enum PotionType
    {
        Red,
        Blue,
        Pink,
        Green,
        White
    }


}
