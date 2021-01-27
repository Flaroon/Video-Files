using UnityEngine;

public class Water : MonoBehaviour
{
    Vector3[] verticies;
    int[] triangles;
    private Vector2[] uvs;
    [Range(0, 256)] public int Size;
    [Range(1, 8)] public int octaves;
    public float height, speed, frequency;
    float off = .0f;

    private void Update()
    {
        verticies = new Vector3[(Size + 1) * (Size + 1)];
        triangles = new int[Size * Size * 6];
        uvs = new Vector2[(Size + 1) * (Size + 1)];

        for (int x = 0, a = 0; x <= Size; x++)
        {
            for (int y = 0; y <= Size; y++, a++)
            {
                float z = 0.0f;
                for (int i = 0; i < octaves; i++)
                {
                    z += Mathf.PerlinNoise(((float)x * frequency / 10 + off) * i, ((float)y * frequency / 10 + off) * i) * height * i;
                }
                verticies[a] = new Vector3(x, z, y);
                uvs[a] = new Vector2((float)x / (float)Size, (float)y / (float)Size);
            }
        }
        off += speed / 100;
        for (int z = 0, vert = 0, tris = 0; z < Size; z++, vert++)
        {
            for (int x = 0; x < Size; x++, vert++, tris+=6)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + 1 + Size;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + 1 + Size;
                triangles[tris + 5] = vert + 2 + Size;

            }
        }

        GetComponent<MeshFilter>().mesh = new Mesh()
        {
            vertices = verticies,
            triangles = triangles,
            uv = uvs
        };
    }
}
