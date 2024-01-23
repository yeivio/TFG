using System;

public abstract class GenerationAlgorithm
{
    protected bool[,] map; // False == Empty | True == Wall
    protected int widthMap;
    protected int heightMap;

    public string seed;

    protected void GenerateSeed(int seed = -1)
    {
        int tempSeed = (int)DateTime.Now.Ticks;
        if (seed == -1) // No seed 
        {
            UnityEngine.Random.InitState(tempSeed);
            this.seed = tempSeed.ToString();
        }
        else
            UnityEngine.Random.InitState(seed);
    }

    public int getSeed() { return (int)Int64.Parse(seed); }
}
