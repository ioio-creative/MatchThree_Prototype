using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public class BoardTestJSON
{
    public TestResult[] results;

    public string SerializeJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void SortBySeed()
    {
        Array.Sort(results, (x, y) => (x.seed.CompareTo(y.seed)));
    }
}

[System.Serializable]
public struct TestResult
{
    public int seed;
    public int types;
    public int moves;
    public int tilesLeft;
}
