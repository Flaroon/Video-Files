using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    public int xSize, ySize;
    private int[,] Walltype;
    public GameObject Tile;
    [Range(0, 15)]
    public int SmoothingSteps;
    [Range(0, 100)]
    public float FillPercent;

    private void Start()
    {
        Walltype = new int[xSize, ySize];

        // Set Values

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (x == 0 || x == xSize - 1 || y == 0 || y == ySize - 1)
                    Walltype[x, y] = 0;
                else
                {
                    if (FillPercent * 10 > UnityEngine.Random.Range(0, 1000))
                        Walltype[x, y] = 1;
                    else
                        Walltype[x, y] = 0;
                }
            }
        }

        // Spawning The cubes

        void instansiateCubes()
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (Walltype[x, y] == 1)
                        Instantiate(Tile, new Vector3(x, 5, y), Quaternion.identity);
                }
            }
        }

        // Smoothing

        for (int a = 0; a < SmoothingSteps; a++)
        {
            for (int x = 1; x < (xSize - 1); x++)
            {
                for (int y = 1; y < (ySize - 1); y++)
                {
                    int tilecount = findTileCount(x, y);

                    if (tilecount >= 5)
                        Walltype[x, y] = 1;
                    else if (tilecount < 4)
                        Walltype[x, y] = 0;
                }
            }
        }

        int findTileCount(int x, int y)
        {
            int finalCount = 0;

            int a = Walltype[x - 1, y - 1];
            if (a == 1)
                finalCount += 1;
            int b = Walltype[x - 1, y];
            if (b == 1)
                finalCount += 1;
            int c = Walltype[x-  1, y + 1];
            if (c == 1)
                finalCount += 1;
            int d = Walltype[x, y + 1];
            if (d == 1)
                finalCount += 1;
            int e = Walltype[x + 1, y + 1];
            if (e == 1)
                finalCount += 1;
            int f = Walltype[x + 1, y];
            if (f == 1)
                finalCount += 1;
            int g = Walltype[x + 1, y - 1];
            if (g == 1)
                finalCount += 1;
            int h = Walltype[x, y - 1];
            if (h == 1)
                finalCount += 1;

            return finalCount;
        }

        instansiateCubes();

    }

}
