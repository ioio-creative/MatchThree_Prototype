using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoardUtils
{
    public static Vector2 WorldToCoordScale = Vector2.one;
    public static float BlockZDepth;

    //Return World Pos from Tile Coordinates, zero-offset
    public static Vector3 GetWorldPosFromTileCoord(Vector2Int tileCoord)
    {
        return new Vector3(tileCoord.x * WorldToCoordScale.x, tileCoord.y * WorldToCoordScale.y, BlockZDepth);
    }

}
