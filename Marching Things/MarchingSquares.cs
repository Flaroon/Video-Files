using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class MarchingSquares : MonoBehaviour
{

    // To get Mathematics to work you will need to type this com.unity.mathematics in to the Package manager

    [Range(0, 100)]
    public int Size, FillPercent, NoiseScale;
    int[] BinaryVals, Base10Nums;
    [Range(0, 150), Tooltip("This Will Set The Size Of The Circle That Your island Is On")]
    public float Falloff;
    [Header("")]
    public bool AutoUpdate;
    public bool InvertMesh;
    [Range(0, 100000)]
    public int Seed;

    //  Mesh


    private List<Vector3> Verticies = new List<Vector3>();
    private List<int> Triangles = new List<int>();
    Mesh mesh;


    public void GetMainMeshValues()
    {

        BinaryVals = new int[(Size + 1) * (Size + 1)];
        Base10Nums = new int[Size * Size];
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                float l = Vector3.Distance(new Vector3(x, 0, y), new Vector3(Size / 2, 0, Size / 2)) / Falloff;
                float z = (((Unity.Mathematics.noise.snoise(new Unity.Mathematics.float2(x + Seed, y + Seed) / NoiseScale) + 1) / 2)
                     + ((Unity.Mathematics.noise.snoise(new Unity.Mathematics.float2((x + Seed) * 6, (y + Seed) * 6) / (NoiseScale * 2)) + 1) / 2)) / 2;
                float a = ((100 - (float)FillPercent) / 100);

                float b = z - l;
                if (InvertMesh)
                {
                    if (b < a)
                        BinaryVals[(int)x * Size + (int)y] = 1;
                    else
                        BinaryVals[(int)x * Size + (int)y] = 0;
                }
                else
                {
                    if (a < b)
                        BinaryVals[(int)x * Size + (int)y] = 1;
                    else
                        BinaryVals[(int)x * Size + (int)y] = 0;
                }
            }
        }

        GenerateGridValues();

        // Mesh

        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = Verticies.ToArray();
        mesh.triangles = Triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.name = "Marching Squares Mesh";
    }
    public void Update()
    {
        if (AutoUpdate)
            GenerateMesh();
    }

    public void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.Clear();
        Verticies.Clear();
        Triangles.Clear();
        GetMainMeshValues();
    }
    public void CompleteClear()
    {
        mesh.Clear();
        Verticies.Clear();
        Triangles.Clear();
    }
    public void GenerateGridValues()
    {
        for (int x = 0, i = 0; x < (Size); x++)
        {
            for (int y = 0; y < (Size); y++)
            {
                int z = ConvertToDec(
                    BinaryVals[i],
                    BinaryVals[i + Size],
                    BinaryVals[i + Size + 1],
                    BinaryVals[i + 1]
                    );
                Base10Nums[i] = z;
                AddMarchedSquare(x, y, z);
                i++;
            }
        }
    }

    public int ConvertToDec(int a, int b, int c, int d)
    {
        int e = (a * 8) + (b * 4) + (c * 2) + (d);
        return e;
    }
    private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        int vertexIndex = Verticies.Count;
        Verticies.Add(a);
        Verticies.Add(b);
        Verticies.Add(c);
        Triangles.Add(vertexIndex);
        Triangles.Add(vertexIndex + 1);
        Triangles.Add(vertexIndex + 2);
    }
    private void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int vertexIndex = Verticies.Count;
        Verticies.Add(a);
        Verticies.Add(b);
        Verticies.Add(c);
        Verticies.Add(d);
        Triangles.Add(vertexIndex);
        Triangles.Add(vertexIndex + 1);
        Triangles.Add(vertexIndex + 2);
        Triangles.Add(vertexIndex);
        Triangles.Add(vertexIndex + 2);
        Triangles.Add(vertexIndex + 3);
    }
    private void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
    {
        int vertexIndex = Verticies.Count;
        Verticies.Add(a);
        Verticies.Add(b);
        Verticies.Add(c);
        Verticies.Add(d);
        Verticies.Add(e);
        Triangles.Add(vertexIndex);
        Triangles.Add(vertexIndex + 1);
        Triangles.Add(vertexIndex + 2);
        Triangles.Add(vertexIndex);
        Triangles.Add(vertexIndex + 2);
        Triangles.Add(vertexIndex + 3);
        Triangles.Add(vertexIndex);
        Triangles.Add(vertexIndex + 3);
        Triangles.Add(vertexIndex + 4);
    }

    public void AddMarchedSquare(int x, int y, int i)
    {
        if (i == 0)
            return;
        if (i == 1)
            AddTriangle(new Vector3(x, 0, y + 0.5f), new Vector3(x, 0, y + 1), new Vector3(x + 0.5f, 0, y + 1));
        if (i == 2)
            AddTriangle(new Vector3(x + 0.5f, 0, y + 1), new Vector3(x + 1, 0, y + 1), new Vector3(x + 1, 0, y + 0.5f));
        if (i == 3)
            AddQuad(new Vector3(x, 0, y + 0.5f), new Vector3(x, 0, y + 1), new Vector3(x + 1, 0, y + 1), new Vector3(x + 1, 0, y + 0.5f));
        if (i == 4)
            AddTriangle(new Vector3(x + 0.5f, 0, y), new Vector3(x + 1, 0, y + 0.5f), new Vector3(x + 1, 0, y));
        if (i == 5)
        {
            AddQuad(new Vector3(x, 0, y + 1), new Vector3(x + 0.5f, 0, y + 1), new Vector3(x + 1, 0, y + 0.5f), new Vector3(x + 1, 0, y));
            AddQuad(new Vector3(x, 0, y + 1), new Vector3(x + 1, 0, y), new Vector3(x + 0.5f, 0, y), new Vector3(x, 0, y + 0.5f));
        }
        if (i == 6)
            AddQuad(new Vector3(x + 0.5f, 0, y), new Vector3(x + 0.5f, 0, y + 1), new Vector3(x + 1, 0, y + 1), new Vector3(x + 1, 0, y));
        if (i == 7)
            AddPentagon(new Vector3(x, 0, y + 0.5f), new Vector3(x, 0, y + 1), new Vector3(x + 1, 0, y + 1), new Vector3(x + 1, 0, y), new Vector3(x + 0.5f, 0, y));
        if (i == 8)
            AddTriangle(new Vector3(x, 0, y), new Vector3(x, 0, y + 0.5f), new Vector3(x + 0.5f, 0, y));
        if (i == 9)
            AddQuad(new Vector3(x, 0, y), new Vector3(x, 0, y + 1), new Vector3(x + 0.5f, 0, y + 1), new Vector3(x + 0.5f, 0, y));
        if (i == 10)
        {
            AddQuad(new Vector3(x, 0, y), new Vector3(x, 0, y + 0.5f), new Vector3(x + 0.5f, 0, y + 1), new Vector3(x + 1, 0, y + 1));
            AddQuad(new Vector3(x, 0, y), new Vector3(x + 1, 0, y + 1), new Vector3(x + 1, 0, y + 0.5f), new Vector3(x + 0.5f, 0, y));
        }
        if (i == 11)
            AddPentagon(new Vector3(x, 0, y), new Vector3(x, 0, y + 1), new Vector3(x + 1, 0, y + 1), new Vector3(x + 1, 0, y + 0.5f), new Vector3(x + 0.5f, 0, y));
        if (i == 12)
            AddQuad(new Vector3(x, 0, y), new Vector3(x, 0, y + 0.5f), new Vector3(x + 1, 0, y + 0.5f), new Vector3(x + 1, 0, y));
        if (i == 13)
            AddPentagon(new Vector3(x, 0, y), new Vector3(x, 0, y + 1), new Vector3(x + 0.5f, 0, y + 1), new Vector3(x + 1, 0, y + 0.5f), new Vector3(x + 1, 0, y));
        if (i == 14)
            AddPentagon(new Vector3(x, 0, y), new Vector3(x, 0, y + 0.5f), new Vector3(x + 0.5f, 0, y + 1), new Vector3(x + 1, 0, y + 1), new Vector3(x + 1, 0, y));
        if (i == 15)
            AddQuad(new Vector3(x, 0, y), new Vector3(x, 0, y + 1), new Vector3(x + 1, 0, y + 1), new Vector3(x + 1, 0, y));
    }
}

[CustomEditor(typeof(MarchingSquares))]
public class MarchingSquaresEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MarchingSquares March = (MarchingSquares)target;

        if (!March.AutoUpdate)
        {
            if (GUILayout.Button("Generate Mesh"))
                March.GenerateMesh();
            if (GUILayout.Button("Clear Mesh"))
                March.CompleteClear();
        }
    }
}