using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonGrid : MonoBehaviour
{
    public int size;

    private List<Vector3> Vecticies = new List<Vector3>();
    private List<int> Triangles = new List<int>();

    public const float outerRadius = 1f;

    public const float innerRadius = outerRadius * 0.866025808f;

    [Range(0, 100)]
    public float noisescale, Falloff, fillpercent;

    public static Vector3[] corners =
    {
        new Vector3(0, 0, outerRadius),
        new Vector3(innerRadius, 0, outerRadius * 0.5f),
        new Vector3(innerRadius, 0, -0.5f * outerRadius),
        new Vector3(0, 0, -outerRadius),
        new Vector3(-innerRadius, 0, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0,0.5f * outerRadius),
    };

    Mesh mesh;

    private void Start()
    {
        generateGrid();

        mesh = new Mesh();
        mesh.vertices = Vecticies.ToArray();
        mesh.triangles = Triangles.ToArray();
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.name = "Hex Grid";
    }

    private void generateGrid()
    {
        int Seed = UnityEngine.Random.Range(0, 999999);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float x1 = (x + y * 0.5f - y / 2) * (innerRadius * 2);
                float z1 = 0;
                float y1 = y * (outerRadius * 1.5f);

                float f = Vector3.Distance(new Vector3(x, 0, y), new Vector3(size / 2, 0, size / 2)) / Falloff;
                float n = (((Unity.Mathematics.noise.snoise(new Unity.Mathematics.float2(x + Seed, y + Seed) / noisescale)) + 1) / 2)
                     + (((Unity.Mathematics.noise.snoise(new Unity.Mathematics.float2((x + Seed) * 6f, (y + Seed) * 6) / (noisescale * 2))) + 1) / 2);
                float a = ((100 - (float)fillpercent) / 100);

                float b = n - f;

                if (b > a)
                {
                    addHexagon(corners[0] + new Vector3(x1, z1, y1)
                             , corners[1] + new Vector3(x1, z1, y1)
                             , corners[2] + new Vector3(x1, z1, y1)
                             , corners[3] + new Vector3(x1, z1, y1)
                             , corners[4] + new Vector3(x1, z1, y1)
                             , corners[5] + new Vector3(x1, z1, y1));
                }
            }
        }
    }

    private void addHexagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e, Vector3 f)
    {
        int index = Vecticies.Count;
        Vecticies.Add(a);
        Vecticies.Add(b);
        Vecticies.Add(c);
        Vecticies.Add(d);
        Vecticies.Add(e);
        Vecticies.Add(f);
        Triangles.Add(index);
        Triangles.Add(index + 1);
        Triangles.Add(index + 2);
        Triangles.Add(index);
        Triangles.Add(index + 2);
        Triangles.Add(index + 3);
        Triangles.Add(index);
        Triangles.Add(index + 3);
        Triangles.Add(index + 4);
        Triangles.Add(index);
        Triangles.Add(index + 4);
        Triangles.Add(index + 5);
    }
}
