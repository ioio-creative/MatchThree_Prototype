
using System;
using System.Collections.Generic;
using UnityEngine;

public struct Tile
{
    public readonly int x;
    public readonly int y;
    public Vector2Int Coordinates => new Vector2Int(x, y);

    public BlockView Occupant;
    public BlockType OccupantType => Occupant == null ? null : Occupant.Type;

    public Tile(int x, int y, BlockView block = null)
    {
        this.x = x;
        this.y = y;
        Occupant = block;
    }

    public Tile(Vector2Int coord, BlockView block = null) : this(coord.x, coord.y, block) { }
}

public static class TileExtension
{
    public static HashSet<Vector2Int> FindMatches(this Tile[,] tiles, int matchCount)
    {
        HashSet<Vector2Int> matches = new HashSet<Vector2Int>();

        //check for horizontal matches
        for (int y = 0; y < tiles.GetLength(0); y++)
        {
            var matchType = tiles[y, 0].OccupantType;
            int _matchCnt = 1;
            //no need to check last 2 elements as we have been looking ahead 2 tiles
            for (int x = 1; x < tiles.GetLength(1); x++)
            {
               
                if (matchType != null && matchType == tiles[y, x].OccupantType) _matchCnt++;
                else
                {
                    matchType = tiles[y, x].OccupantType;

                    if (_matchCnt >= matchCount)
                    {
                        //backtracking adding to match hash set
                        for (int x0 = x - 1; x0 >= x - _matchCnt; x0--)
                        {
                            matches.Add(tiles[y, x0].Coordinates);
                        }
                    }
                    _matchCnt = 1;

                    //no need to check last (matchCount-1) elements if after the type reset
                    if (x > tiles.GetLength(1) - matchCount) continue;
                }
            }

            //in case last element per row is a match 
            if (_matchCnt >= matchCount)
            {
                //backtracking adding to match hash set
                for (int x0 = tiles.GetUpperBound(1); x0 > tiles.GetUpperBound(1) - _matchCnt; x0--)
                {
                    matches.Add(tiles[y, x0].Coordinates);
                }
            }
        }

        //check for vertical matches
        for (int x = 0; x < tiles.GetLength(1); x++)
        {
            var matchType = tiles[0, x].OccupantType;
            int _matchCnt = 1;
            for (int y = 1; y < tiles.GetLength(0); y++)
            {

                if (matchType != null && matchType == tiles[y, x].OccupantType) _matchCnt++;
                else
                {
                    matchType = tiles[y, x].OccupantType;

                    if (_matchCnt >= matchCount)
                    {
                        //backtracking adding to match hash set
                        for (int y0 = y - 1; y0 >= y - _matchCnt; y0--)
                        {
                            matches.Add(tiles[y0, x].Coordinates);
                        }
                    }
                    _matchCnt = 1;

                    //no need to check last (matchCount-1) elements if after the type reset
                    if (y > tiles.GetLength(0) - matchCount) continue;

                }
            }

            //in case last element per column is a match 
            if (_matchCnt >= matchCount)
            {
                //backtracking adding to match hash set
                for (int y0 = tiles.GetUpperBound(0); y0 > tiles.GetUpperBound(0) - _matchCnt; y0--)
                {
                    matches.Add(tiles[y0, x].Coordinates);
                }
            }
        }

        return matches;
    }
}
