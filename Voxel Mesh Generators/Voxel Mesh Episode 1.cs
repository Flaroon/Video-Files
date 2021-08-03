using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class SimplifiedScript : MonoBehaviour
{

    public Vector3Int Dimentions;
    public float NoiseScale;
    byte[,,] Voxels;

    private void Awake()
    {
        Voxels = new byte[Dimentions.x, Dimentions.y, Dimentions.z];
        GenerateNoise();
        GenerateMesh();
    }

    private void GenerateNoise()
    {
        for (int x = 0; x < Dimentions.x; x++){
            for (int y = 0; y < Dimentions.y; y++){
                for (int z = 0; z < Dimentions.z; z++){
                    Voxels[x, y, z] = noise.snoise(new float3((float)x, (float)y, (float)z) / NoiseScale) >= 0 ? (byte)1 : (byte)0;
                }
            }
        }
    }

    private void GenerateMesh()
    {
        float at = Time.realtimeSinceStartup;
        List<int> Triangles = new List<int>();
        List<Vector3> Verticies = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();

        for (int x = 1; x < Dimentions.x - 1; x++)
            for (int y = 1; y < Dimentions.y - 1; y++)
                for (int z = 1; z < Dimentions.z - 1; z++)
                {
                    Vector3[] VertPos = new Vector3[8]{
                        new Vector3(-1, 1, -1), new Vector3(-1, 1, 1),
                        new Vector3(1, 1, 1), new Vector3(1, 1, -1),
                        new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),
                        new Vector3(1, -1, 1), new Vector3(1, -1, -1),
                    };

                    int[,] Faces = new int[6, 9]{
                        {0, 1, 2, 3, 0, 1, 0, 0, 0},     //top
                        {7, 6, 5, 4, 0, -1, 0, 1, 0},   //bottom
                        {2, 1, 5, 6, 0, 0, 1, 1, 1},     //right
                        {0, 3, 7, 4, 0, 0, -1,  1, 1},   //left
                        {3, 2, 6, 7, 1, 0, 0,  1, 1},    //front
                        {1, 0, 4, 5, -1, 0, 0,  1, 1}    //back
                    };

                    if (Voxels[x, y, z] == 1)
                        for (int o = 0; o < 6; o++)
                            if (Voxels[x + Faces[o, 4], y + Faces[o, 5], z + Faces[o, 6]] == 0)
                                AddQuad(o, Verticies.Count);

                    void AddQuad(int facenum, int v)
                    {
                        // Add Mesh
                        for (int i = 0; i < 4; i++) Verticies.Add(new Vector3(x, y, z) + VertPos[Faces[facenum, i]] / 2f);
                        Triangles.AddRange(new List<int>() { v, v + 1, v + 2, v, v + 2, v + 3 });

                        // Add uvs
                        Vector2 bottomleft = new Vector2(Faces[facenum, 7], Faces[facenum, 8]) / 2f;

                        uv.AddRange(new List<Vector2>() { bottomleft + new Vector2(0, 0.5f), bottomleft + new Vector2(0.5f, 0.5f), bottomleft + new Vector2(0.5f, 0), bottomleft });
                    }
                }

        GetComponent<MeshFilter>().mesh = new Mesh()
        {
            vertices = Verticies.ToArray(),
            triangles = Triangles.ToArray(),
            uv = uv.ToArray()
        };

    }
}
