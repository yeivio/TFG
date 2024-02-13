using System;
using UnityEditor;
using UnityEngine;


public abstract class GenerationAlgorithm : MonoBehaviour
{
    public int seed;

    protected void GenerateSeed(int seed = -1)
    {
        int tempSeed = (int)DateTime.Now.Ticks;
        if (seed == -1) // No seed 
        {
            UnityEngine.Random.InitState(tempSeed);
            this.seed = tempSeed;
        }
        else
            UnityEngine.Random.InitState(seed);
    }

    public int getSeed() { return seed; }
}
