using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameData
{
    public GameData()
    {
        RandomSeed = Random.Range(int.MinValue, int.MaxValue);
    }

    public int RandomSeed;
}