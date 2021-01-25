using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.MeshHelper;

public class InterpolatedMarchingSquares : MonoBehaviour
{
    public int size;
    public float scale;
    int[,] binaryVals;
    float[,] VertexValues;
    List<Vector3> Verts = new List<Vector3>();
    List<int> tris = new List<int>();
    private int[,] TriangulationTable = new int[,]
    {
        {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 7, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 6, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 4, 3, 8, 6, 4, -1, -1, -1, -1, -1, -1},
        {6, 5, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 8, 4, 4, 8, 1, 4, 1, 6, 6, 1, 5},
        {7, 5, 4, 4, 5, 2, -1, -1, -1, -1, -1, -1},
        {3, 8, 7, 7, 8, 5, 7, 5, 2, 7, 2, 4},
        {5, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1},
        {7, 8, 4, 4, 8, 1, 4, 1, 5, 4, 5, 6},
        {3, 1, 4, 4, 1, 5, 4, 5, 6, -1, -1, -1},
        {6, 8, 1, 6, 1, 2, -1, -1, -1, -1, -1, -1},
        {1, 2, 3, 3, 2, 7, 7, 2, 6, -1, -1, -1},
        {7, 8, 4, 4, 8, 1, 4, 1, 2, -1, -1, -1},
        {3, 1, 4, 4, 1, 2, -1, -1, -1, -1, -1, -1}
    };


    private void Start()
    {
        binaryVals = new int[size + 1, size + 1];
        VertexValues = new float[size + 1, size + 1];

        for (int x1 = 0; x1 <= size; x1++)
        {
            for (int y1 = 0; y1 <= size; y1++)
            {
                binaryVals[x1, y1] = noise((float)x1, (float)y1) > (scale / 2) ? 1 : 0;
                VertexValues[x1, y1] = noise((float)x1, (float)y1);
            }
        }
        float noise(float x, float y) => Mathf.PerlinNoise(x / scale, y / scale) * scale;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int g = binaryVals[x, y + 1] * 8 + binaryVals[x + 1, y + 1] * 4 + binaryVals[x + 1, y] * 2 + binaryVals[x, y];

                for (int i = 1; i <= 4; i++)
                {
                    Vector3 a = TriangulationTable[g, i * 3 - 3] != -1 ? point(x, y, TriangulationTable[g, i * 3 - 3] - 1) : Vector3.zero;
                    Vector3 b = TriangulationTable[g, i * 3 - 2] != -1 ? point(x, y, TriangulationTable[g, i * 3 - 2] - 1) : Vector3.zero;
                    Vector3 c = TriangulationTable[g, i * 3 - 1] != -1 ? point(x, y, TriangulationTable[g, i * 3 - 1] - 1) : Vector3.zero;

                    if (a != Vector3.zero || b != Vector3.zero || c != Vector3.zero)
                    {
                        tris.AddRange(new List<int>() { Verts.Count, Verts.Count + 1, Verts.Count + 2 });
                        Verts.AddRange(new List<Vector3>() { a, b, c });
                    }
                }
            }
        }

        GetComponent<MeshFilter>().mesh = new Mesh()
        {
            vertices = Verts.ToArray(),
            triangles = tris.ToArray()
        };

        var welder = new MeshWelder(GetComponent<MeshFilter>().mesh);
        welder.Weld();
    }

    Vector3 point(int x, int y, int i)
    {
        float[] Edge = new float[] { VertexValues[x, y + 1], VertexValues[x + 1, y + 1], VertexValues[x + 1, y], VertexValues[x, y] };

        switch (i)
        {
            case 0: return new Vector3(x - 0.5f, 0, y + 0.5f);
            case 1: return new Vector3(x + 0.5f, 0, y + 0.5f);
            case 2: return new Vector3(x - 0.5f, 0, y - 0.5f);
            case 3: return new Vector3(x + 0.5f, 0, y - 0.5f);
            case 4: return new Vector3(x - 0.5f + lerp(Edge[0], Edge[1]), 0, y + 0.5f);
            case 5: return new Vector3(x + 0.5f, 0, y + 0.5f - lerp(Edge[1], Edge[2]));
            case 6: return new Vector3(x + 0.5f - lerp(Edge[2], Edge[3]), 0, y - 0.5f);
            case 7: return new Vector3(x - 0.5f, 0, y - 0.5f + lerp(Edge[3], Edge[0]));
            default: return Vector3.zero;
        }

        float lerp(float a, float b) => Mathf.InverseLerp(a, b, scale / 2);
    }
}
